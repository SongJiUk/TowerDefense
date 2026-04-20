using System;
using UnityEngine;

/// <summary>
/// 게임 상태 관리 — 골드, 경험치, 레벨.
/// Managers.GameM을 통해 인스턴스로 접근.
/// </summary>
public class GameManager
{
    // ─── 레벨 ────────────────────────────────────────────────────────────────

    public LevelData LevelData;
    public int Level { get; private set; } = 1;
    public int CurrentExp { get; private set; } = 0;

    public event Action<int, int> OnExpChanged;
    public event Action<int, int> OnLevelUp;

    public void AddExp(float amount)
    {
        if (LevelData == null) return;
        if (Level >= LevelData.MaxLevel) return;

        CurrentExp += (int)amount;
        OnExpChanged?.Invoke(CurrentExp, LevelData.GetRequiredExp(Level));

        while (Level < LevelData.MaxLevel)
        {
            int required = LevelData.GetRequiredExp(Level);
            if (CurrentExp < required) break;

            CurrentExp -= required;
            Level++;
            OnLevelUp?.Invoke(Level, CurrentExp);
        }
    }

    public void ResetLevel()
    {
        Level = 1;
        CurrentExp = 0;
        OnExpChanged?.Invoke(CurrentExp, LevelData?.GetRequiredExp(Level) ?? 0);
    }

    // ─── 골드 ────────────────────────────────────────────────────────────────

    public int Gold { get; private set; } = 150;
    public event Action<int> OnGoldChanged;

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    public void ResetGold(int startGold = 150)
    {
        Gold = startGold;
        OnGoldChanged?.Invoke(Gold);
    }

    // ─── 카드 ────────────────────────────────────────────────────────────────
    public float globalDamageMultiplier = 1f;
    public float globalAttackSpeedMultiplier = 1f;
    public float globalRangeBonus = 0f;
    public float criticalChanceBonus = 0f;
    public float killRewardMultiplier = 1f;
    public float waveBonusMultiplier = 1f;
    public float buildCostMultiplier = 1f;
    public float nextWaveEnemyHpMultiplier = 1f;

    public event Action OnCardApplied;
    public void NotifyCardApplied() => OnCardApplied?.Invoke();
}
