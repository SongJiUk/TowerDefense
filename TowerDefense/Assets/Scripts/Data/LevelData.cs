using UnityEngine;

/// <summary>
/// 플레이어 레벨업 경험치 테이블.
/// _expPerLevel[i] = 레벨 (i+1) → (i+2) 에 필요한 경험치.
/// 메뉴: Create > TowerDefense > LevelData
/// </summary>
[CreateAssetMenu(menuName = "TowerDefense/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private int[] _expPerLevel = new int[]
    {
         50,  // Lv 1 → 2
         80,  // Lv 2 → 3
        110,  // Lv 3 → 4
        140,  // Lv 4 → 5
        170,  // Lv 5 → 6
        200,  // Lv 6 → 7
        230,  // Lv 7 → 8
        260,  // Lv 8 → 9
        290,  // Lv 9 → 10
        330,  // Lv 10 → 11
        370,  // Lv 11 → 12
        420,  // Lv 12 → 13
        470,  // Lv 13 → 14
        530,  // Lv 14 → 15
        590,  // Lv 15 → 16
        660,  // Lv 16 → 17
        730,  // Lv 17 → 18
        810,  // Lv 18 → 19
        900,  // Lv 19 → 20
    };

    public int MaxLevel => _expPerLevel.Length + 1;

    /// <summary>현재 레벨에서 다음 레벨까지 필요한 경험치. 마스터 레벨이면 0 반환.</summary>
    public int GetRequiredExp(int currentLevel)
    {
        int index = currentLevel - 1;
        if (index < 0 || index >= _expPerLevel.Length) return 0;
        return _expPerLevel[index];
    }
}
