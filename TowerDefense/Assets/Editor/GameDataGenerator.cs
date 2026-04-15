using UnityEditor;
using UnityEngine;

/// <summary>
/// 상단 메뉴 TowerDefense > 게임 데이터 자동 생성 클릭 한 번으로
/// 적 5종 + 스테이지 4개 ScriptableObject 에셋을 자동 생성합니다.
/// 프리팹 슬롯만 직접 연결해주세요.
/// </summary>
public static class GameDataGenerator
{
    private const string ENEMY_PATH = "Assets/Data/Enemies";
    private const string STAGE_PATH = "Assets/Data/Stages";

    [MenuItem("TowerDefense/게임 데이터 자동 생성")]
    public static void GenerateAll()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder(ENEMY_PATH);
        EnsureFolder(STAGE_PATH);

        // ── 적 데이터 생성 ─────────────────────────────────────────────────
        var basic  = CreateEnemy("Enemy_Basic",  Define.EnemyType.Basic,  hp: 80f,   speed: 2.5f, reward: 3);
        var tank   = CreateEnemy("Enemy_Tank",   Define.EnemyType.Tank,   hp: 300f,  speed: 1.5f, reward: 8);
        var runner = CreateEnemy("Enemy_Runner", Define.EnemyType.Runner, hp: 50f,   speed: 5.0f, reward: 4);
        var flyer  = CreateEnemy("Enemy_Flyer",  Define.EnemyType.Flyer,  hp: 60f,   speed: 3.5f, reward: 5);
        var boss   = CreateEnemy("Enemy_Boss",   Define.EnemyType.Boss,   hp: 1500f, speed: 1.8f, reward: 50);

        // ── 스테이지 데이터 생성 ───────────────────────────────────────────
        // Stage 1 — 숲 (HP ×1.0) : Basic + Tank
        CreateStage("Stage1_Forest", totalWaves: 10, hpMult: 1.0f, spawnInterval: 1.5f,
            bossEnemy: boss, bossMinions: 5,
            enemies: new[]
            {
                Entry(basic,  fromWave: 1, weight: 4f),
                Entry(tank,   fromWave: 4, weight: 1f),
            });

        // Stage 2 — 사막 (HP ×1.6) : Basic + Runner + Tank
        CreateStage("Stage2_Desert", totalWaves: 10, hpMult: 1.6f, spawnInterval: 1.4f,
            bossEnemy: boss, bossMinions: 6,
            enemies: new[]
            {
                Entry(basic,  fromWave: 1, weight: 3f),
                Entry(runner, fromWave: 3, weight: 2f),
                Entry(tank,   fromWave: 5, weight: 1f),
            });

        // Stage 3 — 겨울 (HP ×2.5) : Runner + Tank + Flyer
        CreateStage("Stage3_Winter", totalWaves: 10, hpMult: 2.5f, spawnInterval: 1.3f,
            bossEnemy: boss, bossMinions: 7,
            enemies: new[]
            {
                Entry(runner, fromWave: 1, weight: 3f),
                Entry(tank,   fromWave: 1, weight: 2f),
                Entry(flyer,  fromWave: 4, weight: 2f),
            });

        // Stage 4 — 던전 (HP ×3.8) : 전 종류 + 보스 강화
        CreateStage("Stage4_Dungeon", totalWaves: 10, hpMult: 3.8f, spawnInterval: 1.2f,
            bossEnemy: boss, bossMinions: 8,
            enemies: new[]
            {
                Entry(basic,  fromWave: 1, weight: 2f),
                Entry(runner, fromWave: 1, weight: 2f),
                Entry(tank,   fromWave: 1, weight: 2f),
                Entry(flyer,  fromWave: 3, weight: 2f),
            });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[GameDataGenerator] 완료! " +
                  "Assets/Data/Enemies 와 Assets/Data/Stages 를 확인하세요.\n" +
                  "각 EnemyData 에셋의 [Prefab] 슬롯에 프리팹을 연결해주세요.");

        EditorUtility.DisplayDialog(
            "게임 데이터 생성 완료",
            "적 5종 + 스테이지 4개 에셋이 생성되었습니다.\n\n" +
            "다음 작업:\n" +
            "Assets/Data/Enemies 에서 각 EnemyData의\n" +
            "[Prefab] 슬롯에 프리팹을 연결해주세요.",
            "확인"
        );
    }

    // ─── 헬퍼 ─────────────────────────────────────────────────────────────────

    private static EnemyData CreateEnemy(
        string fileName, Define.EnemyType type,
        float hp, float speed, int reward)
    {
        string path = $"{ENEMY_PATH}/{fileName}.asset";
        var data = LoadOrCreate<EnemyData>(path);

        data.enemyName    = fileName;
        data.enemyType    = type;
        data.baseHp       = hp;
        data.baseMoveSpeed = speed;
        data.baseReward   = reward;

        EditorUtility.SetDirty(data);
        return data;
    }

    private static void CreateStage(
        string fileName, int totalWaves, float hpMult,
        float spawnInterval, EnemyData bossEnemy, int bossMinions,
        StageEnemyEntry[] enemies)
    {
        string path = $"{STAGE_PATH}/{fileName}.asset";
        var data = LoadOrCreate<StageData>(path);

        data.totalWaves        = totalWaves;
        data.stageHpMultiplier = hpMult;
        data.spawnInterval     = spawnInterval;
        data.waveStartDelay    = 3f;
        data.enemyPool         = enemies;
        data.bossEnemy         = bossEnemy;
        data.bossWaveMinions   = bossMinions;

        EditorUtility.SetDirty(data);
    }

    private static StageEnemyEntry Entry(EnemyData data, int fromWave, float weight)
        => new StageEnemyEntry { enemyData = data, fromWave = fromWave, spawnWeight = weight };

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            int lastSlash = path.LastIndexOf('/');
            string parent = path.Substring(0, lastSlash);
            string folder = path.Substring(lastSlash + 1);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
