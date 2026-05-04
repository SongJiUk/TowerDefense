using UnityEngine;

/// <summary>
/// 개발 테스트용 도구. 씬에 빈 오브젝트에 붙여두고 Inspector 우클릭으로 실행.
/// 빌드 전 반드시 씬에서 제거할 것.
/// </summary>
public class DevTool : MonoBehaviour
{
    // ─── 난이도 해금 ──────────────────────────────────────────────────────────

    [ContextMenu("난이도 / Easy만 해금 (초기 상태)")]
    void UnlockEasyOnly()
    {
        PlayerPrefs.SetInt("UnlockedDifficulty", 0);
        PlayerPrefs.Save();
        Debug.Log("[DevTool] Easy만 해금");
    }

    [ContextMenu("난이도 / Normal까지 해금")]
    void UnlockNormal()
    {
        PlayerPrefs.SetInt("UnlockedDifficulty", 1);
        PlayerPrefs.Save();
        Debug.Log("[DevTool] Normal까지 해금");
    }

    [ContextMenu("난이도 / Hard까지 해금")]
    void UnlockHard()
    {
        PlayerPrefs.SetInt("UnlockedDifficulty", 2);
        PlayerPrefs.Save();
        Debug.Log("[DevTool] Hard까지 해금");
    }

    [ContextMenu("난이도 / 전체 해금 (Hell 포함)")]
    void UnlockAll()
    {
        PlayerPrefs.SetInt("UnlockedDifficulty", 3);
        PlayerPrefs.Save();
        Debug.Log("[DevTool] 전체 해금");
    }

    // ─── 최고기록 ─────────────────────────────────────────────────────────────

    [ContextMenu("최고기록 / Easy Lv5 웨이브12 세팅")]
    void SetRecordEasy()   => SetRecord(5, 1, 12, Define.Difficulty.Easy);

    [ContextMenu("최고기록 / Normal Lv10 웨이브20 세팅")]
    void SetRecordNormal() => SetRecord(10, 2, 20, Define.Difficulty.Normal);

    [ContextMenu("최고기록 / Hard Lv15 웨이브18 세팅")]
    void SetRecordHard()   => SetRecord(15, 3, 18, Define.Difficulty.Hard);

    void SetRecord(int level, int stage, int wave, Define.Difficulty diff)
    {
        var data = new SaveData
        {
            Level          = level,
            BestStage      = stage,
            BestWave       = wave,
            BestDifficulty = diff,
            Stage1Cleared  = stage >= 1,
            Stage2Cleared  = stage >= 2,
            Stage3Cleared  = stage >= 3,
            Stage4Cleared  = stage >= 4,
        };
        PlayerPrefs.SetString("SaveData", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        Debug.Log($"[DevTool] 최고기록 세팅 — Lv{level} 스테이지{stage} 웨이브{wave} ({diff})");
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    [ContextMenu("전체 데이터 초기화")]
    void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[DevTool] 전체 초기화 완료");
    }

    [ContextMenu("최고기록만 초기화")]
    void ResetRecord()
    {
        var data = new SaveData();
        PlayerPrefs.SetString("SaveData", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        Debug.Log("[DevTool] 최고기록 초기화 완료");
    }
}
