using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/TowerData/SlowTowerData")]
public class SlowTowerData : TowerData
{
    [Header("슬로우 효과")]
    public float slowMultiplier;
    public float slowDuration;

    [Header("단계별 강화 (score 0 / 1-3 / 4-6 / 7-9)")]
    public float[] stageSlowBonus     = { 0f, 0.05f, 0.10f, 0.20f };
    public float[] stageDurationBonus = { 0f, 0.50f, 1.00f, 2.00f };
}
