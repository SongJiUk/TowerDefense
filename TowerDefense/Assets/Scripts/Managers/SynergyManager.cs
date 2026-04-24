using System;
using System.Collections.Generic;

/// <summary>
/// 필드에 배치된 타워 조합을 추적해 시너지 활성화 여부를 제공.
/// TowerController.Init()에서 Register, OnDisable에서 Unregister 호출.
/// </summary>
public class SynergyManager
{
    private readonly Dictionary<Define.TowerType, int> _counts = new();

    public event Action OnSynergyChanged;

    public void Register(Define.TowerType type)
    {
        _counts.TryGetValue(type, out int n);
        _counts[type] = n + 1;
        OnSynergyChanged?.Invoke();
    }

    public void Unregister(Define.TowerType type)
    {
        if (!_counts.TryGetValue(type, out int n)) return;
        if (n <= 1) _counts.Remove(type);
        else _counts[type] = n - 1;
        OnSynergyChanged?.Invoke();
    }

    public void Clear()
    {
        _counts.Clear();
        OnSynergyChanged?.Invoke();
    }

    private bool Has(Define.TowerType type) =>
        _counts.TryGetValue(type, out int n) && n > 0;

    // ─── 시너지 목록 ───────────────────────────────────────────────────────────

    /// <summary>전도성 독 — 독탑 + 번개탑: 체인 타겟 중 독 걸린 적에게 데미지 1.5배</summary>
    public bool ConductivePoison => Has(Define.TowerType.Poison) && Has(Define.TowerType.Lightning);

    /// <summary>독침 — 독탑 + 저격탑: 독 걸린 적 저격 시 크리티컬 확률 +25%</summary>
    public bool PoisonShot => Has(Define.TowerType.Poison) && Has(Define.TowerType.Sniper);

    /// <summary>도체 — 슬로우탑 + 번개탑: 슬로우 걸린 적 명중 시 체인 1회 추가</summary>
    public bool Conductor => Has(Define.TowerType.Slow) && Has(Define.TowerType.Lightning);

    /// <summary>정밀 포격 — 슬로우탑 + 캐논탑: 슬로우 걸린 적 명중 시 스플래시 반경 1.5배</summary>
    public bool PrecisionBombardment => Has(Define.TowerType.Slow) && Has(Define.TowerType.Cannon);

    /// <summary>집중사격 — 소총탑 + 저격탑: 두 타워 모두 공격속도 +15%</summary>
    public bool FocusFire => Has(Define.TowerType.Basic) && Has(Define.TowerType.Sniper);
}
