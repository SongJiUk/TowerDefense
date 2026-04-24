using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/TowerData/SniperTowerData")]
public class SniperTowerData : TowerData
{
    [Header("크리티컬 효과")]
    [Header("단계별 추가 크리티컬 확률 (score 0 / 1-3 / 4-6 / 7-9)")]
    public float[] stageCritBonus = { 0f, 0.05f, 0.12f, 0.20f };
}
