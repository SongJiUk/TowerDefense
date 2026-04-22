using UnityEditor;
using UnityEngine;

/// <summary>
/// 상단 메뉴 TowerDefense > 타워 데이터 자동 생성 클릭 한 번으로
/// 타워 6종 ScriptableObject를 생성/갱신합니다.
/// addressableKey / projectilePrefabKey 는 기존 값을 유지하고
/// 업그레이드 수치만 덮어씁니다.
/// </summary>
public static class TowerDataGenerator
{
    private const string TOWER_PATH = "Assets/Data/Towers";

    [MenuItem("TowerDefense/타워 데이터 자동 생성")]
    public static void GenerateAll()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder(TOWER_PATH);

        // ── 궁수탑 (Basic) ─ 균형형 ────────────────────────────────────────────
        var basic = LoadOrCreate<TowerData>($"{TOWER_PATH}/Tower_Basic.asset");
        SetBase(basic, "궁수탑", Define.TowerType.Basic,
            damage: 15f, speed: 1.2f, range: 5f, buildCost: 80);
        basic.damageUpgrades = Upgrades(
            ("단련된 날",  "+25% 공격력",  80, 1.25f),
            ("강철 화살",  "+60% 공격력", 160, 1.60f),
            ("용의 숨결",  "+120% 공격력",320, 2.20f));
        basic.rangeUpgrades = Upgrades(
            ("긴 활",     "+20% 사거리",  60, 1.20f),
            ("매의 눈",   "+45% 사거리", 120, 1.45f),
            ("하늘지배",  "+80% 사거리", 240, 1.80f));
        basic.speedUpgrades = Upgrades(
            ("빠른 손",   "+30% 속도",   80, 1.30f),
            ("바람의 궁", "+65% 속도",  160, 1.65f),
            ("폭풍 의지", "+120% 속도", 320, 2.20f));
        EditorUtility.SetDirty(basic);

        // ── 캐논탑 (Cannon) ─ 광역/고데미지형 ──────────────────────────────────
        var cannon = LoadOrCreate<CannonTowerData>($"{TOWER_PATH}/Tower_Cannon.asset");
        SetBase(cannon, "캐논탑", Define.TowerType.Cannon,
            damage: 40f, speed: 0.6f, range: 5f, buildCost: 150);
        cannon.splashRadius = 1.5f;
        cannon.damageUpgrades = Upgrades(
            ("강화 화약",   "+30% 공격력",  120, 1.30f),
            ("폭발 장약",   "+70% 공격력",  240, 1.70f),
            ("대지분쇄",    "+150% 공격력", 480, 2.50f));
        cannon.rangeUpgrades = Upgrades(
            ("장포신",       "+20% 사거리",  80, 1.20f),
            ("원거리포",     "+45% 사거리", 160, 1.45f),
            ("극한 사정거리","+80% 사거리", 320, 1.80f));
        cannon.speedUpgrades = Upgrades(
            ("재장전 훈련",  "+25% 속도",  120, 1.25f),
            ("기계 장전",    "+55% 속도",  240, 1.55f),
            ("연속 사격",    "+100% 속도", 480, 2.00f));
        EditorUtility.SetDirty(cannon);

        // ── 슬로우탑 (Slow) ─ 유틸/사거리형 ────────────────────────────────────
        var slow = LoadOrCreate<SlowTowerData>($"{TOWER_PATH}/Tower_Slow.asset");
        SetBase(slow, "슬로우탑", Define.TowerType.Slow,
            damage: 5f, speed: 1.0f, range: 5f, buildCost: 100);
        slow.slowMultiplier = 0.5f;
        slow.slowDuration   = 2.0f;
        slow.damageUpgrades = Upgrades(
            ("얼음 파편",   "+25% 공격력",  60, 1.25f),
            ("냉기 강화",   "+50% 공격력", 120, 1.50f),
            ("절대영도",    "+100% 공격력",240, 2.00f));
        slow.rangeUpgrades = Upgrades(
            ("확산 냉기",   "+25% 사거리",  60, 1.25f),
            ("광역 빙하",   "+55% 사거리", 120, 1.55f),
            ("영역 지배",   "+100% 사거리",240, 2.00f));
        slow.speedUpgrades = Upgrades(
            ("냉각 촉진",   "+30% 속도",   80, 1.30f),
            ("급속 냉각",   "+65% 속도",  160, 1.65f),
            ("순간 빙결",   "+120% 속도", 320, 2.20f));
        EditorUtility.SetDirty(slow);

