using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/TowerData/PoisonTowerData")]
public class PoisonTowerData : TowerData
{

    [Header("독 효과")]
    public float poisonDps;
    public float poisonDuration;
}
