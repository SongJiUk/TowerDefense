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
    [SerializeField] private GameObject _turnObject;

    // ─── 상태 ─────────────────────────────────────────────────────────────────

    public TowerData Data { get; private set; }

    /// <summary>현재 레벨. 0 = 기본, 1 이상 = upgradeSteps[level-1] 적용.</summary>
    public int CurrentLevel { get; private set; } = 0;

    private float _baseDamage;
    protected float _currentDamage;
    private float _baseAttackSpeed;
    private float _currentAttackSpeed;
    private float _baseRange;
    private float _currentRange;

    private float _attackTimer;
    private Transform _currentTarget;

    protected static int _enemyMask;
    private static TowerController _selectedTower;
    private RangeIndicator _rangeIndicator;


    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void Awake()
    {
        _enemyMask = LayerMask.GetMask("Enemy");
        _rangeIndicator = GetComponentInChildren<RangeIndicator>();
    }

    void OnEnable()
    {
        if (Managers.GameM != null)
            Managers.GameM.OnCardApplied += ApplyStats;
    }

    void OnDisable()
    {
        if (Managers.GameM != null)
            Managers.GameM.OnCardApplied -= ApplyStats;
    }

    void OnMouseUp()
    {
        if (CameraController.IsDragging) return;
#if UNITY_EDITOR || UNITY_STANDALONE
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
#else
        if (Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
#endif
        if (Data == null) return;

        // 다른 타워 선택 시 기존 범위 숨김
        if (_selectedTower != null && _selectedTower != this)
            _selectedTower.HideRange();

        if (_selectedTower == this)
        {
            HideRange();
            _selectedTower = null;
        }
        else
        {
            _selectedTower = this;
            _rangeIndicator?.Show(transform.position, _currentRange);
        }
    }

    void Update()
    {
        if (Data == null) return;

        _attackTimer -= Time.deltaTime;

        if (_currentTarget == null)
        {
            _currentTarget = FindTarget();
        }
        else
        {
            float xzDist = Vector2.Distance(
                new Vector2(_currentTarget.position.x, _currentTarget.position.z),
                new Vector2(transform.position.x, transform.position.z));

            if (!_currentTarget.gameObject.activeInHierarchy || xzDist > _currentRange)
            {
                _currentTarget = null;
                return;
            }

            var dir = _currentTarget.transform.position - _turnObject.transform.position;
            dir.y = 0;
            _turnObject.transform.rotation = Quaternion.RotateTowards(
                _turnObject.transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 360f);
        }


        if (_attackTimer > 0f) return;
        if (_currentTarget == null) return;

        float distBeforeFire = Vector2.Distance(
            new Vector2(_currentTarget.position.x, _currentTarget.position.z),
            new Vector2(transform.position.x, transform.position.z));
        if (distBeforeFire > _currentRange)
        {
            _currentTarget = null;
            return;
        }

        Fire(_currentTarget);
        _attackTimer = 1f / _currentAttackSpeed;
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    /// <summary>
    /// TowerPlacer가 타워를 설치할 때 호출. 데이터를 주입하고 스탯을 계산한다.
    /// </summary>
    public virtual void Init(TowerData data)
    {
        Data = data;
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
        if (!Managers.GameM.SpendGold(cost)) return false;

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
        _baseDamage = Data.baseDamage;
        _baseAttackSpeed = Data.baseAttackSpeed;
        _baseRange = Data.baseRange;

        if (CurrentLevel > 0 && Data.upgradeSteps != null
            && CurrentLevel <= Data.upgradeSteps.Length)
        {
            TowerUpgradeStep step = Data.upgradeSteps[CurrentLevel - 1];
            _baseDamage *= step.damageMultiplier;
            _baseAttackSpeed *= step.attackSpeedMultiplier;
            _baseRange += step.rangeBonus;
        }

        _currentDamage = _baseDamage * Managers.GameM.globalDamageMultiplier;
        _currentAttackSpeed = _baseAttackSpeed * Managers.GameM.globalAttackSpeedMultiplier;
        _currentRange = _baseRange + Managers.GameM.globalRangeBonus;
    }

    public void HideRange()
    {
        _rangeIndicator?.Hide();
    }

    public static void HideSelectedRange()
    {
        if (_selectedTower == null) return;
        _selectedTower.HideRange();
        _selectedTower = null;
    }

    /// <summary>
    /// 사거리 내 Enemy 레이어 오브젝트를 탐색해 코어에 가장 가까운 적을 반환.
    /// 코어에 가장 가까운 적 = 경로를 가장 많이 진행한 적 = 가장 위험한 적.
    /// </summary>
    private Transform FindTarget()
    {
        // Y축 무시: 수직 캡슐로 XZ 수평 거리만 체크
        Vector3 bottom = new Vector3(transform.position.x, -50f, transform.position.z);
        Vector3 top = new Vector3(transform.position.x, 50f, transform.position.z);
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
        if (Data.projectilePrefabKey == null) return;

        Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 2f;
        GameObject go = Managers.PoolM.Pop(Data.projectilePrefabKey);
        go.transform.position = spawnPos;

        go.GetComponent<ProjectileController>()?.Init(target, _currentDamage, Data.projectileSpeed, onHit: OnHit);
    }

    protected virtual void OnHit(Transform target) { }

#if UNITY_EDITOR
    /// <summary>씬 뷰에서 타워 선택 시 사거리를 파란 원으로 표시.</summary>
    protected virtual void OnDrawGizmosSelected()
    {
        if (Data == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _currentRange);
    }
#endif
}
