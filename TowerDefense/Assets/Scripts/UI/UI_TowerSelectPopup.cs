using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타워 배치 팝업.
///
/// Bind 오브젝트 이름 규칙:
///   Buttons : Button_BasicTower ~ Button_LightningTower, Button_Close, Button_Cancle, Button_Construction
///   Texts   : Text_Gold, Text_Construction, Text_TowerName, Text_TowerDescription, Text_CardCategory,
///             Text_Synergy1, Text_Synergy2, Text_Damage, Text_Range, Text_AttackSpeed
///   Images  : Image_BasicTower_Border ~ Image_LightningTower_Border (인덱스 0~5 = 타워 순서),
///             Image_Tower, Image_Category_BG, Image_Synergy1_Border, Image_Synergy2_Border
///   Objects : GameObject_BeforeClick, GameObject_AfterClick
///
/// 각 타워 컨테이너 하위 (Find로 캐싱):
///   PriceObject/Text (TMP)
/// </summary>
public class UI_TowerSelectPopup : UI_Base
{
    // ─── Enum ─────────────────────────────────────────────────────────────────

    enum Buttons
    {
        Button_BasicTower, Button_SlowTower, Button_PoisonTower,
        Button_CannonTower, Button_SniperTower, Button_LightningTower,
        Button_Close, Button_Cancle, Button_Construction,
    }

    enum Texts
    {
        Text_Gold, Text_Construction,
        Text_TowerName, Text_TowerDescription, Text_CardCategory,
        Text_Synergy1, Text_Synergy2,
        Text_Damage, Text_Range, Text_AttackSpeed,
    }

    enum Images
    {
        // 타워 선택 테두리 — 인덱스 0~5, TOWER_COUNT와 반드시 일치
        Image_BasicTower_Border, Image_SlowTower_Border, Image_PoisonTower_Border,
        Image_CannonTower_Border, Image_SniperTower_Border, Image_LightningTower_Border,
        // AfterClick 상세 패널
        Image_Tower, Image_Category_BG, Image_Tower_Border,
        Image_Synergy1_Border, Image_Synergy2_Border,
    }

    enum GameObjects
    {
        GameObject_BeforeClick, GameObject_AfterClick,
    }

    private const int TOWER_COUNT = 6;
    private const int MAX_SYNERGY = 2;

    // ─── TowerType 매핑 (enum 순서: Basic/Slow/Poison/Cannon/Sniper/Lightning) ──

    private static readonly (string label, Color color)[] TOWER_TYPE_INFO =
    {
        ("기본", new Color(0.545f, 0.765f, 0.290f)),  // Basic     #8BC34A
        ("둔화", new Color(0.502f, 0.847f, 1.000f)),  // Slow      #80D8FF
        ("독",   new Color(0.463f, 1.000f, 0.012f)),  // Poison    #76FF03
        ("폭발", new Color(1.000f, 0.427f, 0.000f)),  // Cannon    #FF6D00
        ("저격", new Color(0.216f, 0.278f, 0.310f)),  // Sniper    #37474F
        ("연쇄", new Color(1.000f, 0.839f, 0.000f)),  // Lightning #FFD600
    };

    private static readonly Images[] SYNERGY_IMGS = { Images.Image_Synergy1_Border, Images.Image_Synergy2_Border };
    private static readonly Texts[] SYNERGY_TXTS = { Texts.Text_Synergy1, Texts.Text_Synergy2 };

    // ─── 이벤트 ───────────────────────────────────────────────────────────────

    public event Action<TowerData> OnTowerSelected;

    // ─── Inspector ────────────────────────────────────────────────────────────

    [SerializeField] private RangeIndicator _rangeIndicator;

    // ─── 내부 상태 ────────────────────────────────────────────────────────────

    private TowerData[] _currentData;
    private Vector3 _tileWorldPos;
    private int _selectedIndex = -1;
    private bool _initialized;

    private readonly Color[] _defaultBorderColor = new Color[TOWER_COUNT];
    private readonly TMP_Text[] _priceTxts = new TMP_Text[TOWER_COUNT];

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    void OnEnable() => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindObject(typeof(GameObjects));

        for (int i = 0; i < TOWER_COUNT; i++)
        {
            int idx = i;
            GetButton(typeof(Buttons), i).onClick.AddListener(() => OnTowerButtonClicked(idx));

            _defaultBorderColor[i] = GetImage(typeof(Images), i).color;

            // 가격 텍스트: 이름이 "Text (TMP)"로 고정이라 Find 사용
            var container = GetButton(typeof(Buttons), i).transform.parent.parent;
            _priceTxts[i] = container.Find("PriceObject/Text (TMP)")?.GetComponent<TMP_Text>();
        }

        GetButton(typeof(Buttons), (int)Buttons.Button_Close).onClick.AddListener(OnCloseClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Cancle).onClick.AddListener(Hide);
        GetButton(typeof(Buttons), (int)Buttons.Button_Construction).onClick.AddListener(OnConstructionClicked);

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

