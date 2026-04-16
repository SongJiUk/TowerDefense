using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "TowerDefense/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Define.SkillType skillType;

    [Header("스탯")]
    public float baseDamage;
    public float baseRange;

    [Header("오브젝트")]
    public GameObject skillPrefab;

    [Header("업드레이드")]
    public SkillUpgradeStep[] upgradeSteps;

}

[System.Serializable]
public class SkillUpgradeStep
{
    public float damageMultiplier;
    public float rangeBonus;
    public float skillDuration;
}
