using UnityEngine;

/// <summary>
/// 난이도 선택 및 해금 상태 관리. PlayerPrefs로 영구 저장.
/// Easy만 해금된 상태로 시작, 클리어 시 다음 난이도 해금.
/// </summary>
public class DifficultyManager
{
    private const string PREF_KEY = "UnlockedDifficulty";

    public Define.Difficulty Selected { get; private set; } = Define.Difficulty.Easy;

    public int MaxUnlocked => PlayerPrefs.GetInt(PREF_KEY, 0);

    public bool IsUnlocked(Define.Difficulty d) => (int)d <= MaxUnlocked;

    public void Select(Define.Difficulty d)
    {
        if (!IsUnlocked(d)) return;
        Selected = d;
    }

    /// <summary>게임 클리어 시 호출. 현재 난이도가 최고 해금 단계면 다음 단계 해금.</summary>
    public void OnGameClear()
    {
        int unlocked = MaxUnlocked;
        int selected = (int)Selected;
        int maxIndex = System.Enum.GetValues(typeof(Define.Difficulty)).Length - 1;
        if (selected == unlocked && unlocked < maxIndex)
        {
            PlayerPrefs.SetInt(PREF_KEY, selected + 1);
            PlayerPrefs.Save();
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
}
