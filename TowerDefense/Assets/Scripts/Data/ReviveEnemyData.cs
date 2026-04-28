using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/ReviveEnemyData")]
public class ReviveEnemyData : EnemyData
{
    [Header("부활")]
    public float reviveHpRatio = 0.4f;
}
