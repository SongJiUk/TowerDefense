using UnityEngine;

/// <summary>
/// 업적 정의 ScriptableObject. 각 업적의 ID, 제목, 설명, 목표값을 저장.
/// </summary>
[CreateAssetMenu(fileName = "Achievement_", menuName = "TowerDefense/AchievementData")]
public class AchievementData : ScriptableObject
{
    public string id;
    public string title;
    [TextArea] public string description;
    public int targetValue;
}
