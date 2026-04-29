using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Define;

public class CardManager
{
    Dictionary<CardCategory, List<CardData>> _cardDatas = new();

    Dictionary<CardData, int> _stackCount = new();


    public void Init()
    {
        foreach (CardCategory category in Enum.GetValues(typeof(CardCategory)))
        {
            _cardDatas[category] = new List<CardData>();
        }


        foreach (CardEffectType effectType in Enum.GetValues(typeof(CardEffectType)))
        {
            string key = effectType.ToString() + "Data";
            var card = Managers.ResourceM.Load<CardData>(key);
            if (card == null) continue;
            _cardDatas[card.category].Add(card);
        }
    }

    public List<CardData> DrawCard(int count = 3)
    {
        // 카드가 남은 카테고리만 수집
        var availableCategories = new List<CardCategory>();
        foreach (CardCategory category in Enum.GetValues(typeof(CardCategory)))
        {
            bool hasCandidate = _cardDatas[category]
                .Any(card => GetStackCount(card) < card.maxStack && IsCardAvailable(card));
            if (hasCandidate) availableCategories.Add(category);
        }

        // 피셔-예이츠 셔플
        for (int i = availableCategories.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (availableCategories[i], availableCategories[j]) = (availableCategories[j], availableCategories[i]);
        }

        List<CardData> result = new List<CardData>();
        int drawCount = Mathf.Min(count, availableCategories.Count);

        for (int i = 0; i < drawCount; i++)
        {
            var candidates = _cardDatas[availableCategories[i]]
                .Where(card => GetStackCount(card) < card.maxStack && IsCardAvailable(card))
                .ToList();
            result.Add(PickWeighted(candidates));
        }

        return result;
    }

    private bool IsCardAvailable(CardData card)
    {
        if (card.effectType == CardEffectType.SkillPointUp)
        {
            for (int i = 0; i < 3; i++)
                if (Managers.SkillM.GetSlot(i) != null) return true;
            return false;
        }
        return true;
    }

    public int GetStackCount(CardData cardData)
    {
        _stackCount.TryGetValue(cardData, out int count);
        return count;
    }

    public CardData PickWeighted(List<CardData> cardDatas)
    {
        float total = 0f;
        foreach (var card in cardDatas) total += card.weight;

        float random = UnityEngine.Random.Range(0f, total);
        float current = 0f;

        foreach (var card in cardDatas)
        {
            current += card.weight;
            if (random <= current) return card;
        }

        return cardDatas[cardDatas.Count - 1];
    }

    public void ApplyCard(CardData cardData)
    {
        if (!_stackCount.ContainsKey(cardData))
            _stackCount[cardData] = 0;
        _stackCount[cardData]++;

        switch (cardData.effectType)
        {
            case CardEffectType.DamageUp:
                Managers.GameM.globalDamageMultiplier += cardData.effectValue;
                break;

            case CardEffectType.AttackSpeedUp:
                Managers.GameM.globalAttackSpeedMultiplier += cardData.effectValue;
                break;

            case CardEffectType.RangeUp:
                Managers.GameM.globalRangeBonus += cardData.effectValue;
                break;

            case CardEffectType.CriticalChanceUp:
                Managers.GameM.criticalChanceBonus += cardData.effectValue;
                break;

            case CardEffectType.GoldInstant:
                Managers.GameM.AddGold((int)cardData.effectValue);
                break;
            case CardEffectType.KillRewardUp:
                Managers.GameM.killRewardMultiplier += cardData.effectValue;
                break;

            case CardEffectType.BuildCostDown:
                Managers.GameM.buildCostMultiplier -= cardData.effectValue;
                break;

            case CardEffectType.WaveBonusDouble:
                Managers.GameM.waveBonusMultiplier += cardData.effectValue;
                break;

            case CardEffectType.CoreHpUp:
                Managers.ICore?.Heal((int)cardData.effectValue);
                break;

            case CardEffectType.EnemyHpDown:
                if (Managers.WaveM.IsRunning)
                    Managers.GameM.pendingEnemyHpMultiplier -= cardData.effectValue;
                else
                    Managers.GameM.nextWaveEnemyHpMultiplier -= cardData.effectValue;
                break;

            case CardEffectType.SkillSelect:
                Managers.UIM.ShowPopup<UI_SkillSelectPopup>("UI_SkillSelectPopup");
                break;
            case CardEffectType.SkillPointUp:
                Managers.SkillM.AddSkillPoint();
                break;
            case CardEffectType.FreeTower:
                Managers.GameM.freeTowerCount += (int)cardData.effectValue;
                Debug.Log($"[Card:FreeTower] 무료 설치 +{(int)cardData.effectValue} (총 {Managers.GameM.freeTowerCount})");
                break;

            case CardEffectType.SynergyAmp:
                Managers.GameM.synergyMultiplier += cardData.effectValue;
                Debug.Log($"[Card:SynergyAmp] 시너지 배율 {Managers.GameM.synergyMultiplier:F2}x");
                break;
        }

        Managers.GameM.NotifyCardApplied();
    }

    public void Clear()
    {
        _stackCount.Clear();
    }
}
