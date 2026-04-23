using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 시 표시되는 카드 선택 팝업.
/// 카드를 클릭해 임시 선택 → 확인 버튼으로 확정한다.
/// </summary>
public class UI_LevelUpPopup : UI_Base
{
    enum GameObjects { Content_Horizontal }
    enum Buttons { Button_Confirm }
    enum Images { Image_Confirm_BG }
    enum Texts { Text_Confirm }

    private static readonly Color COLOR_BTN_DEFAULT = new Color(0.25f, 0.25f, 0.25f, 1f);
    private static readonly Color COLOR_BTN_ACTIVE  = new Color(0.15f, 0.70f, 0.20f, 1f);

    private bool _initialized;
    private CardData _selectedCard;
    private readonly List<UI_CardItem> _cardItems = new List<UI_CardItem>();

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void OnEnable()  => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;
        _initialized = true;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));

        GetButton(typeof(Buttons), (int)Buttons.Button_Confirm).onClick.AddListener(OnConfirmClicked);

        return true;
    }

    public async UniTaskVoid SetInfo()
    {
        _selectedCard = null;
        _cardItems.Clear();

        Transform parent = GetObject(typeof(GameObjects), (int)GameObjects.Content_Horizontal).transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Managers.ObjectM.DespawnUI(parent.GetChild(i).gameObject);

        RefreshConfirmButton();

        List<CardData> cards = Managers.CardM.DrawCard();
        foreach (CardData cardData in cards)
        {
            UI_CardItem item = Managers.ObjectM.SpawnUI<UI_CardItem>("UI_CardItem", parent);
            await item.Init();
            item.SetInfo(cardData, OnCardClicked);
            _cardItems.Add(item);
        }

        ApplyTheme(Managers.WaveM.CurrentStage);
    }

    public override void ApplyTheme(StageData stage)
    {
        if (stage == null) return;
    }

    // ─── 카드 선택 ────────────────────────────────────────────────────────────

    private void OnCardClicked(CardData cardData)
    {
        _selectedCard = cardData;

        foreach (UI_CardItem item in _cardItems)
            item.SetSelected(item.CardData == cardData);

        RefreshConfirmButton();
    }

    private void RefreshConfirmButton()
    {
        var btn   = GetButton(typeof(Buttons), (int)Buttons.Button_Confirm);
        bool hasSelection = _selectedCard != null;

        btn.interactable = hasSelection;
        GetImage(typeof(Images), (int)Images.Image_Confirm_BG).color = hasSelection ? COLOR_BTN_ACTIVE : COLOR_BTN_DEFAULT;
        GetText(typeof(Texts), (int)Texts.Text_Confirm).text = hasSelection ? "선택 완료" : "카드를 선택하세요";
    }

    // ─── 버튼 핸들러 ─────────────────────────────────────────────────────────

    private void OnConfirmClicked()
    {
        if (_selectedCard == null) return;
        Managers.CardM.ApplyCard(_selectedCard);
        Managers.ObjectM.DespawnUI(gameObject);
    }

    private void OnSkipClicked()
    {
        Managers.ObjectM.DespawnUI(gameObject);
    }
}
