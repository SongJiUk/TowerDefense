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

    /// <summary>웨이브 클리어 시 발행 (wave, bonusGold)</summary>
    public event Action<int, int> OnWaveComplete;

    /// <summary>스테이지 전체 클리어 시 발행</summary>
    public event Action OnAllWavesComplete;

    /// <summary>대기 후 첫 적 스폰 직전 발행 — 카운트다운 UI 닫기용</summary>
    public event Action OnWaveSpawnStart;

    /// <summary>중간 보스 또는 최종 보스 스폰 시 발행 (이름·연출용)</summary>
    public event Action<EnemyData> OnBossAppear;

    /// <summary>중간 보스 또는 최종 보스 스폰 시 발행 (HP바 추적용)</summary>
    public event Action<EnemyController> OnBossSpawned;

    /// <summary>보스 등장 연출(UI_BossAnnounce)이 끝났을 때 발행 — WaveStarter가 대기 해제용으로 사용</summary>
    public event Action OnBossAnnounceComplete;

    /// <summary>다음 웨이브 예고 데이터가 준비됐을 때 발행</summary>
    public event Action<WavePreview> OnNextWaveReady;

    /// <summary>즉시 시작 버튼 클릭 시 발행 — WaveStarter가 대기 취소</summary>
    public event Action OnEarlyStartRequested;

    // ─── 상태 ─────────────────────────────────────────────────────────────────

    public int CurrentWave => _currentWaveIndex + 1;
    public int TotalWaves => _stageData?.totalWaves ?? 0;
    public bool IsRunning { get; private set; }
    public bool IsInitialized => _stageData != null;
    public StageData CurrentStage => _stageData;
    public float LastWaveBonusMultiplier { get; private set; } = 1f;

    private StageData _stageData;
    private int _currentWaveIndex;
    private int _aliveCount;
    private CancellationTokenSource _cts;

    private readonly List<EnemyData> _preGenerated = new();

    /// <summary>WaveStarter가 씬 준비 직후 설정. 카운트다운 UI 표시에 사용.</summary>
    public float NextWaveDelay { get; set; } = 5f;

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
        _preGenerated.Clear();
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
        // BeginSpawning()은 UI 카운트다운 완료 후 호출됨
    }

    /// <summary>UI 카운트다운 완료 후 호출 — 적 스폰을 즉시 시작.</summary>
    public void BeginSpawning()
    {
        if (!IsRunning)
        {
            Debug.LogWarning("[WaveManager] StartNextWave()를 먼저 호출하세요.");
            return;
        }
        RunWave(_currentWaveIndex, _cts.Token).Forget();
    }

    /// <summary>Split 등 런타임 추가 적 등록 시 호출.</summary>
    public void RegisterExtraEnemy(int count) => _aliveCount += count;

    public void NotifyBossSpawned(EnemyController boss) => OnBossSpawned?.Invoke(boss);
    public void NotifyBossAppear(EnemyData data)        => OnBossAppear?.Invoke(data);
    public void NotifyBossAnnounceComplete()            => OnBossAnnounceComplete?.Invoke();

    /// <summary>다음 웨이브 _preGenerated 중 보스/중간보스 데이터. 없으면 null.</summary>
    public EnemyData NextWaveBossData
    {
        get
        {
            foreach (var e in _preGenerated)
                if (e != null && (e.enemyType == Define.EnemyType.Boss || e.enemyType == Define.EnemyType.MiddleBoss))
                    return e;
            return null;
        }
    }

    /// <summary>런타임 위치 지정 스폰 (SplitEnemy 등에서 사용).</summary>
    public EnemyController SpawnEnemyAt(EnemyData data, Vector3 position, float hpMultiplier, float speedMultiplier)
    {
        if (data == null) return null;
        GameObject go = Managers.PoolM.Pop(data.prefabKey);
        if (go == null) return null;
        go.transform.position = position;
        go.transform.rotation = Quaternion.identity;
        if (go.TryGetComponent(out EnemyController enemy))
        {
            enemy.Init(data, hpMultiplier, speedMultiplier);
            return enemy;
        }
        return null;
    }

    /// <summary>EnemyController가 사망 or 코어 도달 시 호출.</summary>
    public void OnEnemyRemoved()
    {
        _aliveCount = Mathf.Max(0, _aliveCount - 1);
        if (_aliveCount > 0 || !IsRunning) return;

        IsRunning = false;
        int cleared = CurrentWave;
        _currentWaveIndex++;

        LastWaveBonusMultiplier = Managers.GameM.waveBonusMultiplier;
        int waveBonus = Mathf.RoundToInt(cleared * 10 * LastWaveBonusMultiplier);
        Managers.GameM.AddGold(waveBonus);
        Managers.GameM.waveBonusMultiplier = 1f;

        Managers.AchievementM?.AddProgress("wave_10");
        Managers.AchievementM?.AddProgress("wave_30");
        Managers.AchievementM?.AddProgress("wave_50");
        OnWaveComplete?.Invoke(cleared, waveBonus);

        if (_currentWaveIndex >= _stageData.totalWaves)
        {
            OnAllWavesComplete?.Invoke();
            Managers.GameM.TriggerGameClear();
        }
        else
        {
            PrepareNextWave();
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsRunning = false;
    }

    /// <summary>다음 웨이브 적 구성을 미리 뽑아 OnNextWaveReady를 발행한다.
    /// WaveStarter가 씬 준비 완료 직후(1웨이브)와 각 웨이브 클리어 후 호출.</summary>
    public void PrepareNextWave()
    {
        if (_stageData == null) return;

        int waveIndex  = _currentWaveIndex;
        int waveNumber = waveIndex + 1;
        bool isBoss    = waveIndex == _stageData.totalWaves - 1;

        _preGenerated.Clear();

        if (isBoss)
        {
            if (_stageData.bossEnemy != null)
                _preGenerated.Add(_stageData.bossEnemy);

            int minionCount = _stageData.bossWaveMinions;
            for (int i = 0; i < minionCount; i++)
            {
                var e = SelectEnemy(waveNumber);
                if (e != null) _preGenerated.Add(e);
            }

            var preview = new WavePreview
            {
                WaveNumber       = waveNumber,
                IsBossWave       = true,
                EnemyGroups      = GroupEnemies(_preGenerated),
                BaseRewardGold   = waveNumber * 10,
                EstimatedSeconds = Mathf.RoundToInt(2f + minionCount * _stageData.spawnInterval),
                WaveDelay        = NextWaveDelay,
            };
            OnNextWaveReady?.Invoke(preview);
        }
        else
        {
            int count = CalcSpawnCount(waveIndex);
            for (int i = 0; i < count; i++)
            {
                var e = SelectEnemy(waveNumber);
                if (e != null) _preGenerated.Add(e);
            }

            var middleBoss = GetMiddleBossForWave(waveNumber);
            if (middleBoss != null)
                _preGenerated.Add(middleBoss);

            float interval = _stageData.spawnInterval;
            if (waveNumber >= 7) interval = Mathf.Max(0.8f, interval - 0.5f);

            var preview = new WavePreview
            {
                WaveNumber       = waveNumber,
                IsBossWave       = false,
                EnemyGroups      = GroupEnemies(_preGenerated),
                BaseRewardGold   = waveNumber * 10,
                EstimatedSeconds = Mathf.RoundToInt(count * interval),
                WaveDelay        = NextWaveDelay,
            };
            OnNextWaveReady?.Invoke(preview);
        }
    }

    /// <summary>즉시 시작 버튼 클릭 시 호출. 웨이브 보너스 1.5배 적용 후 대기 취소 요청.</summary>
    public void RequestEarlyStart()
    {
        Managers.GameM.waveBonusMultiplier *= 1.5f;
        OnEarlyStartRequested?.Invoke();
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

        OnWaveSpawnStart?.Invoke();

        float hpMult = CalcHpMultiplier(waveIndex) * Managers.GameM.globalEnemyHpMultiplier;
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
        int count = _preGenerated.Count > 0 ? _preGenerated.Count : CalcSpawnCount(waveIndex);
        _aliveCount = count;

        float interval = _stageData.spawnInterval;
        if (waveNumber >= 7) interval = Mathf.Max(0.8f, interval - 0.5f);

        for (int i = 0; i < count; i++)
        {
            if (token.IsCancellationRequested) return;

            EnemyData data = i < _preGenerated.Count ? _preGenerated[i] : SelectEnemy(waveNumber);
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

        // 잡몹 후속 스폰 — preGenerated[0]=보스, [1+]=잡몹
        int waveNumber = _stageData.totalWaves;
        for (int i = 0; i < minionCount; i++)
        {
            if (token.IsCancellationRequested) return;

            int preIdx    = i + 1;
            EnemyData data = preIdx < _preGenerated.Count ? _preGenerated[preIdx] : SelectEnemy(waveNumber);
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

        GameObject go = Managers.PoolM.Pop(data.prefabKey);
        if (go == null) return;

        go.transform.position = Managers.SpawnPoint.transform.position;
        go.transform.rotation = Quaternion.identity;

        EnemyController spawnedEnemy = null;
        if (go.TryGetComponent(out EnemyController enemy))
        {
            enemy.Init(data, hpMultiplier, speedMultiplier);
            spawnedEnemy = enemy;
        }

        if (data.enemyType == Define.EnemyType.MiddleBoss || data.enemyType == Define.EnemyType.Boss)
        {
            if (spawnedEnemy != null) OnBossSpawned?.Invoke(spawnedEnemy);
        }
    }

    private static List<(EnemyData data, int count)> GroupEnemies(List<EnemyData> list)
    {
        var result = new List<(EnemyData data, int count)>();
        foreach (var e in list)
        {
            if (e == null) continue;
            bool found = false;
            for (int j = 0; j < result.Count; j++)
            {
                if (result[j].data == e)
                {
                    result[j] = (result[j].data, result[j].count + 1);
                    found = true;
                    break;
                }
            }
            if (!found) result.Add((e, 1));
        }
        return result;
    }

    /// <summary>난이도별 등장 웨이브 이상이면 이 스테이지의 MiddleBoss를 웨이브당 정확히 1마리 반환.</summary>
    private EnemyData GetMiddleBossForWave(int waveNumber)
    {
        int middleBossFromWave = Managers.DifficultyM?.MiddleBossFromWave ?? 5;
        if (waveNumber < middleBossFromWave) return null;

        foreach (var entry in _stageData.enemyPool)
        {
            if (entry.enemyData != null && entry.enemyData.enemyType == Define.EnemyType.MiddleBoss)
                return entry.enemyData;
        }
        return null;
    }

    /// <summary>현재 웨이브에서 등장 가능한 적 중 가중치 기반 랜덤 선택.
    /// MiddleBoss는 웨이브당 1마리만 별도 보장되므로 여기서는 제외한다 (GetMiddleBossForWave 참고).</summary>
    private EnemyData SelectEnemy(int waveNumber)
    {
        if (_stageData.enemyPool == null || _stageData.enemyPool.Length == 0) return null;

        // 이 웨이브에서 등장 가능한 적 필터링
        float totalWeight = 0f;
        var available = new List<StageEnemyEntry>();

        foreach (var entry in _stageData.enemyPool)
        {
            if (entry.enemyData == null) continue;
            if (entry.enemyData.enemyType == Define.EnemyType.MiddleBoss) continue;

            if (entry.fromWave <= waveNumber)
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
