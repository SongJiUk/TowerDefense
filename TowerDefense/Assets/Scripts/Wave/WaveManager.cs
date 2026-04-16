using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 웨이브 순차 실행 · 적 스폰 · 클리어 판정을 담당.
/// 웨이브 번호로 스폰 수·HP 배율·속도를 공식으로 자동 계산.
/// MonoBehaviour 아님 — Managers.WaveM 으로 접근.
/// </summary>
public class WaveManager
{
    // ─── 이벤트 ───────────────────────────────────────────────────────────────

    /// <summary>웨이브 시작 시 발행 (1-based)</summary>
    public event Action<int> OnWaveStart;

    /// <summary>웨이브 클리어 시 발행 (1-based)</summary>
    public event Action<int> OnWaveComplete;

    /// <summary>스테이지 전체 클리어 시 발행</summary>
    public event Action OnAllWavesComplete;

    // ─── 상태 ─────────────────────────────────────────────────────────────────

    public int CurrentWave => _currentWaveIndex + 1;
    public int TotalWaves => _stageData?.totalWaves ?? 0;
    public bool IsRunning { get; private set; }

    private StageData _stageData;
    private int _currentWaveIndex;
    private int _aliveCount;
    private CancellationTokenSource _cts;

    // ─── 공식 상수 ────────────────────────────────────────────────────────────

    /// <summary>웨이브당 HP 증가율 (0.15 = 15%)</summary>
    private const float HP_SCALE_PER_WAVE = 0.15f;

    /// <summary>Wave 5, 8에서 이동속도 +0.1씩 증가</summary>
    private static readonly int[] SPEED_BOOST_WAVES = { 5, 8 };
    private const float SPEED_BOOST_AMOUNT = 0.1f;

    // ─── 공개 API ─────────────────────────────────────────────────────────────

    public void Init(StageData stageData)
    {
        _stageData = stageData;
        _currentWaveIndex = 0;
        _aliveCount = 0;
        IsRunning = false;
    }

