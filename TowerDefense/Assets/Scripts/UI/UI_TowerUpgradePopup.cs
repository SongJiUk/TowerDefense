using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 배치된 타워 클릭 시 표시되는 업그레이드/판매 팝업.
/// 공격력(D) / 사거리(R) / 공격속도(S) 3열 독립 업그레이드.
///
/// Unity 오브젝트 이름 규칙 (Bind 패턴):
///   텍스트  : Text_TowerName, Text_Description, Text_SellPrice
///             Text_D1_Name, Text_D1_Description, Text_D1_Price  (D=Damage, R=Range, S=Speed / 1~3)
///   버튼    : Button_Close
///             Button_D1~D3, Button_R1~R3, Button_S1~S3
///   이미지  : Image_Main_Border, Image_Main_BG
///             Image_Tower_Border, Image_Tower_BG, Image_Tower
///             Image_D1_Border, Image_D1_BG  (D=Damage, R=Range, S=Speed / 1~3)
///   오브젝트: BG  (클릭 차단 반투명 배경)
/// </summary>
public class UI_TowerUpgradePopup : UI_Base
{
    // ─── Enum ─────────────────────────────────────────────────────────────────

    enum GameObjects {BG, Content_Upgrade, Content_Sell }
    enum Texts
    {
        Text_TowerName, Text_Description, Text_SellPrice, Text_SellPriceWord,
        Text_CurrentDamage, Text_CurrentRange, Text_CurrentAttackSpeed,
        Text_D1_Name, Text_D1_Description, Text_D1_Price,
        Text_D2_Name, Text_D2_Description, Text_D2_Price,
        Text_D3_Name, Text_D3_Description, Text_D3_Price,
        Text_R1_Name, Text_R1_Description, Text_R1_Price,
        Text_R2_Name, Text_R2_Description, Text_R2_Price,
        Text_R3_Name, Text_R3_Description, Text_R3_Price,
        Text_S1_Name, Text_S1_Description, Text_S1_Price,
        Text_S2_Name, Text_S2_Description, Text_S2_Price,
        Text_S3_Name, Text_S3_Description, Text_S3_Price,
    }

    enum Buttons    
    {
        Button_Close, Button_Upgrade, Button_Sell, Button_Cancel, Button_RealSell,
        Button_D1, Button_D2, Button_D3,
        Button_R1, Button_R2, Button_R3,
        Button_S1, Button_S2, Button_S3,
    }

    enum Images
    {
        Image_Main_Border, Image_Main_BG,
        Image_Tower_Border, Image_Tower_BG, Image_Tower, Image_Sell_Border, Image_Sell_BG,
        Image_D1_Border, Image_D1_BG,
        Image_D2_Border, Image_D2_BG,
        Image_D3_Border, Image_D3_BG,
        Image_R1_Border, Image_R1_BG,
        Image_R2_Border, Image_R2_BG,
        Image_R3_Border, Image_R3_BG,
        Image_S1_Border, Image_S1_BG,
        Image_S2_Border, Image_S2_BG,
        Image_S3_Border, Image_S3_BG,
    }

    // ─── 컬럼별 인덱스 테이블 [col 0=D 1=R 2=S][card 0~2] ─────────────────────

