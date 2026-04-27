using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 시너지 항목 하나. UI_Synergy가 동적으로 생성.
///
/// [필요 계층 구조]
/// UI_SynergyItem
/// └── Image_Synergy_BG
///     ├── Image_Synergy_Border  (LayoutElement: IgnoreLayout)
///     ├── Object_Active         ← 시너지 활성 시 보이는 녹색 배경
///     ├── Top
///     │   ├── Object_Check      ← 활성 체크 아이콘
///     │   ├── Object_Lock       ← 비활성 자물쇠 아이콘
///     │   ├── Text_SynergyName  ← 시너지 이름
///     │   └── Object_SynergyOn  ← "활성" 뱃지
///     ├── Middle
///     │   └── Text_SynergyDes   ← 시너지 설명
///     └── Object_Bottom         ← 비활성 시에만 표시
///         ├── Text_NeedA        ← 필요 타워 A (보유 시 숨김)
///         └── Text_NeedB        ← 필요 타워 B (보유 시 숨김)
/// </summary>
public class UI_SynergyItem : UI_Base
{
    enum Texts
    {
        Text_SynergyName, Text_SynergyDes,
        Text_NeedA, Text_NeedB,
    }
    enum GameObjects
    {
        Object_Active,
        Object_Check,
        Object_Lock,
        Object_SynergyOn,
        Object_Bottom,
    }

    private static readonly Color COLOR_ACTIVE_NAME   = new Color(0.30f, 1.00f, 0.30f);
    private static readonly Color COLOR_INACTIVE_NAME = Color.white;
    private static readonly Color COLOR_ACTIVE_DESC   = Color.white;
    private static readonly Color COLOR_INACTIVE_DESC = new Color(0.60f, 0.60f, 0.60f);

    public bool IsActive => isInit && _defSet
        && Managers.SynergyM.HasTower(_def.reqA)
        && Managers.SynergyM.HasTower(_def.reqB);

    private SynergyData _def;
    private bool _defSet;

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        return true;
    }

    public void SetDef(SynergyData def)
    {
        _def    = def;
        _defSet = true;

        if (!isInit) return;

        GetText(typeof(Texts), (int)Texts.Text_SynergyName).text = def.synergyName;
        GetText(typeof(Texts), (int)Texts.Text_SynergyDes).text  = def.desc;
        GetText(typeof(Texts), (int)Texts.Text_NeedA).text = $"필요 : {TowerName(def.reqA)}";
        GetText(typeof(Texts), (int)Texts.Text_NeedB).text = $"필요 : {TowerName(def.reqB)}";

        Refresh();
    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    public void Refresh()
    {
        if (!isInit || !_defSet) return;

        bool active = Managers.SynergyM.HasTower(_def.reqA) && Managers.SynergyM.HasTower(_def.reqB);

        GetObject(typeof(GameObjects), (int)GameObjects.Object_Active).SetActive(active);
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Check).SetActive(active);
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Lock).SetActive(!active);
        GetObject(typeof(GameObjects), (int)GameObjects.Object_SynergyOn).SetActive(active);
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Bottom).SetActive(!active);

        GetText(typeof(Texts), (int)Texts.Text_SynergyName).color = active ? COLOR_ACTIVE_NAME : COLOR_INACTIVE_NAME;
        GetText(typeof(Texts), (int)Texts.Text_SynergyDes).color  = active ? COLOR_ACTIVE_DESC : COLOR_INACTIVE_DESC;

        if (!active)
        {
            bool hasA = Managers.SynergyM.HasTower(_def.reqA);
            bool hasB = Managers.SynergyM.HasTower(_def.reqB);
            GetText(typeof(Texts), (int)Texts.Text_NeedA).gameObject.SetActive(!hasA);
            GetText(typeof(Texts), (int)Texts.Text_NeedB).gameObject.SetActive(!hasB);
        }
    }

    // ─── 내부 ─────────────────────────────────────────────────────────────────

    private static string TowerName(Define.TowerType type) => type switch
    {
        Define.TowerType.Basic     => "소총탑",
        Define.TowerType.Slow      => "슬로우탑",
        Define.TowerType.Poison    => "독탑",
        Define.TowerType.Cannon    => "캐논탑",
        Define.TowerType.Sniper    => "저격탑",
        Define.TowerType.Lightning => "번개탑",
        _                          => type.ToString(),
    };
}
