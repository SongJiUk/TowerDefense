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
    private Renderer[] _renderers;

    protected static readonly int HASH_DIE = Animator.StringToHash("IsDie");
    protected Animator _animator;

    protected EnemyData _data;
    protected float _hp;
    protected float _maxHp;
    public float CurrentHp => _hp;
    public virtual bool IsDead => _isDead;
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
        _animator = GetComponent<Animator>();
        _buffHandler = GetComponent<BuffHandler>();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    void OnEnable()
    {
        Managers.Path.OnPathChanged += OnPathChanged;
        if (_buffHandler != null) _buffHandler.OnModifiersChanged += RecalculateSpeed;

        _hpBarGo = Managers.ResourceM.Instantiate("EnemyHPBar", null, true);
        if (_hpBarGo != null)
        {
            _hpBar = _hpBarGo.GetComponent<EnemyHPBar>();
            _hpBar.Follow(transform, new Vector3(0f, GetHeadOffset(), 0f));
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

    private float GetHeadOffset()
    {
        if (_renderers == null || _renderers.Length == 0) return 2f;

        Bounds bounds = _renderers[0].bounds;
        for (int i = 1; i < _renderers.Length; i++)
            bounds.Encapsulate(_renderers[i].bounds);

        return bounds.max.y - transform.position.y + 0.3f;
    }

    // ─── 전투 ─────────────────────────────────────────────────────────────────

    public virtual void TakeDamage(float damage, bool isCritical = false, bool isPoison = false)
    {
        if (_isDead) return;
        _hp -= damage;
        _hpBar?.SetHP(_hp, _maxHp);
        if (!isPoison)
            Managers.FloatingTextM?.ShowDamage(transform.position, damage, isCritical);
        if (_hp <= 0f) Die();
    }

    public void Heal(float amount) { }

    protected virtual void Die()
    {
        _isDead = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        DieAsync(destroyCancellationToken).Forget();
    }

    private async UniTaskVoid DieAsync(System.Threading.CancellationToken token)
    {
        await PlayDeathAnimationAsync(token);
        if (token.IsCancellationRequested) return;
        OnDeathComplete();
    }

    protected virtual async UniTask PlayDeathAnimationAsync(System.Threading.CancellationToken token)
    {
        if (_animator == null) return;

        _animator.SetBool(HASH_DIE, true);
        await UniTask.Yield(PlayerLoopTiming.Update, token);

        float elapsed = 0f;
        const float TIMEOUT = 2f;
        while (elapsed < TIMEOUT)
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= 1f && !info.loop) break;
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        _animator.SetBool(HASH_DIE, false);
    }

    protected virtual void OnDeathComplete()
    {
        Managers.WaveM.OnEnemyRemoved();
        Managers.GameM.AddGold(Mathf.RoundToInt(_data.baseReward * Managers.GameM.killRewardMultiplier));
        Managers.GameM.AddExp(_data.rewardExp);
        Managers.GameM.AddKill();
        Managers.ResourceM.Destroy(gameObject);
    }

    // ─── 경로 이동 ────────────────────────────────────────────────────────────

    private void OnPathChanged()
    {
        if (_isDead) return;
        List<Vector3> newPath = Managers.Path.FindPath(
            _currentTarget,
            Managers.EndPoint.transform.position
        );
        StartMove(newPath);
    }

    protected void RequestPath()
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
