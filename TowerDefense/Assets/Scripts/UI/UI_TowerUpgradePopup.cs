using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 배치된 타워 클릭 시 표시되는 업그레이드/판매 팝업.
/// 공격력 / 사거리 / 공격속도 3열 독립 업그레이드.
///
/// Unity 오브젝트 이름 규칙 (Bind 패턴):
///   텍스트  : Txt_TowerName, Txt_Gold, Txt_SellPrice
///             Txt_D0_Name, Txt_D0_Desc, Txt_D0_Cost  (D=Damage, R=Range, S=Speed / 0~2)
///   버튼    : Btn_Close, Btn_Sell, Btn_D0~D2, Btn_R0~R2, Btn_S0~S2
///   오브젝트: Card_D0~D2, Card_R0~R2, Card_S0~S2  (카드 루트, SetActive 제어)
///             Done_D0~D2, Done_R0~R2, Done_S0~S2  (완료 상태 초록 오버레이)
/// </summary>
public class UI_TowerUpgradePopup : UI_Base
{
    // ─── Enum ─────────────────────────────────────────────────────────────────

    enum Txts
    {
        Txt_TowerName, Txt_Gold, Txt_SellPrice,
        Txt_D0_Name, Txt_D0_Desc, Txt_D0_Cost,
        Txt_D1_Name, Txt_D1_Desc, Txt_D1_Cost,
        Txt_D2_Name, Txt_D2_Desc, Txt_D2_Cost,
        Txt_R0_Name, Txt_R0_Desc, Txt_R0_Cost,
        Txt_R1_Name, Txt_R1_Desc, Txt_R1_Cost,
        Txt_R2_Name, Txt_R2_Desc, Txt_R2_Cost,
        Txt_S0_Name, Txt_S0_Desc, Txt_S0_Cost,
        Txt_S1_Name, Txt_S1_Desc, Txt_S1_Cost,
        Txt_S2_Name, Txt_S2_Desc, Txt_S2_Cost,
    }

    enum Btns
    {
        Btn_Close, Btn_Sell,
        Btn_D0, Btn_D1, Btn_D2,
        Btn_R0, Btn_R1, Btn_R2,
        Btn_S0, Btn_S1, Btn_S2,
    }

    enum GO
    {
        Card_D0, Card_D1, Card_D2,
        Card_R0, Card_R1, Card_R2,
        Card_S0, Card_S1, Card_S2,
        Done_D0, Done_D1, Done_D2,
        Done_R0, Done_R1, Done_R2,
        Done_S0, Done_S1, Done_S2,
    }

    // ─── 컬럼별 인덱스 테이블 [col 0=D 1=R 2=S][card 0~2] ─────────────────────

    private static readonly int[,] _cardGO = {
        { (int)GO.Card_D0, (int)GO.Card_D1, (int)GO.Card_D2 },
        { (int)GO.Card_R0, (int)GO.Card_R1, (int)GO.Card_R2 },
        { (int)GO.Card_S0, (int)GO.Card_S1, (int)GO.Card_S2 },
    };
    private static readonly int[,] _doneGO = {
        { (int)GO.Done_D0, (int)GO.Done_D1, (int)GO.Done_D2 },
        { (int)GO.Done_R0, (int)GO.Done_R1, (int)GO.Done_R2 },
        { (int)GO.Done_S0, (int)GO.Done_S1, (int)GO.Done_S2 },
    };
    private static readonly int[,] _btnIdx = {
        { (int)Btns.Btn_D0, (int)Btns.Btn_D1, (int)Btns.Btn_D2 },
        { (int)Btns.Btn_R0, (int)Btns.Btn_R1, (int)Btns.Btn_R2 },
        { (int)Btns.Btn_S0, (int)Btns.Btn_S1, (int)Btns.Btn_S2 },
    };
    private static readonly int[,] _nameIdx = {
        { (int)Txts.Txt_D0_Name, (int)Txts.Txt_D1_Name, (int)Txts.Txt_D2_Name },
        { (int)Txts.Txt_R0_Name, (int)Txts.Txt_R1_Name, (int)Txts.Txt_R2_Name },
        { (int)Txts.Txt_S0_Name, (int)Txts.Txt_S1_Name, (int)Txts.Txt_S2_Name },
    };
    private static readonly int[,] _descIdx = {
        { (int)Txts.Txt_D0_Desc, (int)Txts.Txt_D1_Desc, (int)Txts.Txt_D2_Desc },
        { (int)Txts.Txt_R0_Desc, (int)Txts.Txt_R1_Desc, (int)Txts.Txt_R2_Desc },
        { (int)Txts.Txt_S0_Desc, (int)Txts.Txt_S1_Desc, (int)Txts.Txt_S2_Desc },
    };
    private static readonly int[,] _costIdx = {
        { (int)Txts.Txt_D0_Cost, (int)Txts.Txt_D1_Cost, (int)Txts.Txt_D2_Cost },
        { (int)Txts.Txt_R0_Cost, (int)Txts.Txt_R1_Cost, (int)Txts.Txt_R2_Cost },
        { (int)Txts.Txt_S0_Cost, (int)Txts.Txt_S1_Cost, (int)Txts.Txt_S2_Cost },
    };

