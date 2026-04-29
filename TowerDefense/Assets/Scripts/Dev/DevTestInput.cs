#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// 테스트 전용. 1~7 키로 적 즉시 소환 / 골드 무한.
/// GameScene에 빈 오브젝트 만들고 붙여서 사용.
/// </summary>
public class DevTestInput : MonoBehaviour
{
    [Tooltip("키 1~7에 매핑할 EnemyData (순서대로)")]
    [SerializeField] private EnemyData[] _enemies = new EnemyData[7];

    void Awake()
    {
        Managers.GameM.TestInfiniteGold = true;
        Managers.GameM.ResetGold(9999);

        var waveStarter = FindObjectOfType<WaveStarter>();
        if (waveStarter != null) waveStarter.enabled = false;
    }

    void Update()
    {
        for (int i = 0; i < 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SpawnEnemy(i);
        }

        if (Input.GetKeyDown(KeyCode.L))
            LevelUp();
    }

    private void LevelUp()
    {
        var gameM = Managers.GameM;
        if (gameM.LevelData == null) return;
        int needed = gameM.LevelData.GetRequiredExp(gameM.Level) - gameM.CurrentExp;
        gameM.AddExp(needed);
    }

    private void SpawnEnemy(int index)
    {
        if (index >= _enemies.Length || _enemies[index] == null)
        {
            Debug.LogWarning($"[DevTest] 슬롯 {index + 1}에 EnemyData가 없습니다.");
            return;
        }

        if (Managers.SpawnPoint == null)
        {
            Debug.LogWarning("[DevTest] SpawnPoint가 씬에 없습니다.");
            return;
        }

        EnemyData data = _enemies[index];
        GameObject go = Managers.PoolM.Pop(data.prefabKey);
        if (go == null) return;

        go.transform.position = Managers.SpawnPoint.transform.position;
        go.transform.rotation = Quaternion.identity;

        if (go.TryGetComponent(out EnemyController enemy))
            enemy.Init(data, 1f, 1f);
    }
}
#endif
