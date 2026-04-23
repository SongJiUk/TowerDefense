using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/TowerData")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public string Description;
    public Define.TowerType towerType;

    [Header("기본 스탯")]
    public float baseDamage;
    public float baseAttackSpeed;
    public float baseRange;

    [Header("투사체")]
    public string projectilePrefabKey;
    public float projectileSpeed = 10f;

    [Header("업그레이드 — 공격력 / 사거리 / 공격속도 독립")]
    public TowerStatUpgrade[] damageUpgrades;
    public TowerStatUpgrade[] rangeUpgrades;
    public TowerStatUpgrade[] speedUpgrades;

    [Header("비용")]
    public int buildCost;

    [Header("아트")]
    public string addressableKey;
    public string iconKey;
}

[System.Serializable]
public class TowerStatUpgrade
{
    public string upgradeName;
    public string description;
    public int cost;
    /// <summary>base * multiplier = 업그레이드 후 값. 예) 1.25 = +25%</summary>
    public float multiplier;
}
