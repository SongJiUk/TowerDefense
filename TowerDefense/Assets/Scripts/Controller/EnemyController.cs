using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 적 하나의 이동·피격·사망을 담당.
/// WaveManager.SpawnEnemy()에서 Init()으로 초기화되고 ObjectPool로 재사용된다.
/// PathFinder.OnPathChanged 이벤트를 구독해 타워 설치 시 실시간으로 경로를 재계산한다.
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable
{
    protected EnemyHPBar _hpBar;
    protected GameObject _hpBarGo;

    protected EnemyData _data;
    protected float _hp;
    protected float _maxHp;
    public float CurrentHp => _hp;
    protected float _baseSpeed;
    protected float _speed;
    protected bool _isDead;
    protected BuffHandler _buffHandler;

    protected float _storedHpMult = 1f;
    protected float _storedSpeedMult = 1f;

    private List<Vector3> _path;
    private CancellationTokenSource _cts;

    /// <summary>
    /// 현재 이동 중인 목표 웨이포인트.
    /// OnPathChanged 시 transform.position이 아닌 이 값에서 재탐색해 역방향 이동을 방지.
    /// </summary>
    private Vector3 _currentTarget;

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public virtual void Init(EnemyData data, float hpMultiplier = 1f, float speedMultiplier = 1f)
    {
        _data = data;
        _storedHpMult = hpMultiplier;
        _storedSpeedMult = speedMultiplier;
        float diffHp = Managers.DifficultyM?.EnemyHpMultiplier ?? 1f;
        float diffSpeed = Managers.DifficultyM?.EnemySpeedMultiplier ?? 1f;
        _maxHp = data.baseHp * hpMultiplier * Managers.GameM.nextWaveEnemyHpMultiplier * diffHp;
        _hp = _maxHp;
        _baseSpeed = data.baseMoveSpeed * speedMultiplier * diffSpeed;
        _speed = _baseSpeed;
        _currentTarget = transform.position;
        _isDead = false;
        _hpBar?.SetHP(_hp, _maxHp);
        RequestPath();
    }

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    protected virtual void Awake()
    {
        if (_buffHandler == null) _buffHandler = GetComponent<BuffHandler>();
    }

    void OnEnable()
    {
        Managers.Path.OnPathChanged += OnPathChanged;
        if (_buffHandler != null) _buffHandler.OnModifiersChanged += RecalculateSpeed;

        _hpBarGo = Managers.ResourceM.Instantiate("EnemyHPBar", null, true);
        if (_hpBarGo != null)
        {
            _hpBar = _hpBarGo.GetComponent<EnemyHPBar>();
            _hpBar.Follow(transform, new Vector3(0f, 2f, 0f));
        }
    }

    void OnDisable()
    {
        Managers.Path.OnPathChanged -= OnPathChanged;
        if (_buffHandler != null) _buffHandler.OnModifiersChanged -= RecalculateSpeed;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        if (_hpBarGo != null)
        {
            Managers.ResourceM.Destroy(_hpBarGo);
            _hpBarGo = null;
            _hpBar = null;
        }
    }

    // ─── 전투 ─────────────────────────────────────────────────────────────────

    public virtual void TakeDamage(float damage)
    {
        if (_isDead) return;
        _hp -= damage;
        _hpBar?.SetHP(_hp, _maxHp);
        if (_hp <= 0f) Die();
    }

    public void Heal(float amount) { }

    protected virtual void Die()
    {
        _isDead = true;
        Managers.WaveM.OnEnemyRemoved();
        Managers.GameM.AddGold(Mathf.RoundToInt(_data.baseReward * Managers.GameM.killRewardMultiplier));
        Managers.GameM.AddExp(_data.rewardExp);
        Managers.ResourceM.Destroy(gameObject);
    }

    // ─── 경로 이동 ────────────────────────────────────────────────────────────

    private void OnPathChanged()
    {
        List<Vector3> newPath = Managers.Path.FindPath(
            _currentTarget,
            Managers.EndPoint.transform.position
        );
        StartMove(newPath);
    }

    private void RequestPath()
    {
        if (_data == null || Managers.CoreTransform == null) return;

        if (Managers.SpawnPoint != null)
            transform.position = new Vector3(transform.position.x, Managers.SpawnPoint.transform.position.y, transform.position.z);

        List<Vector3> path = Managers.Path.FindPath(
            transform.position,
            Managers.EndPoint.transform.position
        );
        StartMove(path);
    }

    private void StartMove(List<Vector3> newPath)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _path = newPath;
        MoveAlongPath(_cts.Token).Forget();
    }

    private async UniTaskVoid MoveAlongPath(CancellationToken token)
    {
        if (_path == null || _path.Count == 0) return;

        foreach (Vector3 waypoint in _path)
        {
            _currentTarget = waypoint;
            Vector3 target = waypoint;

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                if (token.IsCancellationRequested) return;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    _speed * Time.deltaTime
                );

                var dir = target - transform.position;
                dir.y = 0;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        OnReachCore();
    }

    private void OnReachCore()
    {
        if (_isDead) return;
        _isDead = true;
        Managers.ICore?.TakeDamage(_data.coreDamage);
        Managers.WaveM.OnEnemyRemoved();
        Managers.ResourceM.Destroy(gameObject);
    }

    // ─── 스탯 변경 ────────────────────────────────────────────────────────────

    public void RecalculateSpeed()
    {
        _speed = _buffHandler.GetStat(Define.StatType.Speed, _baseSpeed);
    }
}
