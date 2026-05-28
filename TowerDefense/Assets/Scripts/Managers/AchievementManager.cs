using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 진행도 추적 및 달성 알림. SaveData에 저장되어 Firebase로 동기화.
/// </summary>
public class AchievementManager
{
    public event Action<AchievementData> OnAchievementUnlocked;

    private AchievementDatabase _db;

    public AchievementDatabase Database => _db;

    public void Init(AchievementDatabase db) => _db = db;

    // ─── 공개 API ─────────────────────────────────────────────────────────────

    public bool IsUnlocked(string id) => GetOrCreate(id).unlocked;

    public int GetProgress(string id) => GetOrCreate(id).progress;

    public void AddProgress(string id, int delta = 1)
    {
        if (_db == null) return;
        var data = _db.Get(id);
        if (data != null) AddProgress(data, delta);
    }

    public void Unlock(string id)
    {
        if (_db == null) return;
        var data = _db.Get(id);
        if (data != null) Unlock(data);
    }

    public void AddProgress(AchievementData data, int delta = 1)
    {
        if (data == null || IsUnlocked(data.id)) return;

        var entry = GetOrCreate(data.id);
        entry.progress += delta;

        if (entry.progress >= data.targetValue)
            Unlock(data);
        else
            Managers.SaveM.SaveCurrent();
    }

    public void Unlock(AchievementData data)
    {
        if (data == null || IsUnlocked(data.id)) return;

        var entry = GetOrCreate(data.id);
        entry.unlocked = true;
        Managers.SaveM.SaveCurrent();

        OnAchievementUnlocked?.Invoke(data);
        ShowPopup(data);
    }

    // ─── 내부 ─────────────────────────────────────────────────────────────────

    private AchievementSaveEntry GetOrCreate(string id)
    {
        var list = Managers.SaveM.Data.Achievements;
        var entry = list.Find(e => e.id == id);
        if (entry == null)
        {
            entry = new AchievementSaveEntry { id = id };
            list.Add(entry);
        }
        return entry;
    }

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