        // ── 스나이퍼탑 (Sniper) ─ 고데미지/장거리형 ────────────────────────────
        var sniper = LoadOrCreate<TowerData>($"{TOWER_PATH}/Tower_Sniper.asset");
        SetBase(sniper, "스나이퍼탑", Define.TowerType.Sniper,
            damage: 60f, speed: 0.5f, range: 10f, buildCost: 130);
        sniper.damageUpgrades = Upgrades(
            ("예리한 조준", "+40% 공격력",  120, 1.40f),
            ("급소 타격",   "+90% 공격력",  240, 1.90f),
            ("신의 한 발",  "+200% 공격력", 480, 3.00f));
        sniper.rangeUpgrades = Upgrades(
            ("망원 조준",   "+30% 사거리",   80, 1.30f),
            ("초장거리",    "+65% 사거리",  160, 1.65f),
            ("지평선 너머", "+120% 사거리", 320, 2.20f));
        sniper.speedUpgrades = Upgrades(
            ("연사 훈련",   "+20% 속도",  140, 1.20f),
            ("속사 기술",   "+50% 속도",  280, 1.50f),
            ("자동 장전",   "+90% 속도",  560, 1.90f));
        EditorUtility.SetDirty(sniper);

        // ── 독탑 (Poison) ─ DoT/속도형 ─────────────────────────────────────────
        var poison = LoadOrCreate<PoisonTowerData>($"{TOWER_PATH}/Tower_Poison.asset");
        SetBase(poison, "독탑", Define.TowerType.Poison,
            damage: 8f, speed: 1.2f, range: 5f, buildCost: 110);
        poison.poisonDps      = 0.03f;   // 현재 HP의 3% / 초
        poison.poisonDuration = 4f;
        poison.damageUpgrades = Upgrades(
            ("농축 독",   "+25% 공격력",  60, 1.25f),
            ("맹독",      "+55% 공격력", 120, 1.55f),
            ("치사 독소", "+100% 공격력",240, 2.00f));
        poison.rangeUpgrades = Upgrades(
            ("독 분사",   "+25% 사거리",  70, 1.25f),
            ("광역 오염", "+55% 사거리", 140, 1.55f),
            ("독기 지배", "+100% 사거리",280, 2.00f));
        poison.speedUpgrades = Upgrades(
            ("독 주사기",  "+35% 속도",   80, 1.35f),
            ("연속 감염",  "+70% 속도",  160, 1.70f),
            ("전염 폭풍",  "+130% 속도", 320, 2.30f));
        EditorUtility.SetDirty(poison);

        // ── 번개탑 (Lightning) ─ 광역 번개/데미지형 ─────────────────────────────
        var lightning = LoadOrCreate<LightningTowerData>($"{TOWER_PATH}/Tower_Lightning.asset");
        SetBase(lightning, "번개탑", Define.TowerType.Lightning,
            damage: 25f, speed: 1.0f, range: 5f, buildCost: 140);
        lightning.chainCount        = 0;
        lightning.chainRange        = 0f;
        lightning.chainDamageFalloff= 0f;
        lightning.damageUpgrades = Upgrades(
            ("전하 강화",    "+30% 공격력",  120, 1.30f),
            ("고압 방전",    "+70% 공격력",  240, 1.70f),
            ("천둥신의 분노","+150% 공격력", 480, 2.50f));
        lightning.rangeUpgrades = Upgrades(
            ("확장 방전",  "+25% 사거리",   80, 1.25f),
            ("광역 번개",  "+55% 사거리",  160, 1.55f),
            ("폭풍 영역",  "+100% 사거리", 320, 2.00f));
        lightning.speedUpgrades = Upgrades(
            ("빠른 방전",   "+30% 속도",  100, 1.30f),
            ("연속 번개",   "+65% 속도",  200, 1.65f),
            ("폭풍 연사",   "+120% 속도", 400, 2.20f));
        EditorUtility.SetDirty(lightning);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[TowerDataGenerator] 타워 6종 생성 완료 — Assets/Data/Towers 확인");
        EditorUtility.DisplayDialog(
            "타워 데이터 생성 완료",
            "타워 6종 ScriptableObject가 Assets/Data/Towers 에 생성되었습니다.\n\n" +
            "다음 작업:\n" +
            "각 TowerData의 addressableKey / projectilePrefabKey / iconKey 를 연결해주세요.",
            "확인");
    }

    // ─── 헬퍼 ─────────────────────────────────────────────────────────────────

    private static void SetBase(TowerData d, string towerName, Define.TowerType type,
        float damage, float speed, float range, int buildCost)
    {
        d.towerName       = towerName;
        d.towerType       = type;
        d.baseDamage      = damage;
        d.baseAttackSpeed = speed;
        d.baseRange       = range;
        d.buildCost       = buildCost;
    }

    private static TowerStatUpgrade[] Upgrades(
        (string name, string desc, int cost, float mult) a,
        (string name, string desc, int cost, float mult) b,
        (string name, string desc, int cost, float mult) c)
    {
        return new[]
        {
            new TowerStatUpgrade { upgradeName = a.name, description = a.desc, cost = a.cost, multiplier = a.mult },
            new TowerStatUpgrade { upgradeName = b.name, description = b.desc, cost = b.cost, multiplier = b.mult },
            new TowerStatUpgrade { upgradeName = c.name, description = c.desc, cost = c.cost, multiplier = c.mult },
        };
    }

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
        if (AssetDatabase.IsValidFolder(path)) return;
        int lastSlash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path[..lastSlash], path[(lastSlash + 1)..]);
    }
}
