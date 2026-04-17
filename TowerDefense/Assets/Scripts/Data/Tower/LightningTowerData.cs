using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/TowerData/LightningTowerData")]
public class LightningTowerData : TowerData
{
    [Header("체인 효과")]
    public int chainCount;
    public float chainRange;
    public float chainDamageFalloff;
}
