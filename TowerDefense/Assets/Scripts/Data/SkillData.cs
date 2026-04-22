using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "TowerDefense/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Define.SkillType skillType;
    public string Description;

    [Header("스탯")]
    public float baseDamage;
    public float baseRange;
    public float baseDuration;
    public float cooldown;

    [Header("부가효과")]
    public float effectValue;

    [Header("오브젝트")]
    public string skillPrefabkey;

    [Header("타겟팅")]
    public bool isTargeted;

    [Header("업드레이드")]
    public SkillUpgradeStep[] upgradeSteps;

    [Header("UI")]
    public Color color;

}

[System.Serializable]
public class SkillUpgradeStep
{
    public float damageMultiplier;
    public float rangeBonus;
    public float skillDuration;
    public float cooldownReduction;
}
