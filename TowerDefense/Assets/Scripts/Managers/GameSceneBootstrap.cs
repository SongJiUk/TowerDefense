#if UNITY_EDITOR
using UnityEditor;
#endif
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// GameScene 진입 시 맵 생성 + ReadyTask 완료를 담당.
/// 정상 플로우: 타이틀에서 WaveM.Init 완료 + 에셋 유지 → 여기서 맵만 생성.
/// 테스트 모드: WaveM 미초기화 상태로 GameScene 직접 진입 → PrevLoad 재로드 + 전체 초기화.
/// </summary>
public class GameSceneBootstrap : MonoBehaviour
{
    public static UniTask ReadyTask => _readyTcs.Task;
    private static UniTaskCompletionSource _readyTcs = new();

    /// <summary>씬 재로드 전 호출 — SceneManager.LoadScene 직전에 사용</summary>
    public static void ResetTask() => _readyTcs = new UniTaskCompletionSource();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnPlay() => _readyTcs = new();

    async void Awake()
    {
        if (!Managers.WaveM.IsInitialized)
        {
            // 테스트 모드: GameScene을 직접 열었으므로 모든 초기화 필요
            int stage = Managers.SelectedStage;
            Debug.Log($"[Bootstrap] 테스트 모드 — Stage {stage} 초기화");

            await Managers.ResourceM.LoadGroupAsync<Object>("PrevLoad", (key, cur, total) =>
                Debug.Log($"[Bootstrap] {cur}/{total} : {key}")
            );

            Managers.GameM.LevelData = Managers.ResourceM.Load<LevelData>("LevelData");
            Managers.CardM.Init();
            Managers.SaveM.ApplyToGame();

            string stageKey = $"Stage{stage}Data";
            StageData stageData = Managers.ResourceM.Load<StageData>(stageKey);

            if (stageData == null)
            {
                Debug.LogError($"[Bootstrap] StageData 로드 실패: '{stageKey}'");
                _readyTcs.TrySetResult();
                return;
            }

            Managers.WaveM.Init(stageData);
        }

        // 정상/테스트 공통: 맵 생성
        SpawnMap();

        _readyTcs.TrySetResult();
    }

    private void SpawnMap()
    {
        string mapKey = Managers.WaveM.CurrentStage?.mapPrefabKey;
        if (!string.IsNullOrEmpty(mapKey))
            Managers.ResourceM.Instantiate(mapKey);
        else
            Debug.LogWarning("[Bootstrap] mapPrefabKey가 비어 있음");
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Managers.IsTestMode) return;
        if (Input.GetKeyDown(KeyCode.F)) Managers.GameM.TriggerGameOver();
        if (Input.GetKeyDown(KeyCode.C)) Managers.GameM.TriggerGameClear();
        if (Input.GetKeyDown(KeyCode.N)) Managers.WaveM.PrepareNextWave();
    }
#endif
}
