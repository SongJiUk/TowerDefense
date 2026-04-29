using System;
using UnityEngine;

/// <summary>
/// 게임 진행 데이터 저장/불러오기.
/// ISaveStorage 구현체를 교체하는 것만으로 저장소를 변경할 수 있다.
/// 현재: PlayerPrefs (임시) → 추후: FirebaseSaveStorage로 교체 예정.
/// </summary>
public class SaveManager
{
    private readonly ISaveStorage _storage = new PlayerPrefsSaveStorage();

    private SaveData _data;
    public SaveData Data => _data ??= _storage.Load();

    // ─── 공개 API ─────────────────────────────────────────────────────────────

    public void OnStageClear(int stage)
    {
        if (stage >= 1 && stage <= 4)
            Data.SetStageCleared(stage);
        Data.Level = Managers.GameM.Level;
        Data.Exp   = Managers.GameM.CurrentExp;
        _storage.Save(Data);
    }

    public void OnGameOver()
    {
        Data.Level = Managers.GameM.Level;
        Data.Exp   = Managers.GameM.CurrentExp;
        _storage.Save(Data);
    }

    public bool IsStageCleared(int stage) => Data.IsStageCleared(stage);

    public void ApplyToGame()
    {
        Managers.GameM.SetLevel(Data.Level, Data.Exp);
    }
}

// ─── 저장소 인터페이스 ────────────────────────────────────────────────────────

public interface ISaveStorage
{
    void     Save(SaveData data);
    SaveData Load();
}

// ─── PlayerPrefs 구현 (임시) ──────────────────────────────────────────────────

public class PlayerPrefsSaveStorage : ISaveStorage
{
    private const string KEY = "SaveData";

    public void Save(SaveData data)
    {
        PlayerPrefs.SetString(KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public SaveData Load()
    {
        string json = PlayerPrefs.GetString(KEY, null);
        if (!string.IsNullOrEmpty(json))
        {
            try { return JsonUtility.FromJson<SaveData>(json); }
            catch (Exception e) { Debug.LogWarning($"[SaveManager] 불러오기 실패: {e.Message}"); }
        }
        return new SaveData();
    }
}

// ─── 데이터 ───────────────────────────────────────────────────────────────────

[Serializable]
public class SaveData
{
    public int  Level = 1;
    public int  Exp   = 0;
    public bool Stage1Cleared;
    public bool Stage2Cleared;
    public bool Stage3Cleared;
    public bool Stage4Cleared;

    public void SetStageCleared(int stage)
    {
        if      (stage == 1) Stage1Cleared = true;
        else if (stage == 2) Stage2Cleared = true;
        else if (stage == 3) Stage3Cleared = true;
        else if (stage == 4) Stage4Cleared = true;
    }

    public bool IsStageCleared(int stage) => stage switch
    {
        1 => Stage1Cleared,
        2 => Stage2Cleared,
        3 => Stage3Cleared,
        4 => Stage4Cleared,
        _ => false
    };
}