    public void StartNextWave()
    {
        if (_stageData == null)
        {
            Debug.LogError("[WaveManager] Init()을 먼저 호출하세요.");
            return;
        }
        if (IsRunning)
        {
            Debug.LogWarning("[WaveManager] 이미 웨이브가 진행 중입니다.");
            return;
        }
        if (_currentWaveIndex >= _stageData.totalWaves)
        {
            Debug.LogWarning("[WaveManager] 더 이상 웨이브가 없습니다.");
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        IsRunning = true;
        OnWaveStart?.Invoke(CurrentWave);

        RunWave(_currentWaveIndex, _cts.Token).Forget();
    }

    /// <summary>EnemyController가 사망 or 코어 도달 시 호출.</summary>
    public void OnEnemyRemoved()
    {
        _aliveCount = Mathf.Max(0, _aliveCount - 1);
        if (_aliveCount > 0 || !IsRunning) return;

        IsRunning = false;
        int cleared = CurrentWave;
        _currentWaveIndex++;

        OnWaveComplete?.Invoke(cleared);

        if (_currentWaveIndex >= _stageData.totalWaves)
            OnAllWavesComplete?.Invoke();
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsRunning = false;
    }

    // ─── 공식 계산 ────────────────────────────────────────────────────────────

    /// <summary>이번 웨이브 적 스폰 수: 3 + waveIndex × 2</summary>
    private int CalcSpawnCount(int waveIndex) => 3 + waveIndex * 2;

    /// <summary>이번 웨이브 HP 배율: stageMultiplier × (1 + waveIndex × 0.15)</summary>
    private float CalcHpMultiplier(int waveIndex)
        => _stageData.stageHpMultiplier * (1f + waveIndex * HP_SCALE_PER_WAVE);

    /// <summary>이번 웨이브 속도 보정: Wave 5, 8마다 +0.1</summary>
    private float CalcSpeedMultiplier(int waveIndex)
    {
        int waveNumber = waveIndex + 1;
        float bonus = 0f;
        foreach (int boostWave in SPEED_BOOST_WAVES)
        {
            if (waveNumber >= boostWave)
                bonus += SPEED_BOOST_AMOUNT;
        }
        return 1f + bonus;
    }

    // ─── 스폰 로직 ────────────────────────────────────────────────────────────

    private async UniTaskVoid RunWave(int waveIndex, CancellationToken token)
    {
        bool isBossWave = (waveIndex == _stageData.totalWaves - 1);

        await UniTask.Delay(
            TimeSpan.FromSeconds(_stageData.waveStartDelay),
            cancellationToken: token
        );

        float hpMult = CalcHpMultiplier(waveIndex);
        float speedMult = CalcSpeedMultiplier(waveIndex);
        int waveNumber = waveIndex + 1;

        if (isBossWave)
            await SpawnBossWave(hpMult, speedMult, token);
        else
            await SpawnNormalWave(waveIndex, waveNumber, hpMult, speedMult, token);
    }

    private async UniTask SpawnNormalWave(
        int waveIndex, int waveNumber,
        float hpMult, float speedMult,
        CancellationToken token)
    {
        int count = CalcSpawnCount(waveIndex);
        _aliveCount = count;

        float interval = _stageData.spawnInterval;
        // 후반 웨이브(7~9)는 스폰 간격 단축
        if (waveNumber >= 7) interval = Mathf.Max(0.8f, interval - 0.5f);

        for (int i = 0; i < count; i++)
        {
            if (token.IsCancellationRequested) return;

            EnemyData data = SelectEnemy(waveNumber);
            if (data != null)
                SpawnEnemy(data, hpMult, speedMult);

            await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
        }
    }

    private async UniTask SpawnBossWave(float hpMult, float speedMult, CancellationToken token)
    {
        int minionCount = _stageData.bossWaveMinions;
        // 보스 1 + 잡몹
        _aliveCount = 1 + minionCount;

        // 보스 먼저
        if (_stageData.bossEnemy != null)
            SpawnEnemy(_stageData.bossEnemy, hpMult * 2f, speedMult);

        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: token);

        // 잡몹 후속 스폰
        int waveNumber = _stageData.totalWaves;
        for (int i = 0; i < minionCount; i++)
        {
            if (token.IsCancellationRequested) return;

            EnemyData data = SelectEnemy(waveNumber);
            if (data != null)
                SpawnEnemy(data, hpMult, speedMult);

            await UniTask.Delay(
                TimeSpan.FromSeconds(_stageData.spawnInterval),
                cancellationToken: token
            );
        }
    }

    private void SpawnEnemy(EnemyData data, float hpMultiplier, float speedMultiplier)
    {
        if (Managers.SpawnPoint == null)
        {
            Debug.LogError("[WaveManager] SpawnPoint가 씬에 없습니다.");
            return;
        }

        GameObject go = Managers.PoolM.Pop(data.prefab);
        if (go == null) return;

        go.transform.position = Managers.SpawnPoint.transform.position;
        go.transform.rotation = Quaternion.identity;

        if (go.TryGetComponent(out EnemyController enemy))
            enemy.Init(data, hpMultiplier, speedMultiplier);
    }

    /// <summary>현재 웨이브에서 등장 가능한 적 중 가중치 기반 랜덤 선택.</summary>
    private EnemyData SelectEnemy(int waveNumber)
    {
        if (_stageData.enemyPool == null || _stageData.enemyPool.Length == 0) return null;

        // 이 웨이브에서 등장 가능한 적 필터링
        float totalWeight = 0f;
        var available = new List<StageEnemyEntry>();

        foreach (var entry in _stageData.enemyPool)
        {
            if (entry.enemyData != null && entry.fromWave <= waveNumber)
            {
                available.Add(entry);
                totalWeight += entry.spawnWeight;
            }
        }

        if (available.Count == 0) return null;

        // 가중치 랜덤
        float rand = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var entry in available)
        {
            cumulative += entry.spawnWeight;
            if (rand <= cumulative) return entry.enemyData;
        }

        return available[available.Count - 1].enemyData;
    }
}
