using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 진행도 추적 및 달성 알림. Managers.AchievementM으로 접근.
/// 진행도는 PlayerPrefs에 "ACH_{id}_progress" 키로 저장.
/// </summary>
public class AchievementManager
{
    private const string PREFIX = "ACH_";
    private const string UNLOCKED_SUFFIX = "_unlocked";
    private const string PROGRESS_SUFFIX = "_progress";

    public event Action<AchievementData> OnAchievementUnlocked;

    // ─── 공개 API ─────────────────────────────────────────────────────────────

    public bool IsUnlocked(string id)
        => PlayerPrefs.GetInt(PREFIX + id + UNLOCKED_SUFFIX, 0) == 1;

    public int GetProgress(string id)
        => PlayerPrefs.GetInt(PREFIX + id + PROGRESS_SUFFIX, 0);

    /// <summary>특정 업적의 진행도를 delta만큼 증가. 목표 달성 시 OnAchievementUnlocked 발행.</summary>
    public void AddProgress(AchievementData data, int delta = 1)
    {
        if (data == null || IsUnlocked(data.id)) return;

        int current = GetProgress(data.id) + delta;
        PlayerPrefs.SetInt(PREFIX + data.id + PROGRESS_SUFFIX, current);

        if (current >= data.targetValue)
            Unlock(data);
        else
            PlayerPrefs.Save();
    }

    /// <summary>진행도와 무관하게 즉시 달성.</summary>
    public void Unlock(AchievementData data)
    {
        if (data == null || IsUnlocked(data.id)) return;
        PlayerPrefs.SetInt(PREFIX + data.id + UNLOCKED_SUFFIX, 1);
        PlayerPrefs.Save();
        OnAchievementUnlocked?.Invoke(data);
        ShowPopup(data);
    }

    // ─── 내부 ─────────────────────────────────────────────────────────────────

    private void ShowPopup(AchievementData data)
    {
        var popup = Managers.ObjectM?.SpawnUI<UI_AchievementPopup>("UI_AchievementPopup", null);
        if (popup != null)
        {
            _ = popup.Init();
            popup.Show(data);
        }
    }
}
