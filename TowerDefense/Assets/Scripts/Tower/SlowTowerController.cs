using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTowerController : TowerController
{
    private SlowTowerData _slowTowerData;
    public override string GetUniqueEffectText()
    {
        if (_slowTowerData == null) return "";
        float slow = Mathf.Max(0.1f, _slowTowerData.slowMultiplier - _slowTowerData.stageSlowBonus[UniqueEffectStage]);
        float dur  = _slowTowerData.slowDuration + _slowTowerData.stageDurationBonus[UniqueEffectStage];
        return $"이동속도 {slow * 100:F0}% 감소  |  {dur:F1}초";
    }

    protected override void OnHit(Transform target)
    {
        if (target == null) return;
        int stage      = UniqueEffectStage;
        float slow     = Mathf.Max(0.1f, _slowTowerData.slowMultiplier - _slowTowerData.stageSlowBonus[stage]);
        float duration = _slowTowerData.slowDuration + _slowTowerData.stageDurationBonus[stage];
        target.GetComponent<BuffHandler>()?.AddEffect(new SlowEffect(slow, duration));
    }

    public override void Init(TowerData data)
    {
        base.Init(data);
        _slowTowerData = data as SlowTowerData;
    }
}
