using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/TowerData/SlowTowerData")]
public class SlowTowerData : TowerData
{
    [Header("슬로우 효과")]
    public float slowMultiplier;
    public float slowDuration;
}
