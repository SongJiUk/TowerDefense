using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTowerController : TowerController
{
    private SlowTowerData _slowTowerData;
    protected override void OnHit(Transform target)
    {
        if (target != null) target.GetComponent<BuffHandler>()?.AddEffect(new SlowEffect(_slowTowerData.slowMultiplier, _slowTowerData.slowDuration));
    }

    public override void Init(TowerData data)
    {
        base.Init(data);
        _slowTowerData = data as SlowTowerData;
    }
}
