using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 시 표시되는 카드 선택 팝업.
/// DrawCard()로 카테고리별 카드를 최대 4장 뽑아 Horizontal 레이아웃에 UI_CardItem으로 표시한다.
/// 카드 선택 시 ApplyCard() 호출 후 팝업이 닫힌다.
/// </summary>
public class UI_LevelUpPopup : UI_Base
{
    enum Objects { Content_Horizontal }
    enum Images  { Image_Background }  // 팝업 배경 이미지 이름에 맞게 조정

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    async void Start()
    {
        await Init();
    }

    void OnEnable()  => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindObject(typeof(Objects));
        BindImage(typeof(Images));

        Transform parent = GetObject(typeof(Objects), (int)Objects.Content_Horizontal).transform;

        List<CardData> cards = Managers.CardM.DrawCard();
        foreach (CardData cardData in cards)
        {
            UI_CardItem item = Managers.ObjectM.SpawnUI<UI_CardItem>("UI_CardItem", parent);
            await item.Init();
            item.SetInfo(cardData, OnCardSelected);
        }

        ApplyTheme(Managers.WaveM.CurrentStage);

        return true;
    }

    public override void ApplyTheme(StageData stage)
    {
        if (stage == null) return;
        GetImage(typeof(Images), (int)Images.Image_Background).color = stage.uiBarBG;
        // 필요한 오브젝트 추가
    }

    // ─── 카드 선택 ────────────────────────────────────────────────────────────

    private void OnCardSelected(CardData cardData)
    {
        Managers.CardM.ApplyCard(cardData);
        Managers.ObjectM.DespawnUI(gameObject);
    }
}
