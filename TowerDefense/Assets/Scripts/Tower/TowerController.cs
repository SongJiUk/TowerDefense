using UnityEngine;

/// <summary>
/// 배치된 타워 하나의 타겟팅·공격·업그레이드를 담당.
/// TowerPlacer.HandleTowerSelected()에서 Init(data)로 초기화된다.
/// 매 프레임 공격 타이머를 감소시키고, 사거리 내 적을 탐색해 투사체를 발사한다.
/// </summary>
public class TowerController : MonoBehaviour
{
    /// <summary>총구 위치. 없으면 자신 위치 + up에서 발사.</summary>
    [SerializeField] private Transform _firePoint;

    // ─── 상태 ─────────────────────────────────────────────────────────────────

    public TowerData Data         { get; private set; }

    /// <summary>현재 레벨. 0 = 기본, 1 이상 = upgradeSteps[level-1] 적용.</summary>
    public int       CurrentLevel { get; private set; } = 0;

    private float _currentDamage;
    private float _currentAttackSpeed;
    private float _currentRange;

    private float     _attackTimer;
    private Transform _currentTarget;

    private static int _enemyMask;
    private RangeIndicator _rangeIndicator;
    
    private bool _isRangeVisible;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void Awake()
    {
        _enemyMask      = LayerMask.GetMask("Enemy");
        _rangeIndicator = GetComponentInChildren<RangeIndicator>();
    }


    void OnMouseDown()
    {
        if (Data == null) return;

        _isRangeVisible = !_isRangeVisible;

        if (_isRangeVisible)
            _rangeIndicator?.Show(transform.position, _currentRange);
        else
            _rangeIndicator?.Hide();
    }

    void Update()
    {
        if (Data == null) return;

        _attackTimer -= Time.deltaTime;
        if (_attackTimer > 0f) return;

        _currentTarget = FindTarget();
        if (_currentTarget == null) return;

        Fire(_currentTarget);
        _attackTimer = 1f / _currentAttackSpeed;
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    /// <summary>
    /// TowerPlacer가 타워를 설치할 때 호출. 데이터를 주입하고 스탯을 계산한다.
    /// </summary>
    public void Init(TowerData data)
    {
        Data         = data;
        CurrentLevel = 0;
        ApplyStats();
    }

    // ─── 업그레이드 ───────────────────────────────────────────────────────────

    /// <summary>
    /// 업그레이드 시도. 골드가 부족하거나 최대 레벨이면 false 반환.
    /// 성공 시 CurrentLevel 증가 후 ApplyStats()로 스탯을 재계산한다.
    /// </summary>
    public bool TryUpgrade()
    {
        if (Data.upgradeSteps == null) return false;
        if (CurrentLevel >= Data.upgradeSteps.Length) return false;

        int cost = Data.upgradeSteps[CurrentLevel].upgradeCost;
        if (!Managers.SpendGold(cost)) return false;

        CurrentLevel++;
        ApplyStats();
        return true;
    }

    // ─── 내부 로직 ────────────────────────────────────────────────────────────

    /// <summary>
    /// 현재 레벨에 따라 데미지·공격속도·사거리를 계산해 적용.
    /// 레벨 0이면 baseStats 그대로, 이상이면 upgradeSteps의 배율 적용.
    /// </summary>
    private void ApplyStats()
    {
        _currentDamage      = Data.baseDamage;
        _currentAttackSpeed = Data.baseAttackSpeed;
        _currentRange       = Data.baseRange;

        if (CurrentLevel > 0 && Data.upgradeSteps != null
            && CurrentLevel <= Data.upgradeSteps.Length)
        {
            TowerUpgradeStep step = Data.upgradeSteps[CurrentLevel - 1];
            _currentDamage      *= step.damageMultiplier;
            _currentAttackSpeed *= step.attackSpeedMultiplier;
            _currentRange       += step.rangeBonus;
        }
    }

    /// <summary>
    /// 사거리 내 Enemy 레이어 오브젝트를 탐색해 코어에 가장 가까운 적을 반환.
    /// 코어에 가장 가까운 적 = 경로를 가장 많이 진행한 적 = 가장 위험한 적.
    /// </summary>
    private Transform FindTarget()
    {
        // Y축 무시: 수직 캡슐로 XZ 수평 거리만 체크
        Vector3 bottom = new Vector3(transform.position.x, -50f, transform.position.z);
        Vector3 top    = new Vector3(transform.position.x,  50f, transform.position.z);
        Collider[] hits = Physics.OverlapCapsule(bottom, top, _currentRange, _enemyMask, QueryTriggerInteraction.Collide);
        if (hits.Length == 0) return null;

        Transform best = null;
        float minDist = float.MaxValue;
        Vector3 corePos = Managers.CoreTransform != null
                            ? Managers.CoreTransform.position
                            : Vector3.zero;

        foreach (Collider col in hits)
        {
            float dist = Vector3.Distance(col.transform.position, corePos);
            if (dist < minDist)
            {
                minDist = dist;
                best = col.transform;
            }
        }

        return best;
    }

    /// <summary>
    /// _firePoint 위치에서 투사체를 스폰하고 타겟·데미지·속도를 전달한다.
    /// projectilePrefab이 null이면 아무것도 하지 않는다.
    /// </summary>
    private void Fire(Transform target)
    {
        if (Data.projectilePrefab == null) return;

        Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 2f;
        GameObject go = Managers.PoolM.Pop(Data.projectilePrefab);
        go.transform.position = spawnPos;

        go.GetComponent<ProjectileController>()?.Init(target, _currentDamage, Data.projectileSpeed);
    }

#if UNITY_EDITOR
    /// <summary>씬 뷰에서 타워 선택 시 사거리를 파란 원으로 표시.</summary>
    void OnDrawGizmosSelected()
    {
        if (Data == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _currentRange);
    }
#endif
}
