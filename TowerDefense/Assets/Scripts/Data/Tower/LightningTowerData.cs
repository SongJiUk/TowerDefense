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

    [Header("단계별 강화 (score 0 / 1-3 / 4-6 / 7-9)")]
    public int[] stageChainCountBonus = { 0, 1, 1, 2 };
}
