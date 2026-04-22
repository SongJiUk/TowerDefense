using System;
using UnityEngine;

/// <summary>
/// 스테이지 1개의 웨이브 설정 데이터. ScriptableObject.
/// GameDataGenerator로 4개 스테이지 에셋이 자동 생성된다.
/// WaveStarter가 이 데이터를 WaveManager.Init()에 전달해 웨이브를 구성한다.
/// 메뉴: Create > TowerDefense > StageData
/// </summary>
[CreateAssetMenu(menuName = "TowerDefense/StageData")]
public class StageData : ScriptableObject
{
    [Header("웨이브 기본")]
    public int totalWaves = 10;

    [Header("스테이지 배율")]
    [Tooltip("스테이지 기준 HP 배율\n숲=1.0 / 사막=1.6 / 겨울=2.5 / 던전=3.8")]
    public float stageHpMultiplier = 1f;

    [Tooltip("적 스폰 간격 (초)")]
    public float spawnInterval = 1.5f;

    [Tooltip("웨이브 시작 전 대기 (초)")]
    public float waveStartDelay = 3f;

    [Header("적 풀 — 등장 조건만 설정")]
    [Tooltip("이 스테이지에 등장하는 적 종류와 등장 시작 웨이브 지정")]
    public StageEnemyEntry[] enemyPool;

    [Header("보스 웨이브 (마지막 웨이브 자동 적용)")]
    public EnemyData bossEnemy;

    [Tooltip("보스 등장 2초 후 함께 스폰될 잡몹 수")]
    public int bossWaveMinions = 5;

    [Header("UI 테마")]
    public Color uiBarBG;
    public Color uiLineColor;
    public Color uiAccentColor;
}

/// <summary>
/// 스테이지에 등장할 적 한 종류의 설정.
/// fromWave 이후 웨이브에서 spawnWeight 가중치로 랜덤 선택된다.
/// </summary>
[Serializable]
public class StageEnemyEntry
{
    public EnemyData enemyData;

    [Tooltip("이 적이 등장하기 시작하는 웨이브 번호 (1부터)")]
    public int fromWave = 1;

    [Range(0.1f, 5f), Tooltip("스폰 가중치 — 높을수록 더 자주 나옴")]
    public float spawnWeight = 1f;
}
