using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 팝업에서 카드 한 장을 표현하는 아이템.
/// SetInfo()로 데이터를 주입하고, 클릭 시 onSelected 콜백을 호출한다.
/// </summary>
public class UI_CardItem : UI_Base
{
    enum Images { Image_Icon }
    enum Texts { Text_CardName, Text_Description, Text_Stack }
    enum Buttons { Button_Card }

    private CardData _cardData;
    private Action<CardData> _onSelected;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    async void Start()
    {
        await Init();
        Refresh();
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        BindEvent(
            GetButton(typeof(Buttons), (int)Buttons.Button_Card).gameObject,
            OnCardClicked
        );

        return true;
    }

    /// <summary>카드 데이터를 주입한다. Init 완료 전이면 Start()에서 Refresh()로 후처리된다.</summary>
    public void SetInfo(CardData cardData, Action<CardData> onSelected)
    {
        _cardData = cardData;
        _onSelected = onSelected;
        Refresh();
    }

    // ─── 내부 로직 ────────────────────────────────────────────────────────────

    /// <summary>현재 데이터로 UI를 갱신. Init 미완료면 무시 (Start에서 재호출).</summary>
    private void Refresh()
    {
        if (!isInit || _cardData == null) return;

        GetText(typeof(Texts), (int)Texts.Text_CardName).text = _cardData.cardName;
        GetText(typeof(Texts), (int)Texts.Text_Description).text = _cardData.Description;

        int current = Managers.CardM.GetStackCount(_cardData);
        GetText(typeof(Texts), (int)Texts.Text_Stack).text = $"{current} / {_cardData.maxStack}";

        // 아이콘: Addressable 아틀라스에서 iconKey로 Sprite 로드
        var sprite = Managers.ResourceM.Load<Sprite>(_cardData.iconKey);
        if (sprite != null)
            GetImage(typeof(Images), (int)Images.Image_Icon).sprite = sprite;
    }

    private void OnCardClicked()
    {
        _onSelected?.Invoke(_cardData);
    }
}
