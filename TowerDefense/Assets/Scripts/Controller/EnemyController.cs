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

    private EnemyData _data;
    [SerializeField] private float _hp;
    public float CurrentHp => _hp;
    private float _baseSpeed;
    [SerializeField] private float _speed;
    private bool _isDead;
    private BuffHandler _buffHandler;

    private List<Vector3> _path;
    private CancellationTokenSource _cts;

    /// <summary>
    /// 현재 이동 중인 목표 웨이포인트.
    /// OnPathChanged 시 transform.position이 아닌 이 값에서 재탐색해 역방향 이동을 방지.
    /// </summary>
    private Vector3 _currentTarget;

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    /// <summary>
    /// 스폰 직후 WaveManager가 호출. 스탯 설정 후 경로탐색을 시작한다.
    /// </summary>
    /// <param name="hpMultiplier">웨이브 공식으로 계산된 HP 배율</param>
    /// <param name="speedMultiplier">웨이브 공식으로 계산된 속도 배율</param>
    public void Init(EnemyData data, float hpMultiplier = 1f, float speedMultiplier = 1f)
    {
        _data = data;
        _hp = data.baseHp * hpMultiplier * Managers.GameM.nextWaveEnemyHpMultiplier;
        _baseSpeed = data.baseMoveSpeed * speedMultiplier;
        _speed = _baseSpeed;
        _currentTarget = transform.position;
        _isDead = false;
        RequestPath();
    }

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void Awake()
    {
        if (_buffHandler == null) _buffHandler = GetComponent<BuffHandler>();
    }

    void OnEnable()
    {
        Managers.Path.OnPathChanged += OnPathChanged;
        if (_buffHandler != null) _buffHandler.OnModifiersChanged += RecalculateSpeed;
    }

    void OnDisable()
    {
        Managers.Path.OnPathChanged -= OnPathChanged;
        if (_buffHandler != null) _buffHandler.OnModifiersChanged -= RecalculateSpeed;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    // ─── 전투 ─────────────────────────────────────────────────────────────────

    /// <summary>타워의 ProjectileController가 충돌 시 호출. HP가 0 이하면 Die().</summary>
    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        _hp -= damage;
        if (_hp <= 0f) Die();
    }

    public void Heal(float amount)
    {

    }

    /// <summary>
    /// 사망 처리.
    /// 골드 지급 → WaveManager에 제거 알림 → ObjectPool 반환.
    /// </summary>
    private void Die()
    {
        _isDead = true;
        Managers.WaveM.OnEnemyRemoved();
        Managers.GameM.AddGold(Mathf.RoundToInt(_data.baseReward * Managers.GameM.killRewardMultiplier));
        Managers.GameM.AddExp(_data.rewardExp);
        Managers.ResourceM.Destroy(gameObject);
    }

    // ─── 경로 이동 ────────────────────────────────────────────────────────────

    /// <summary>
    /// PathFinder.OnPathChanged 이벤트 수신 시 호출.
    /// transform.position 대신 _currentTarget(다음 웨이포인트)부터 재탐색해
    /// 현재 칸을 통과한 직후 역방향으로 돌아가는 현상을 방지.
    /// </summary>
    private void OnPathChanged()
    {
        List<Vector3> newPath = Managers.Path.FindPath(
            _currentTarget,
            Managers.EndPoint.transform.position
        );
        StartMove(newPath);
    }

    /// <summary>
    /// 스폰 직후 경로 요청. Managers.Core가 null이면 이동하지 않는다.
    /// Core가 Road 타일 위에 없으면 FindPath가 null을 반환하므로 반드시 Road 위에 배치해야 함.
    /// </summary>
    private void RequestPath()
    {
        if (_data == null || Managers.CoreTransform == null) return;

        // 스폰 Y를 SpawnPoint 표면에 맞춤 (땅 박힘 방지)
        if (Managers.SpawnPoint != null)
            transform.position = new Vector3(transform.position.x, Managers.SpawnPoint.transform.position.y, transform.position.z);

        List<Vector3> path = Managers.Path.FindPath(
            transform.position,
            Managers.EndPoint.transform.position
        );
        StartMove(path);
    }

    /// <summary>진행 중인 이동 UniTask를 취소하고 새 경로로 재시작.</summary>
    private void StartMove(List<Vector3> newPath)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _path = newPath;
        MoveAlongPath(_cts.Token).Forget();
    }

    /// <summary>
    /// 경로의 웨이포인트를 순서대로 MoveTowards로 이동.
    /// 각 웨이포인트 도달 직전에 _currentTarget을 업데이트해 경로 재계산 기준점으로 사용.
    /// 전체 경로 완주 시 OnReachCore() 호출.
    /// </summary>
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
                transform.rotation = Quaternion.LookRotation(dir);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        OnReachCore();
    }

    /// <summary>
    /// 코어 도달 처리.
    /// WaveManager에 제거 알림 후 풀 반환.
    /// </summary>
    private void OnReachCore()
    {
        if (_isDead) return;
        _isDead = true;
        Managers.ICore?.TakeDamage(_data.coreDamage);
        Managers.WaveM.OnEnemyRemoved();
        Managers.ResourceM.Destroy(gameObject);
    }

    // ─── 스탯 변경 ─────────────────────────────────────────────────────────────────

    public void RecalculateSpeed()
    {
        _speed = _buffHandler.GetStat(Define.StatType.Speed, _baseSpeed);
    }
}
