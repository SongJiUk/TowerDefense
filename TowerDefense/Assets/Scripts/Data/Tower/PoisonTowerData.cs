using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/TowerData/PoisonTowerData")]
public class PoisonTowerData : TowerData
{

    [Header("독 효과")]
    public float poisonDps;
    public float poisonDuration;

    [Header("단계별 강화 (score 0 / 1-3 / 4-6 / 7-9)")]
    public float[] stageDpsMultiplier  = { 1f, 1.3f, 1.6f, 2.0f };
    public float[] stageDurationBonus  = { 0f, 0.5f, 1.0f, 2.0f };
}
