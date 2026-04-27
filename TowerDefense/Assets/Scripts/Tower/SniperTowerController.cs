using UnityEngine;

public class SniperTowerController : TowerController
{
    private SniperTowerData _sniperData;

    public override void Init(TowerData data)
    {
        base.Init(data);
        _sniperData = data as SniperTowerData;
    }

    protected override float GetBonusCritChance(Transform target)
    {
        if (_sniperData == null) return 0f;
        float bonus = _sniperData.stageCritBonus[UniqueEffectStage];
        if (Managers.SynergyM != null && Managers.SynergyM.PoisonShot)
        {
            var buff = target.GetComponent<BuffHandler>();
            if (buff != null && buff.HasEffect<PoisonEffect>())
            {
                float poisonBonus = 0.25f * (Managers.GameM?.synergyMultiplier ?? 1f);
                bonus += poisonBonus;
                Debug.Log($"[Synergy:독침] 독 적 저격 → 크리티컬 +{poisonBonus * 100:F0}% (총 {bonus * 100:F0}%)");
            }
        }
        return bonus;
    }

    public override string GetUniqueEffectText()
    {
        if (_sniperData == null) return "";
        float crit = _sniperData.stageCritBonus[UniqueEffectStage];
        return $"크리티컬 확률 +{crit * 100:F0}%";
    }
}
