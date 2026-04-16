using System;
using UnityEngine;

/// <summary>
/// 게임 전역 싱글톤. @Managers GameObject를 자동 생성하고 DontDestroyOnLoad로 유지한다.
/// 씬 어디서든 Managers.Grid, Managers.WaveM 등으로 핵심 시스템에 접근 가능.
/// </summary>
public class Managers : MonoBehaviour
{
    static Managers instance;
    static bool init = false;

    readonly PoolManager poolManager = new();
    readonly ResourceManager resourceManager = new();
    readonly UIManager uiManager = new();
    UpdateManager updateManager = null;

    // ─── 전역 참조 ────────────────────────────────────────────────────────────

    /// <summary>현재 활성 씬의 GridSystem. GridSystem.OnEnable/OnDisable에서 자동 등록/해제.</summary>
    public static GridSystem Grid;

    /// <summary>A* 경로탐색 인스턴스. PathFinder.FindPath(), RecalculatePath()로 사용.</summary>
    public static PathFinder Path { get; private set; } = new PathFinder();

    /// <summary>웨이브 관리 인스턴스. WaveStarter.Start()에서 Init() 후 사용.</summary>
    public static WaveManager WaveM { get; private set; } = new WaveManager();

    /// <summary>적 스폰 위치. SpawnPoint.OnEnable/OnDisable에서 자동 등록/해제.</summary>
    public static SpawnPoint SpawnPoint;
    public static EndPoint EndPoint;

    /// <summary>코어 오브젝트. Core.OnEnable/OnDisable에서 자동 등록/해제. Road 타일 위에 배치 필수.</summary>
    public static Transform CoreTransform;
    public static IDamageable ICore;

    // ─── 골드 ────────────────────────────────────────────────────────────────

    /// <summary>현재 골드. UI_GameScene이 OnGoldChanged를 구독해 자동으로 HUD 갱신.</summary>
    public static int Gold { get; private set; } = 150;

    /// <summary>골드가 변경될 때 발행. 현재 골드값을 파라미터로 전달.</summary>
    public static event Action<int> OnGoldChanged;

    /// <summary>골드 획득. 적 사망 시 EnemyController.Die()에서 호출 예정.</summary>
    public static void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    /// <summary>
    /// 골드 소비. 타워 설치·업그레이드 시 호출.
    /// 골드가 부족하면 false를 반환하고 차감하지 않는다.
    /// </summary>
    public static bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    /// <summary>씬 시작 시 골드 초기화.</summary>
    public static void ResetGold(int startGold = 150)
    {
        Gold = startGold;
        OnGoldChanged?.Invoke(Gold);
    }

    // ─── 매니저 접근자 ────────────────────────────────────────────────────────

    public static PoolManager PoolM { get { return Instance?.poolManager; } }
    public static ResourceManager ResourceM { get { return Instance?.resourceManager; } }
    public static UIManager UIM { get { return Instance?.uiManager; } }
    public static UpdateManager UpdateM { get { return Instance?.updateManager; } }

    public static Managers Instance
    {
        get
        {
            if (!init)
            {
                init = true;
                GameObject go = GameObject.Find("@Managers");

                if (go == null)
                {
                    go = new GameObject() { name = "@Managers" };
                    go.AddComponent<Managers>();
                }

                DontDestroyOnLoad(go);
                instance = go.GetComponent<Managers>();
                instance.updateManager = go.AddComponent<UpdateManager>();
            }

            return instance;
        }
    }

    /// <summary>씬 전환 시 Pool·Resource·UI 매니저를 초기화.</summary>
    public static void Clear()
    {
        PoolM.Clear();
        ResourceM.Clear();
        UIM.Clear();
    }
}
