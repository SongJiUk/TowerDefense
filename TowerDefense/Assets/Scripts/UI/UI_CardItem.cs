using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 팝업에서 카드 한 장을 표현하는 아이템.
/// SetInfo()로 데이터를 주입하고, 클릭 시 onSelected 콜백을 호출한다.
/// </summary>
public class UI_CardItem : UI_Base
{
    enum Images { Image_Border, Image_BG, 
    Image_Category_Border, Image_Category_BG,
        Image_Icon_Border, Image_Icon_BG, Image_Icon
    }
    enum Texts { Text_CardCategory, Text_CurrentStack, Text_MaxStack,
        Text_CardName, Text_CardType,Text_Description, Text_MaxCountDescription
    }
    enum Buttons { Button_Card }
    enum GameObjects { Object_SelectFX }

    public CardData CardData => _cardData;

    private CardData _cardData;
    private Action<CardData> _onSelected;


   
    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

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
        transform.DOKill();
        transform.localScale = Vector3.one;
        Refresh();
    }

    // ─── 내부 로직 ────────────────────────────────────────────────────────────

    private void Refresh()
    {
        if (!isInit || _cardData == null) return;

        int current = Managers.CardM.GetStackCount(_cardData);

       ;
        GetImage(typeof(Images), (int)Images.Image_Border).color = GetCategoryBorderColor(_cardData.category);

        GetText(typeof(Texts), (int)Texts.Text_CardCategory).text         = GetCategoryLabel(_cardData.category);
        GetText(typeof(Texts), (int)Texts.Text_CardCategory).color = GetCategoryBorderColor(_cardData.category);
        GetImage(typeof(Images), (int)Images.Image_Icon_BG).color             = GetCategoryBGColor(_cardData.category);
        GetImage(typeof(Images), (int)Images.Image_Icon_Border).color         = GetCategoryBorderColor(_cardData.category);
        GetImage(typeof(Images), (int)Images.Image_Category_BG).color = GetCategoryBGColor(_cardData.category);
        GetImage(typeof(Images), (int)Images.Image_Category_Border).color         = GetCategoryBorderColor(_cardData.category);
        

        GetText(typeof(Texts), (int)Texts.Text_CardName).text            = _cardData.cardName;
        GetText(typeof(Texts), (int)Texts.Text_CardType).text            = _cardData.effectType.ToString();
        GetText(typeof(Texts), (int)Texts.Text_Description).text         = _cardData.Description;
        GetText(typeof(Texts), (int)Texts.Text_CurrentStack).text        = $"{current}";
        GetText(typeof(Texts), (int)Texts.Text_MaxStack).text            = $"/ {_cardData.maxStack}";
        var maxDesc = GetText(typeof(Texts), (int)Texts.Text_MaxCountDescription);
        maxDesc.gameObject.SetActive(_cardData.maxStack < 99);
        maxDesc.text = $"최대 {_cardData.maxStack}회 중첩";

        var sprite = Managers.ResourceM.GetAtlas(_cardData.iconKey);
        if (sprite != null)
            GetImage(typeof(Images), (int)Images.Image_Icon).sprite = sprite;
    }

    private static string GetCategoryLabel(Define.CardCategory category) => category switch
    {
        Define.CardCategory.A => "패시브",
        Define.CardCategory.B => "경제",
        Define.CardCategory.C => "특수",
        Define.CardCategory.D => "스킬",
        _ => ""
    };

    private static Color GetCategoryBGColor(Define.CardCategory category) => category switch
    {
        Define.CardCategory.A => new Color(0.18f, 0.10f, 0.03f, 1f), // 갈색
        Define.CardCategory.B => new Color(0.05f, 0.15f, 0.04f, 1f), // 초록
        Define.CardCategory.C => new Color(0.10f, 0.04f, 0.18f, 1f), // 보라
        Define.CardCategory.D => new Color(0.04f, 0.10f, 0.18f, 1f), // 파랑
        _ => Color.black
    };

    private static Color GetCategoryBorderColor(Define.CardCategory category) => category switch
    {
        Define.CardCategory.A => new Color(0.78f, 0.53f, 0.04f, 1f), // 금색
        Define.CardCategory.B => new Color(0.35f, 0.54f, 0.10f, 1f), // 초록
        Define.CardCategory.C => new Color(0.55f, 0.20f, 0.80f, 1f), // 보라
        Define.CardCategory.D => new Color(0.18f, 0.53f, 0.78f, 1f), // 파랑
        _ => Color.white
    };

    public void SetSelected(bool selected)
    {
        transform.DOKill();
        var border = GetImage(typeof(Images), (int)Images.Image_Border);
        var fx     = GetObject(typeof(GameObjects), (int)GameObjects.Object_SelectFX);

        if (selected)
        {
            border.DOColor(Color.Lerp(GetCategoryBorderColor(_cardData.category), Color.white, 0.5f), 0.15f).SetUpdate(true);
            transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            fx?.SetActive(true);
        }
        else
        {
            border.DOColor(GetCategoryBorderColor(_cardData.category), 0.15f).SetUpdate(true);
            transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad).SetUpdate(true);
            fx?.SetActive(false);
        }
    }

    private void OnCardClicked()
    {
        _onSelected?.Invoke(_cardData);
    }
}
