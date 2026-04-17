using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonTowerController : TowerController
{
    private PoisonTowerData _poisonTowerData;


    public override void Init(TowerData data)
    {
        base.Init(data);
        _poisonTowerData = data as PoisonTowerData;
    }

    protected override void OnHit(Transform target)
    {
        var damageable = target.GetComponent<IDamageable>();
        target.GetComponent<BuffHandler>()?.AddEffect(new PoisonEffect(damageable, _poisonTowerData.poisonDps, _poisonTowerData.poisonDuration));
    }
}