    public void Show(Vector3 screenPos, Vector3 tileWorldPos, TowerData[] towerData)
    {
        _currentData = towerData;
        _tileWorldPos = tileWorldPos;
        _selectedIndex = -1;

        if (!_initialized) Init().Forget();

        GetText(typeof(Texts), (int)Texts.Text_Gold).text = $"{Managers.GameM.Gold}";

        for (int i = 0; i < TOWER_COUNT && i < _currentData?.Length; i++)
        {
            if (_priceTxts[i] == null || _currentData[i] == null) continue;
            int cost = Mathf.RoundToInt(_currentData[i].buildCost * Managers.GameM.buildCostMultiplier);
            _priceTxts[i].text = $"{cost}";
        }

        ResetSelection();

        gameObject.SetActive(true);
        transform.localScale = Vector3.one * 0.85f;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void Hide()
    {
        _rangeIndicator?.Hide();
        DOTween.Kill(gameObject);
        gameObject.SetActive(false);
    }

    // ─── 버튼 핸들러 ─────────────────────────────────────────────────────────

    private void OnTowerButtonClicked(int index)
    {
        if (_currentData == null || index >= _currentData.Length) return;

        _selectedIndex = index;
        TowerData data = _currentData[index];

        // 선택 테두리 색 갱신
        for (int i = 0; i < TOWER_COUNT; i++)
            GetImage(typeof(Images), i).color = (i == index) ? data.towerUIColor : _defaultBorderColor[i];

        GetImage(typeof(Images), (int)Images.Image_Tower_Border).color = data.towerUIColor;

        // 패널 전환
        GetObject(typeof(GameObjects), (int)GameObjects.GameObject_BeforeClick).SetActive(false);
        GetObject(typeof(GameObjects), (int)GameObjects.GameObject_AfterClick).SetActive(true);

        // 기본 정보
        GetText(typeof(Texts), (int)Texts.Text_TowerName).text = data.towerName;
        GetText(typeof(Texts), (int)Texts.Text_TowerDescription).text = data.Description;
        GetText(typeof(Texts), (int)Texts.Text_Damage).text = $"{data.baseDamage}";
        GetText(typeof(Texts), (int)Texts.Text_Range).text = $"{data.baseRange}";
        GetText(typeof(Texts), (int)Texts.Text_AttackSpeed).text = $"{data.baseAttackSpeed}/s";

        // 카테고리 레이블 + 배경 색
        var (label, bgColor) = TOWER_TYPE_INFO[(int)data.towerType];
        GetText(typeof(Texts), (int)Texts.Text_CardCategory).text = label;
        GetImage(typeof(Images), (int)Images.Image_Category_BG).color = bgColor;

        // 시너지 태그
        for (int si = 0; si < MAX_SYNERGY; si++)
        {
            bool has = data.synergies != null && si < data.synergies.Length
                       && !string.IsNullOrEmpty(data.synergies[si]);
            GetImage(typeof(Images), (int)SYNERGY_IMGS[si]).gameObject.SetActive(has);
            if (has) GetText(typeof(Texts), (int)SYNERGY_TXTS[si]).text = data.synergies[si];
        }

        // 타워 아이콘
        if (data.iconKey != null)
            GetImage(typeof(Images), (int)Images.Image_Tower).sprite = Managers.ResourceM.GetAtlas(data.iconKey);

        // 설치 버튼 텍스트
        int buildCost = Mathf.RoundToInt(data.buildCost * Managers.GameM.buildCostMultiplier);
        GetText(typeof(Texts), (int)Texts.Text_Construction).text = $"설치  {buildCost}";

        _rangeIndicator?.Show(_tileWorldPos, data.baseRange);
        RefreshConstructionButton();
    }

    private void OnConstructionClicked()
    {
        if (_selectedIndex < 0 || _currentData == null) return;
        OnTowerSelected?.Invoke(_currentData[_selectedIndex]);
    }

    // ─── 갱신 ────────────────────────────────────────────────────────────────

    private void ResetSelection()
    {
        for (int i = 0; i < TOWER_COUNT; i++)
            GetImage(typeof(Images), i).color = _defaultBorderColor[i];

        GetObject(typeof(GameObjects), (int)GameObjects.GameObject_BeforeClick).SetActive(true);
        GetObject(typeof(GameObjects), (int)GameObjects.GameObject_AfterClick).SetActive(false);

        GetButton(typeof(Buttons), (int)Buttons.Button_Construction).interactable = false;
        GetText(typeof(Texts), (int)Texts.Text_Construction).text = "타워를 선택하세요";
    }

    private void RefreshConstructionButton()
    {
        if (_selectedIndex < 0 || _currentData == null || _selectedIndex >= _currentData.Length)
        {
            GetButton(typeof(Buttons), (int)Buttons.Button_Construction).interactable = false;
            return;
        }

        int cost = Mathf.RoundToInt(_currentData[_selectedIndex].buildCost * Managers.GameM.buildCostMultiplier);
        GetButton(typeof(Buttons), (int)Buttons.Button_Construction).interactable = Managers.GameM.Gold >= cost;
    }

    private void OnGoldChanged(int gold)
    {
        GetText(typeof(Texts), (int)Texts.Text_Gold).text = $"{gold}";
        RefreshConstructionButton();
    }

    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }
}