    // ─── 내부 상태 ────────────────────────────────────────────────────────────

    private TowerController _tower;
    private bool _initialized;

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    async void Start() => await Init();

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;

        BindText(typeof(Txts));
        BindButton(typeof(Btns));
        BindObject(typeof(GO));

        GetButton(typeof(Btns), (int)Btns.Btn_Close).onClick.AddListener(OnCloseClicked);
        GetButton(typeof(Btns), (int)Btns.Btn_Sell).onClick.AddListener(OnSellClicked);

        Managers.GameM.OnGoldChanged += OnGoldChanged;
        _initialized = true;
        return true;
    }

    void OnDestroy()
    {
        if (Managers.GameM != null)
            Managers.GameM.OnGoldChanged -= OnGoldChanged;
    }

    // ─── 공개 API ─────────────────────────────────────────────────────────────

    public void Show(TowerController tower)
    {
        _tower = tower;
        if (!_initialized) Init().Forget();
        RefreshAll();

        transform.localScale = Vector3.one * 0.85f;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (_tower == null || _tower.Data == null) return;

        TowerData data = _tower.Data;
        GetText(typeof(Txts), (int)Txts.Txt_TowerName).text = data.towerName;
        GetText(typeof(Txts), (int)Txts.Txt_Gold).text = $"{Managers.GameM.Gold}";
        GetText(typeof(Txts), (int)Txts.Txt_SellPrice).text = $"{_tower.GetSellPrice()}";

        RefreshColumn(0, data.damageUpgrades, _tower.DamageLevel, Define.UpgradeType.Damage);
        RefreshColumn(1, data.rangeUpgrades, _tower.RangeLevel, Define.UpgradeType.Range);
        RefreshColumn(2, data.speedUpgrades, _tower.SpeedLevel, Define.UpgradeType.Speed);
    }

    private void RefreshColumn(int col, TowerStatUpgrade[] upgrades,
                                int currentLevel, Define.UpgradeType type)
    {
        for (int i = 0; i < 3; i++)
        {
            bool hasData = upgrades != null && i < upgrades.Length;
            GetObject(typeof(GO), _cardGO[col, i]).SetActive(hasData);
            if (!hasData) continue;

            GetText(typeof(Txts), _nameIdx[col, i]).text = upgrades[i].upgradeName;
            GetText(typeof(Txts), _descIdx[col, i]).text = upgrades[i].description;

            bool isDone = i < currentLevel;
            bool isNext = i == currentLevel;

            GetObject(typeof(GO), _doneGO[col, i]).SetActive(isDone);

            var btn = GetButton(typeof(Btns), _btnIdx[col, i]);
            btn.gameObject.SetActive(!isDone);

            if (isDone) continue;

            GetText(typeof(Txts), _costIdx[col, i]).text = $"{upgrades[i].cost}";
            btn.interactable = isNext && Managers.GameM.Gold >= upgrades[i].cost;

            Define.UpgradeType capturedType = type;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnUpgradeClicked(capturedType));
        }
    }

    private void OnGoldChanged(int gold)
    {
        GetText(typeof(Txts), (int)Txts.Txt_Gold).text = $"{gold}";
        RefreshAll();
    }

    // ─── 버튼 핸들러 ─────────────────────────────────────────────────────────

    private void OnUpgradeClicked(Define.UpgradeType type)
    {
        if (_tower == null) return;
        if (_tower.TryUpgrade(type))
            RefreshAll();
    }

    private void OnSellClicked() => _tower?.Sell();
    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
        TowerController.HideSelectedRange();
    }
}
