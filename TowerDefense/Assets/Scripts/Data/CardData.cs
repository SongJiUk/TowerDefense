using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerDefense/CardData")]
public class CardData : ScriptableObject
{
    [Header("카드 효과")]
    public string cardName;
    public string Description;
    public string iconKey;
    public Define.CardCategory category;
    public Define.CardEffectType effectType;
    public float effectValue;
    public float weight;
    public int maxStack;


}
