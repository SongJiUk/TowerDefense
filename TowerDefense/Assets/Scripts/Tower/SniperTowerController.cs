using UnityEngine;

public class SniperTowerController : TowerController
{
    private SniperTowerData _sniperData;

    public override void Init(TowerData data)
    {
        base.Init(data);
        _sniperData = data as SniperTowerData;
    }

    protected override float GetBonusCritChance()
    {
        if (_sniperData == null) return 0f;
        return _sniperData.stageCritBonus[UniqueEffectStage];
    }

    public override string GetUniqueEffectText()
    {
        if (_sniperData == null) return "";
        int stage      = UniqueEffectStage;
        float crit     = _sniperData.stageCritBonus[stage];
        if (stage < 3)
        {
            float nextCrit = _sniperData.stageCritBonus[stage + 1];
            return $"크리티컬 +{crit * 100:F0}%  ->  +{nextCrit * 100:F0}%";
        }
        return $"크리티컬 +{crit * 100:F0}%  (최대)";
    }
}
