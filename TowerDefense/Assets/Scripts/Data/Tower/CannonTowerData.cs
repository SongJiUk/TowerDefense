using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/TowerData/CannonTower")]
public class CannonTowerData : TowerData
{
    [Header("스플래시 효과")]
    public float splashRadius;

    [Header("단계별 강화 (score 0 / 1-3 / 4-6 / 7-9)")]
    public float[] stageSplashBonus = { 0f, 0.3f, 0.6f, 1.0f };
}
