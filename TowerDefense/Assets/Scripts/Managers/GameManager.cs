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

    public void SetLevel(int level, int exp)
    {
        Level = Mathf.Max(1, level);
        CurrentExp = Mathf.Max(0, exp);
        OnExpChanged?.Invoke(CurrentExp, LevelData?.GetRequiredExp(Level) ?? 0);
    }

    // ─── 골드 ────────────────────────────────────────────────────────────────

    public int Gold { get; private set; } = 150;
    public event Action<int> OnGoldChanged;

    public void AddGold(int amount)
    {
        float mult = Managers.DifficultyM?.GoldMultiplier ?? 1f;
        Gold += Mathf.RoundToInt(amount * mult);
        OnGoldChanged?.Invoke(Gold);
    }

    public bool TestInfiniteGold = false;

    public bool SpendGold(int amount)
    {
        if (TestInfiniteGold) return true;
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
    public float pendingEnemyHpMultiplier = 1f;
    public float synergyMultiplier = 1f;
    public int   freeTowerCount = 0;

    public event Action OnCardApplied;
    public void NotifyCardApplied() => OnCardApplied?.Invoke();

    // ─── 게임오버 / 클리어 ───────────────────────────────────────────────────

    public event Action OnGameOver;
    public void TriggerGameOver() => OnGameOver?.Invoke();

    public event Action OnGameClear;
    public void TriggerGameClear() => OnGameClear?.Invoke();

    public void Reset()
    {
        ResetLevel();
        ResetGold();
        globalDamageMultiplier      = 1f;
        globalAttackSpeedMultiplier = 1f;
        globalRangeBonus            = 0f;
        criticalChanceBonus         = 0f;
        killRewardMultiplier        = 1f;
        waveBonusMultiplier         = 1f;
        buildCostMultiplier         = 1f;
        nextWaveEnemyHpMultiplier   = 1f;
        pendingEnemyHpMultiplier    = 1f;
        synergyMultiplier           = 1f;
        freeTowerCount              = 0;
    }
}
