using UnityEngine;

/// <summary>
/// 타워 1종의 모든 데이터를 담는 ScriptableObject.
/// TowerPlacer._allTowerData[]에 연결하면 팝업 버튼과 1:1 매칭된다.
/// 메뉴: Create > TowerDefense > TowerData
/// </summary>
[CreateAssetMenu(menuName = "TowerDefense/TowerData")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public Define.TowerType towerType;

    [Header("기본 스탯")]
    public float baseDamage;
    public float baseAttackSpeed;   // 초당 공격 횟수
    public float baseRange;         // 사거리 (유닛)


    [Header("투사체")]
    /// <summary>투사체 -> Addressalbe로 가져와서 Key로 가져옴.</summary>
    public string projectilePrefabKey;
    public float projectileSpeed = 10f;

    [Header("업그레이드")]
    /// <summary>레벨업 시 적용할 스탯 배율. [0]=Lv2, [1]=Lv3 순.</summary>
    public TowerUpgradeStep[] upgradeSteps;

    [Header("비용")]
    public int buildCost;

    [Header("아트")]
    /// <summary>해당 키로 설정</summary>
    public string addressableKey;
}

/// <summary>
/// 타워 레벨업 한 단계의 데이터.
/// TowerController.TryUpgrade()에서 CurrentLevel에 맞는 인덱스의 값을 사용.
/// </summary>
[System.Serializable]
public class TowerUpgradeStep
{
    public int upgradeCost;
    public float damageMultiplier;
    public float attackSpeedMultiplier;
    public float rangeBonus;
}