    private static readonly int[,] _btnIdx = {
        { (int)Buttons.Button_D1, (int)Buttons.Button_D2, (int)Buttons.Button_D3 },
        { (int)Buttons.Button_R1, (int)Buttons.Button_R2, (int)Buttons.Button_R3 },
        { (int)Buttons.Button_S1, (int)Buttons.Button_S2, (int)Buttons.Button_S3 },
    };
    private static readonly int[,] _nameIdx = {
        { (int)Texts.Text_D1_Name, (int)Texts.Text_D2_Name, (int)Texts.Text_D3_Name },
        { (int)Texts.Text_R1_Name, (int)Texts.Text_R2_Name, (int)Texts.Text_R3_Name },
        { (int)Texts.Text_S1_Name, (int)Texts.Text_S2_Name, (int)Texts.Text_S3_Name },
    };
    private static readonly int[,] _descIdx = {
        { (int)Texts.Text_D1_Description, (int)Texts.Text_D2_Description, (int)Texts.Text_D3_Description },
        { (int)Texts.Text_R1_Description, (int)Texts.Text_R2_Description, (int)Texts.Text_R3_Description },
        { (int)Texts.Text_S1_Description, (int)Texts.Text_S2_Description, (int)Texts.Text_S3_Description },
    };
    private static readonly int[,] _priceIdx = {
        { (int)Texts.Text_D1_Price, (int)Texts.Text_D2_Price, (int)Texts.Text_D3_Price },
        { (int)Texts.Text_R1_Price, (int)Texts.Text_R2_Price, (int)Texts.Text_R3_Price },
        { (int)Texts.Text_S1_Price, (int)Texts.Text_S2_Price, (int)Texts.Text_S3_Price },
    };
    private static readonly int[,] _borderImgIdx = {
        { (int)Images.Image_D1_Border, (int)Images.Image_D2_Border, (int)Images.Image_D3_Border },
        { (int)Images.Image_R1_Border, (int)Images.Image_R2_Border, (int)Images.Image_R3_Border },
        { (int)Images.Image_S1_Border, (int)Images.Image_S2_Border, (int)Images.Image_S3_Border },
    };
    private static readonly int[,] _bgImgIdx = {
        { (int)Images.Image_D1_BG, (int)Images.Image_D2_BG, (int)Images.Image_D3_BG },
        { (int)Images.Image_R1_BG, (int)Images.Image_R2_BG, (int)Images.Image_R3_BG },
        { (int)Images.Image_S1_BG, (int)Images.Image_S2_BG, (int)Images.Image_S3_BG },
    };

    // ─── 색상 상수 ────────────────────────────────────────────────────────────

    private static readonly Color COLOR_LOCKED_BG   = new Color(0.25f, 0.25f, 0.25f, 0.80f);
    private static readonly Color COLOR_DONE_BORDER = new Color(0.20f, 0.85f, 0.20f, 1.00f);

    // ─── 내부 상태 ────────────────────────────────────────────────────────────

    private TowerController _tower;
    private bool _initialized;
    private RectTransform _rect;
    private Vector2 _originPos;
    private Color _themeAccent = Color.white;

    private Button[,]   _cachedBtns;
    private Image[,]    _cachedBgImgs;
    private Image[,]    _cachedBorderImgs;
    private TMP_Text[,] _cachedNameTexts;
    private TMP_Text[,] _cachedDescTexts;
    private TMP_Text[,] _cachedPriceTexts;

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    void OnEnable() => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();
    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        
        GetButton(typeof(Buttons), (int)Buttons.Button_Close).onClick.AddListener(OnCloseClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Upgrade).onClick.AddListener(() => SwitchTab(true));
        GetButton(typeof(Buttons), (int)Buttons.Button_Sell).onClick.AddListener(() => SwitchTab(false));
        GetButton(typeof(Buttons), (int)Buttons.Button_Cancel).onClick.AddListener(() => SwitchTab(true));
        GetButton(typeof(Buttons), (int)Buttons.Button_RealSell).onClick.AddListener(OnRealSellClicked);

        AddHoverAnimation(GetButton(typeof(Buttons), (int)Buttons.Button_Upgrade));
        AddHoverAnimation(GetButton(typeof(Buttons), (int)Buttons.Button_Sell));
        AddHoverAnimation(GetButton(typeof(Buttons), (int)Buttons.Button_Cancel));
        AddHoverAnimation(GetButton(typeof(Buttons), (int)Buttons.Button_RealSell));

        _cachedBtns        = new Button[3, 3];
        _cachedBgImgs      = new Image[3, 3];
        _cachedBorderImgs  = new Image[3, 3];
        _cachedNameTexts   = new TMP_Text[3, 3];
        _cachedDescTexts   = new TMP_Text[3, 3];
        _cachedPriceTexts  = new TMP_Text[3, 3];

