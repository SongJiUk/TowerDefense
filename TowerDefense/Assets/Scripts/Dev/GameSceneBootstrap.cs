#if UNITY_EDITOR
using UnityEditor;
#endif
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// TowerDefense/MyEditor/테스트모드로 GameScene을 직접 실행할 때 초기화를 담당.
/// 정식 플로우(LoadingScene 경유)에서는 즉시 완료 처리됨.
/// </summary>
public class GameSceneBootstrap : MonoBehaviour
{
    public static UniTask ReadyTask => _readyTcs.Task;
    private static UniTaskCompletionSource _readyTcs = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnPlay() => _readyTcs = new();

    async void Awake()
    {
        // 정식 플로우(LoadingScene 경유): LoadingSceneManager가 이미 WaveM.Init()까지 완료함
        if (Managers.WaveM.IsInitialized)
        {
            _readyTcs.TrySetResult();
            return;
        }

        int stage = 1;
#if UNITY_EDITOR
        stage = EditorPrefs.GetInt("TestMode_Stage", 1);
#endif
        Debug.Log($"[Bootstrap] 테스트모드 Stage {stage} — 리소스 로드 시작");

        await Managers.ResourceM.LoadGroupAsync<Object>("PrevLoad", (key, cur, total) =>
            Debug.Log($"[Bootstrap] {cur}/{total} : {key}")
        );

        Managers.SelectedStage = stage;
        Managers.GameM.LevelData = Managers.ResourceM.Load<LevelData>("LevelData");
        Managers.CardM.Init();

        string stageKey = $"Stage{stage}Data";
        StageData stageData = Managers.ResourceM.Load<StageData>(stageKey);

        if (stageData == null)
        {
            Debug.LogError($"[Bootstrap] StageData 로드 실패 — key: '{stageKey}' / PrevLoad 그룹에 있는지, Addressable key가 맞는지 확인");
            _readyTcs.TrySetResult(); // hang 방지: WaveStarter가 무한 대기하지 않도록
            return;
        }

        Managers.WaveM.Init(stageData);
        Debug.Log($"[Bootstrap] 초기화 완료");
        _readyTcs.TrySetResult();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            Managers.GameM.TriggerGameOver();
    }
#endif
}
