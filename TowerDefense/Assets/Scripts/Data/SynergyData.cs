using UnityEngine;

[CreateAssetMenu(fileName = "SynergyData", menuName = "TowerDefense/SynergyData")]
public class SynergyData : ScriptableObject
{
    public string synergyName;
    [TextArea] public string desc;
    public Define.TowerType reqA;
    public Define.TowerType reqB;
}
