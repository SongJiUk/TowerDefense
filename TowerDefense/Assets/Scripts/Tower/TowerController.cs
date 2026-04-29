using UnityEngine;

public class TowerController : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _turnObject;

    [Header("업그레이드 이펙트 (점수 1~3 / 4~6 / 7~9)")]
    [SerializeField] private GameObject _effectLow;
    [SerializeField] private GameObject _effectMid;
    [SerializeField] private GameObject _effectHigh;

    // ─── 상태 ─────────────────────────────────────────────────────────────────

    public TowerData Data { get; private set; }

    public int DamageLevel { get; private set; }
    public int RangeLevel  { get; private set; }
    public int SpeedLevel  { get; private set; }

    private float _currentDamage;
    private float _currentAttackSpeed;
    private float _currentRange;

    public float CurrentRange  => _currentRange;
    public float CurrentDamage => _currentDamage;
    public float CurrentSpeed  => _currentAttackSpeed;

    private float _attackTimer;
    private Transform _currentTarget;
    private EnemyController _currentTargetEnemy;

    protected static int _enemyMask;
    private static TowerController _selectedTower;
    private static UI_TowerUpgradePopup _upgradePopup;

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
        if (Managers.SynergyM != null)
            Managers.SynergyM.OnSynergyChanged += ApplyStats;
    }

    void OnDisable()
    {
        if (Managers.GameM != null)
            Managers.GameM.OnCardApplied -= ApplyStats;
        if (Managers.SynergyM != null)
        {
            Managers.SynergyM.OnSynergyChanged -= ApplyStats;
            if (Data != null) Managers.SynergyM.Unregister(Data.towerType);
        }

        if (_selectedTower == this)
        {
            HideUpgradePopupInternal();
            _selectedTower = null;
        }
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

        if (_selectedTower != null && _selectedTower != this)
        {
            _selectedTower.HideRange();
            HideUpgradePopupInternal();
        }

        if (_selectedTower == this)
        {
            HideRange();
            HideUpgradePopupInternal();
            _selectedTower = null;
        }
        else
        {
            _selectedTower = this;
            _rangeIndicator?.Show(transform.position, _currentRange);
            ShowUpgradePopup();
        }
    }

    void Update()
    {
        if (Data == null) return;

        _attackTimer -= Time.deltaTime;

        if (_currentTarget == null)
        {
            _currentTarget = FindTarget();
            _currentTargetEnemy = _currentTarget?.GetComponent<EnemyController>();
        }
        else
        {
            float xzDist = Vector2.Distance(
                new Vector2(_currentTarget.position.x, _currentTarget.position.z),
                new Vector2(transform.position.x, transform.position.z));

            if (!_currentTarget.gameObject.activeInHierarchy || _currentTargetEnemy?.IsDead == true || xzDist > _currentRange)
            {
                _currentTarget = null;
                _currentTargetEnemy = null;
                return;
            }

            var dir = _currentTarget.position - _turnObject.transform.position;
            dir.y = 0;
            _turnObject.transform.rotation = Quaternion.RotateTowards(
                _turnObject.transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 360f);
        }

        if (_attackTimer > 0f || _currentTarget == null) return;

        float distBeforeFire = Vector2.Distance(
            new Vector2(_currentTarget.position.x, _currentTarget.position.z),
            new Vector2(transform.position.x, transform.position.z));
        if (distBeforeFire > _currentRange)
        {
            _currentTarget = null;
            _currentTargetEnemy = null;
            return;
        }

        Fire(_currentTarget);
        _attackTimer = 1f / _currentAttackSpeed;
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public virtual void Init(TowerData data)
    {
        Data = data;
        DamageLevel = 0;
        RangeLevel  = 0;
        SpeedLevel  = 0;
        Managers.SynergyM?.Register(data.towerType);
        ApplyStats();
    }

    // ─── 업그레이드 ───────────────────────────────────────────────────────────

    public bool CanUpgrade(Define.UpgradeType type)
    {
        TowerStatUpgrade[] steps = GetUpgradeSteps(type);
        return steps != null && GetLevel(type) < steps.Length;
    }

    public bool TryUpgrade(Define.UpgradeType type)
    {
        TowerStatUpgrade[] steps = GetUpgradeSteps(type);
        int level = GetLevel(type);
        if (steps == null || level >= steps.Length) return false;

        int cost = steps[level].cost;
        if (!Managers.GameM.SpendGold(cost)) return false;

        SetLevel(type, level + 1);
        ApplyStats();
        return true;
    }

    public int GetSellPrice()
    {
        int total = Data.buildCost;
        total += SumSpentCost(Data.damageUpgrades, DamageLevel);
        total += SumSpentCost(Data.rangeUpgrades,  RangeLevel);
        total += SumSpentCost(Data.speedUpgrades,  SpeedLevel);
        return Mathf.RoundToInt(total * 0.8f);
    }

    public void Sell()
    {
        Managers.GameM.AddGold(GetSellPrice());
        Managers.Grid.SetOccupied(transform.position, false);
        HideRange();
        HideUpgradePopupInternal();
        if (_selectedTower == this) _selectedTower = null;
        Managers.ResourceM.Destroy(gameObject);
    }

    // ─── 공개 유틸 ────────────────────────────────────────────────────────────

    public void HideRange() => _rangeIndicator?.Hide();

    public static void HideSelectedRange()
    {
        if (_selectedTower == null) return;
        _selectedTower.HideRange();
        _selectedTower = null;
    }

    // ─── 팝업 관리 (static) ──────────────────────────────────────────────────

    private void ShowUpgradePopup()
    {
        if (_upgradePopup == null)
        {
            GameObject go = Managers.ResourceM.Instantiate("UI_TowerUpgradePopup", _pooling: false);
            go.transform.SetParent(Managers.UIM.Root.transform, false);
            _upgradePopup = go.GetOrAddComponent<UI_TowerUpgradePopup>();
        }
        _upgradePopup.gameObject.SetActive(true);
        _upgradePopup.Show(this);
    }

    private static void HideUpgradePopupInternal()
    {
        if (_upgradePopup != null)
            _upgradePopup.gameObject.SetActive(false);
    }

    // ─── 내부 로직 ────────────────────────────────────────────────────────────

    private void ApplyStats()
    {
        if (Data == null) return;

        float damage = Data.baseDamage;
        float speed  = Data.baseAttackSpeed;
        float range  = Data.baseRange;

        if (DamageLevel > 0 && Data.damageUpgrades != null && DamageLevel <= Data.damageUpgrades.Length)
            damage *= Data.damageUpgrades[DamageLevel - 1].multiplier;

        if (SpeedLevel > 0 && Data.speedUpgrades != null && SpeedLevel <= Data.speedUpgrades.Length)
            speed *= Data.speedUpgrades[SpeedLevel - 1].multiplier;

        if (RangeLevel > 0 && Data.rangeUpgrades != null && RangeLevel <= Data.rangeUpgrades.Length)
            range *= Data.rangeUpgrades[RangeLevel - 1].multiplier;

        if (Managers.SynergyM != null && Managers.SynergyM.FocusFire &&
            (Data.towerType == Define.TowerType.Basic || Data.towerType == Define.TowerType.Sniper))
        {
            float speedMult = 1f + 0.15f * (Managers.GameM?.synergyMultiplier ?? 1f);
            speed *= speedMult;
            Debug.Log($"[Synergy:집중사격] {Data.towerType} 공격속도 {speedMult:F2}배 → {speed:F2}");
        }

        _currentDamage      = damage * Managers.GameM.globalDamageMultiplier;
        _currentAttackSpeed = speed  * Managers.GameM.globalAttackSpeedMultiplier;
        _currentRange       = range  + Managers.GameM.globalRangeBonus;

        UpdateVisualEffect();
    }

    // 세 스탯을 균등하게 올려야 단계 상승 (한 스탯 몰빵 방지)
    public int UniqueEffectStage => Mathf.Min(DamageLevel, RangeLevel, SpeedLevel);

    public virtual string GetUniqueEffectText() => "";

    private void UpdateVisualEffect()
    {
        //int stage = UniqueEffectStage;
        //_effectLow?.SetActive(stage == 1);
        //_effectMid?.SetActive(stage == 2);
        //_effectHigh?.SetActive(stage == 3);
    }

    protected virtual Transform FindTarget()
    {
        Vector3 bottom = new Vector3(transform.position.x, -50f, transform.position.z);
        Vector3 top    = new Vector3(transform.position.x,  50f, transform.position.z);
        Collider[] hits = Physics.OverlapCapsule(bottom, top, _currentRange, _enemyMask, QueryTriggerInteraction.Collide);
        if (hits.Length == 0) return null;

        Transform best = null;
        float minDist = float.MaxValue;
        Vector3 corePos = Managers.CoreTransform != null ? Managers.CoreTransform.position : Vector3.zero;

        foreach (Collider col in hits)
        {
            if (col.GetComponent<EnemyController>()?.IsDead == true) continue;
            float dist = Vector3.Distance(col.transform.position, corePos);
            if (dist < minDist) { minDist = dist; best = col.transform; }
        }
        return best;
    }

    private void Fire(Transform target)
    {
        if (Data.projectilePrefabKey == null) return;

        float damage = _currentDamage;
        bool isCritical = UnityEngine.Random.value < Managers.GameM.criticalChanceBonus + GetBonusCritChance(target);
        if (isCritical) damage *= 2f;

        Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 2f;
        GameObject go = Managers.PoolM.Pop(Data.projectilePrefabKey);
        go.transform.position = spawnPos;
        go.GetComponent<ProjectileController>()?.Init(target, damage, Data.projectileSpeed, isCritical, onHit: OnHit);
    }

    protected virtual void OnHit(Transform target) { }

    protected virtual float GetBonusCritChance(Transform target) => 0f;

    // ─── 헬퍼 ────────────────────────────────────────────────────────────────

    private TowerStatUpgrade[] GetUpgradeSteps(Define.UpgradeType type) => type switch
    {
        Define.UpgradeType.Damage => Data?.damageUpgrades,
        Define.UpgradeType.Range  => Data?.rangeUpgrades,
        Define.UpgradeType.Speed  => Data?.speedUpgrades,
        _ => null
    };

    private int GetLevel(Define.UpgradeType type) => type switch
    {
        Define.UpgradeType.Damage => DamageLevel,
        Define.UpgradeType.Range  => RangeLevel,
        Define.UpgradeType.Speed  => SpeedLevel,
        _ => 0
    };

    private void SetLevel(Define.UpgradeType type, int value)
    {
        switch (type)
        {
            case Define.UpgradeType.Damage: DamageLevel = value; break;
            case Define.UpgradeType.Range:  RangeLevel  = value; break;
            case Define.UpgradeType.Speed:  SpeedLevel  = value; break;
        }
    }

    private static int SumSpentCost(TowerStatUpgrade[] steps, int level)
    {
        if (steps == null) return 0;
        int sum = 0;
        for (int i = 0; i < level && i < steps.Length; i++)
            sum += steps[i].cost;
        return sum;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (Data == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, Data.baseRange);
    }
#endif
}
