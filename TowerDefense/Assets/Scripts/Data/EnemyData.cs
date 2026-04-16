using UnityEngine;

/// <summary>
/// 적 1종의 기본 데이터를 담는 ScriptableObject.
/// GameDataGenerator 에디터 메뉴로 자동 생성 가능.
/// WaveManager가 스폰 시 hpMultiplier·speedMultiplier를 곱해 웨이브별 강도를 조절한다.
/// 메뉴: Create > TowerDefense > EnemyData
/// </summary>
[CreateAssetMenu(menuName = "TowerDefense/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string           enemyName;
    public Define.EnemyType enemyType;

    [Header("기본 스탯")]
    public int coreDamage = 1;
    public float baseHp;
    public float baseMoveSpeed;

    /// <summary>처치 시 지급할 골드. EnemyController.Die()에서 Managers.AddGold()로 지급 예정.</summary>
    public int   baseReward;

    [Header("아트")]
    /// <summary>적 GameObject 프리팹. EnemyController 컴포넌트 + Enemy 레이어 설정 필수.</summary>
    public GameObject prefab;
    public string     addressableKey;
}
