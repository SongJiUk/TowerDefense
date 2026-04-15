using UnityEngine;

/// <summary>
/// 씬에 하나 배치해 WaveManager에 StageData를 연결하는 브릿지.
/// Inspector에서 StageData를 지정하면 Start()에서 WaveManager를 초기화하고 이벤트를 구독한다.
///
/// Inspector 설정:
///   _stageData  — 해당 씬의 StageData 에셋 연결 (필수)
///   _autoStart  — true: 즉시 웨이브 시작 / false: 버튼으로 수동 시작
///
/// _autoStart = false일 때 버튼 OnClick에 StartWave() 메서드를 직접 연결 가능.
/// </summary>
public class WaveStarter : MonoBehaviour
{
    [SerializeField] private StageData _stageData;

    [Tooltip("true: Start()에서 자동 시작 / false: StartWave() 버튼으로 수동 시작")]
    [SerializeField] private bool _autoStart = true;

    void Start()
    {
        if (_stageData == null)
        {
            Debug.LogError("[WaveStarter] StageData가 연결되지 않았습니다.");
            return;
        }

        Managers.WaveM.Init(_stageData);

        Managers.WaveM.OnWaveStart        += OnWaveStart;
        Managers.WaveM.OnWaveComplete     += OnWaveComplete;
        Managers.WaveM.OnAllWavesComplete += OnAllWavesComplete;

        if (_autoStart)
            Managers.WaveM.StartNextWave();
    }

    void OnDestroy()
    {
        Managers.WaveM.OnWaveStart        -= OnWaveStart;
        Managers.WaveM.OnWaveComplete     -= OnWaveComplete;
        Managers.WaveM.OnAllWavesComplete -= OnAllWavesComplete;
    }

    /// <summary>웨이브 시작 버튼 OnClick에 연결 가능. _autoStart = false일 때 사용.</summary>
    public void StartWave() => Managers.WaveM.StartNextWave();

    private void OnWaveStart(int wave)
    {
        Debug.Log($"[Wave] {wave} / {Managers.WaveM.TotalWaves} 웨이브 시작");
    }

    private void OnWaveComplete(int wave)
    {
        Debug.Log($"[Wave] {wave}웨이브 클리어!");

        if (_autoStart)
            Managers.WaveM.StartNextWave();
    }

    private void OnAllWavesComplete()
    {
        Debug.Log("[Wave] 스테이지 클리어!");
        // TODO: 스테이지 클리어 UI 표시
    }
}
