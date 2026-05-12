using Cysharp.Threading.Tasks;
using System.Threading;
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
    [SerializeField] private float _nextWaveDelay = 10f;

    private CancellationTokenSource _delayCts;
    private bool _announceComplete;

    void Start()
    {
        Managers.WaveM.OnWaveStart              += OnWaveStart;
        Managers.WaveM.OnWaveComplete           += OnWaveComplete;
        Managers.WaveM.OnAllWavesComplete       += OnAllWavesComplete;
        Managers.WaveM.OnEarlyStartRequested    += OnEarlyStartRequested;
        Managers.WaveM.OnBossAnnounceComplete   += () => _announceComplete = true;

        if (_autoStart)
            WaitAndStart().Forget();
    }

    private async UniTaskVoid WaitAndStart()
    {
        await GameSceneBootstrap.ReadyTask;
        float stageDelay = Managers.WaveM.CurrentStage?.waveStartDelay ?? _nextWaveDelay;
        Managers.WaveM.NextWaveDelay = stageDelay;
        _nextWaveDelay = stageDelay;
        Managers.WaveM.PrepareNextWave();
        StartNextWaveDelayed().Forget();
    }

    void Update() => Managers.GameM.TickTimer(Time.deltaTime);

    void OnDestroy()
    {
        Managers.WaveM.OnWaveStart           -= OnWaveStart;
        Managers.WaveM.OnWaveComplete        -= OnWaveComplete;
        Managers.WaveM.OnAllWavesComplete    -= OnAllWavesComplete;
        Managers.WaveM.OnEarlyStartRequested -= OnEarlyStartRequested;
        Managers.WaveM.OnBossAnnounceComplete -= () => _announceComplete = true;
        _delayCts?.Cancel();
        _delayCts?.Dispose();
    }

    public void DisableAutoStart() => _autoStart = false;
    public void StartWave()        => Managers.WaveM.StartNextWave();

    // ─── 이벤트 핸들러 ────────────────────────────────────────────────────────

    private void OnWaveStart(int wave)
    {
        if (wave == 1) Managers.GameM.StartTimer();
        Debug.Log($"[Wave] {wave} / {Managers.WaveM.TotalWaves} 웨이브 시작");
    }

    private void OnWaveComplete(int wave, int bonus)
    {
        Debug.Log($"[Wave] {wave}웨이브 클리어! +{bonus}G");
        // PrepareNextWave는 WaveManager.OnEnemyRemoved에서 이미 호출됨
        if (_autoStart)
            StartNextWaveDelayed().Forget();
    }

    private void OnAllWavesComplete()
    {
        Debug.Log("[Wave] 스테이지 클리어!");
    }

    /// <summary>즉시 시작 버튼 → 대기 취소 후 보스면 어나운스, 아니면 바로 시작.</summary>
    private void OnEarlyStartRequested()
    {
        _delayCts?.Cancel();
        StartAfterAnnounceIfNeeded().Forget();
    }

    // ─── 대기 ─────────────────────────────────────────────────────────────────

    private async UniTaskVoid StartNextWaveDelayed()
    {
        _delayCts?.Cancel();
        _delayCts?.Dispose();
        _delayCts = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(
                (int)(_nextWaveDelay * 1000),
                cancellationToken: _delayCts.Token
            );
            await StartAfterAnnounceIfNeeded();
        }
        catch (System.OperationCanceledException) { }
    }

    private async UniTask StartAfterAnnounceIfNeeded()
    {
        var bossData = Managers.WaveM.NextWaveBossData;
        if (bossData != null)
        {
            _announceComplete = false;
            Managers.WaveM.NotifyBossAppear(bossData);
            await UniTask.WaitUntil(() => _announceComplete);
        }
        Managers.WaveM.StartNextWave();
    }
}