        for (int col = 0; col < 3; col++)
        {
            for (int i = 0; i < 3; i++)
            {
                _cachedBtns[col, i]       = GetButton(typeof(Buttons), _btnIdx[col, i]);
                _cachedBgImgs[col, i]     = GetImage(typeof(Images), _bgImgIdx[col, i]);
                _cachedBorderImgs[col, i] = GetImage(typeof(Images), _borderImgIdx[col, i]);
                _cachedNameTexts[col, i]  = GetText(typeof(Texts), _nameIdx[col, i]);
                _cachedDescTexts[col, i]  = GetText(typeof(Texts), _descIdx[col, i]);
                _cachedPriceTexts[col, i] = GetText(typeof(Texts), _priceIdx[col, i]);

                AddHoverAnimation(_cachedBtns[col, i]);
            }
        }

        Managers.GameM.OnGoldChanged += OnGoldChanged;

        _rect = GetComponent<RectTransform>();
        _originPos = _rect.anchoredPosition;

        _initialized = true;
        return true;
    }

    void OnDestroy()
    {
        if (Managers.GameM != null)
            Managers.GameM.OnGoldChanged -= OnGoldChanged;
    }

    // ─── 공개 API ─────────────────────────────────────────────────────────────

    public override void ApplyTheme(StageData stage)
    {
        if (stage == null) return;
        _themeAccent = stage.uiAccentColor;
        GetImage(typeof(Images), (int)Images.Image_Main_BG).color     = stage.uiBarBG;
        GetImage(typeof(Images), (int)Images.Image_Main_Border).color = stage.uiLineColor;

        GetImage(typeof(Images), (int)Images.Image_Sell_BG).color = stage.uiBarBG;
        GetImage(typeof(Images), (int)Images.Image_Sell_Border).color = stage.uiLineColor;

    }

    public void Show(TowerController tower)
    {
        _tower = tower;
        if (!_initialized) Init().Forget();
        ApplyTheme(Managers.WaveM.CurrentStage);
        RefreshAll();

        var bg = GetObject(typeof(GameObjects), (int)GameObjects.BG);
        bg.SetActive(false);

        // 업그레이드 탭 기본 표시
        var upgradeContent = GetObject(typeof(GameObjects), (int)GameObjects.Content_Upgrade);
        var sellContent    = GetObject(typeof(GameObjects), (int)GameObjects.Content_Sell);
        upgradeContent.SetActive(true);
        upgradeContent.GetComponent<RectTransform>().localScale = Vector3.one;
        sellContent.SetActive(false);
        sellContent.GetComponent<RectTransform>().localScale = Vector3.one;

        _rect.DOKill();
        _rect.anchoredPosition = _originPos + Vector2.down * 800f;
        _rect.DOAnchorPos(_originPos, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true)
             .OnComplete(() => bg.SetActive(true));
    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (_tower == null || _tower.Data == null) return;

        TowerData data = _tower.Data;
        GetImage(typeof(Images), (int)Images.Image_Tower).sprite = Managers.ResourceM.GetAtlas(data.iconKey);
        GetText(typeof(Texts), (int)Texts.Text_TowerName).text = $"{data.towerName} 업그레이드";
        int sellPrice = _tower.GetSellPrice();
        GetText(typeof(Texts), (int)Texts.Text_SellPrice).text          = $"{sellPrice}";
        GetText(typeof(Texts), (int)Texts.Text_SellPriceWord).text      = $"판매 시 {sellPrice} 골드 환급";
        GetText(typeof(Texts), (int)Texts.Text_CurrentDamage).text      = $"{_tower.CurrentDamage:F1}";
        GetText(typeof(Texts), (int)Texts.Text_CurrentRange).text       = $"{_tower.CurrentRange:F1}";
        GetText(typeof(Texts), (int)Texts.Text_CurrentAttackSpeed).text = $"{_tower.CurrentSpeed:F2}/s";

        RefreshColumn(0, data.damageUpgrades, _tower.DamageLevel, Define.UpgradeType.Damage);
        RefreshColumn(1, data.rangeUpgrades, _tower.RangeLevel, Define.UpgradeType.Range);
        RefreshColumn(2, data.speedUpgrades, _tower.SpeedLevel, Define.UpgradeType.Speed);

        var contentRect = GetObject(typeof(GameObjects), (int)GameObjects.Content_Upgrade).GetComponent<RectTransform>();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    private void RefreshColumn(int col, TowerStatUpgrade[] upgrades,
                                int currentLevel, Define.UpgradeType type)
    {
        for (int i = 0; i < 3; i++)
        {
            bool hasData  = upgrades != null && i < upgrades.Length;
            bool isDone   = i < currentLevel;
            bool isNext   = i == currentLevel && hasData;
            bool isLocked = !isDone && !isNext;

            var btn       = _cachedBtns[col, i];
            var bgImg     = _cachedBgImgs[col, i];
            var borderImg = _cachedBorderImgs[col, i];
            borderImg.color                    = isDone ? COLOR_DONE_BORDER : Color.white;
            _cachedNameTexts[col, i].color     = isDone ? COLOR_DONE_BORDER : Color.white;

            if (isDone || !hasData)
            {
                btn.gameObject.SetActive(false);
                continue;
            }

            btn.gameObject.SetActive(true);
            _cachedNameTexts[col, i].text  = upgrades[i].upgradeName;
            _cachedDescTexts[col, i].text  = upgrades[i].description;
            _cachedPriceTexts[col, i].text = $"{upgrades[i].cost}";

            if (isLocked)
            {
                bgImg.color      = COLOR_LOCKED_BG;
                btn.interactable = false;
            }
            else
            {
                bgImg.color      = _themeAccent;
                btn.interactable = Managers.GameM.Gold >= upgrades[i].cost;

                Define.UpgradeType capturedType = type;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnUpgradeClicked(capturedType));
            }
        }
    }

    private void OnGoldChanged(int gold)
    {
        RefreshAll();
    }

    // ─── 버튼 핸들러 ─────────────────────────────────────────────────────────

    private void OnUpgradeClicked(Define.UpgradeType type)
    {
        if (_tower == null) return;
        if (_tower.TryUpgrade(type))
            RefreshAll();
    }

    private void AddHoverAnimation(Button btn)
    {
        // highlightedColor를 normalColor와 같게 해서 Button 기본 색 전환 차단
        var colors = btn.colors;
        colors.highlightedColor = colors.normalColor;
        btn.colors = colors;

        var trigger = btn.gameObject.AddComponent<EventTrigger>();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => { if (btn.interactable) btn.transform.DOScale(1.08f, 0.12f).SetUpdate(true); });
        trigger.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => btn.transform.DOScale(1f, 0.12f).SetUpdate(true));
        trigger.triggers.Add(exit);
    }

    private void SwitchTab(bool toUpgrade)
    {
        var upgradeContent = GetObject(typeof(GameObjects), (int)GameObjects.Content_Upgrade);
        var sellContent    = GetObject(typeof(GameObjects), (int)GameObjects.Content_Sell);

        var showObj = toUpgrade ? upgradeContent : sellContent;
        var hideObj = toUpgrade ? sellContent    : upgradeContent;

        if (!hideObj.activeSelf && showObj.activeSelf) return;

        var hideRect = hideObj.GetComponent<RectTransform>();
        hideRect.DOKill();
        hideRect.DOScale(0.85f, 0.12f).SetEase(Ease.InBack).SetUpdate(true)
            .OnComplete(() => hideObj.SetActive(false));

        showObj.SetActive(true);
        var showRect = showObj.GetComponent<RectTransform>();
        showRect.DOKill();
        showRect.localScale = Vector3.one * 0.85f;
        showRect.DOScale(1f, 0.22f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void OnRealSellClicked()
    {
        _tower?.Sell();
        OnCloseClicked();
    }

    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
        TowerController.HideSelectedRange();
    }
}
