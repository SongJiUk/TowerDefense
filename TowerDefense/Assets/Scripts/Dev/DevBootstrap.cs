#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 에디터 전용. 씬 로드 전에 SessionState에서 테스트 모드 플래그를 읽어 Managers에 적용.
/// TestStartMenu에서 GameScene을 직접 시작할 때 사용.
/// </summary>
public static class DevBootstrap
{
    private const string PREF_KEY = "TD_IsTestMode";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        bool isTest = SessionState.GetBool(PREF_KEY, false);
        SessionState.EraseBool(PREF_KEY);

        if (!isTest) return;

        Managers.IsTestMode    = true;
        Managers.SelectedStage = 1;
        Debug.Log("[DevBootstrap] 테스트 모드로 시작합니다.");
    }
}
#endif
