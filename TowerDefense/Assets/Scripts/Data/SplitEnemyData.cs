using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/SplitEnemyData")]
public class SplitEnemyData : EnemyData
{
    [Header("분열")]
    public string afterSplitKey;
    public int splitCount = 2;
    public float splitHpRatio = 0.6f;
}
