using UnityEngine;

/// <summary>
/// 난이도 선택 및 해금 상태 관리. SaveData에 저장되어 Firebase로 동기화.
/// Easy만 해금된 상태로 시작, 클리어 시 다음 난이도 해금.
/// </summary>
public class DifficultyManager
{
    public Define.Difficulty Selected { get; private set; } = Define.Difficulty.Easy;

    public int MaxUnlocked => Managers.SaveM?.Data.UnlockedDifficulty ?? 0;

    public bool IsUnlocked(Define.Difficulty d) => (int)d <= MaxUnlocked;

    public void Init(SaveData data)
    {
        int maxIdx = System.Enum.GetValues(typeof(Define.Difficulty)).Length - 1;
        Selected = (Define.Difficulty)Mathf.Clamp(data.SelectedDifficulty, 0, data.UnlockedDifficulty);
    }

    public void Select(Define.Difficulty d)
    {
        if (!IsUnlocked(d)) return;
        Selected = d;
        var data = Managers.SaveM?.Data;
        if (data == null) return;
        data.SelectedDifficulty = (int)d;
        Managers.SaveM.SaveCurrent();
    }

    public void OnGameClear()
    {
        var data = Managers.SaveM?.Data;
        if (data == null) return;
        int maxIndex = System.Enum.GetValues(typeof(Define.Difficulty)).Length - 1;
        int selected = (int)Selected;
        if (selected == data.UnlockedDifficulty && data.UnlockedDifficulty < maxIndex)
        {
            data.UnlockedDifficulty = selected + 1;
            Managers.SaveM.SaveCurrent();
        }
    }

    // ─── 배율 ─────────────────────────────────────────────────────────────────

    public float EnemyHpMultiplier => Selected switch
    {
        Define.Difficulty.Easy   => 0.80f,
        Define.Difficulty.Normal => 1.00f,
        Define.Difficulty.Hard   => 1.30f,
        Define.Difficulty.Hell   => 1.70f,
        _ => 1f
    };

    public float EnemySpeedMultiplier => Selected switch
    {
        Define.Difficulty.Easy   => 0.90f,
        Define.Difficulty.Normal => 1.00f,
        Define.Difficulty.Hard   => 1.15f,
        Define.Difficulty.Hell   => 1.30f,
        _ => 1f
    };

    public float GoldMultiplier => Selected switch
    {
        Define.Difficulty.Easy   => 1.20f,
        Define.Difficulty.Normal => 1.00f,
        Define.Difficulty.Hard   => 0.90f,
        Define.Difficulty.Hell   => 0.75f,
        _ => 1f
    };

    public float CoreHpMultiplier => Selected switch
    {
        Define.Difficulty.Easy   => 1.50f,
        Define.Difficulty.Normal => 1.00f,
        Define.Difficulty.Hard   => 0.80f,
        Define.Difficulty.Hell   => 0.50f,
        _ => 1f
    };

    public int WaveCount => Selected switch
    {
        Define.Difficulty.Easy   => 10,
        Define.Difficulty.Normal => 15,
        Define.Difficulty.Hard   => 20,
        Define.Difficulty.Hell   => 25,
        _ => 10
    };

    public int MiddleBossCount => Selected switch
    {
        Define.Difficulty.Easy   => 1,
        Define.Difficulty.Normal => 2,
        Define.Difficulty.Hard   => 3,
        Define.Difficulty.Hell   => 4,
        _ => 1
    };
}
