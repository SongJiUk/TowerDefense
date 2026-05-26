using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementDatabase", menuName = "TowerDefense/AchievementDatabase")]
public class AchievementDatabase : ScriptableObject
{
    public List<AchievementData> achievements = new List<AchievementData>();

    public AchievementData Get(string id) => achievements.Find(a => a.id == id);
}
