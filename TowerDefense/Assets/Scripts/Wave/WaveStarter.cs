using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 씬에 하나 배치. LoadingScene에서 WaveM.Init()이 완료된 상태로 진입하므로
/// 여기서는 이벤트 구독과 웨이브 시작만 담당한다.
/// </summary>
public class WaveStarter : MonoBehaviour
{
    [Tooltip("true: Start()에서 자동 시작 / false: StartWave() 버튼으로 수동 시작")]
    [SerializeField] private bool _autoStart = true;

    [Tooltip("웨이브 클리어 후 다음 웨이브 시작까지 대기 시간 (초)")]
    [SerializeField] private float _nextWaveDelay = 3f;

    void Start()
    {
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

    public void StartWave() => Managers.WaveM.StartNextWave();

    private void OnWaveStart(int wave)
    {
        Debug.Log($"[Wave] {wave} / {Managers.WaveM.TotalWaves} 웨이브 시작");
    }

    private void OnWaveComplete(int wave, int bonus)
    {
        Debug.Log($"[Wave] {wave}웨이브 클리어! +{bonus}G");

        if (_autoStart)
            StartNextWaveDelayed().Forget();
    }

    private async UniTaskVoid StartNextWaveDelayed()
    {
        await UniTask.Delay(
            (int)(_nextWaveDelay * 1000),
            cancellationToken: destroyCancellationToken
        );
        Managers.WaveM.StartNextWave();
    }

    private void OnAllWavesComplete()
    {
        Debug.Log("[Wave] 스테이지 클리어!");
    }
}
