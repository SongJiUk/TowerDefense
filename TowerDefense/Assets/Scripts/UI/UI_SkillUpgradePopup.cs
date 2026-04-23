using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class UI_SkillUpgradePopup : UI_Base
{
    enum GameObjects { Content_Horizontal, SkillSelectObject }
    enum Buttons { Button_Confirm }
    enum Images { Image_Confirm_Border, Image_Confirm_BG }
    enum Texts { Text_Confirm }

    private static readonly Color COLOR_BTN_DEFAULT    = new Color(0.25f, 0.25f, 0.25f, 1f);
    private static readonly Color COLOR_BTN_ACTIVE     = new Color(0.15f, 0.70f, 0.20f, 1f);
    private static readonly Color COLOR_BORDER_DEFAULT = new Color(0.35f, 0.35f, 0.35f, 1f);
    private static readonly Color COLOR_BORDER_ACTIVE  = new Color(0.20f, 0.90f, 0.28f, 1f);

    private bool _bindDone = false;
    private int _selectedIndex = -1;
    private readonly List<UI_SkillItem> _skillItems = new List<UI_SkillItem>();

    void OnEnable()
    {
        if (!_bindDone) return;
        BuildSkills().Forget();
    }

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindText(typeof(Texts));

        GetButton(typeof(Buttons), (int)Buttons.Button_Confirm).onClick.AddListener(OnConfirmClicked);

        _bindDone = true;
        await BuildSkills();
        return true;
    }

    private async UniTask BuildSkills()
    {
        _selectedIndex = -1;
        _skillItems.Clear();

        Transform parent = GetObject(typeof(GameObjects), (int)GameObjects.Content_Horizontal).transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);

        RefreshConfirmButton();

        bool hasUpgradeable = false;
        for (int i = 0; i < 3; i++)
        {
            SkillData slotSkill = Managers.SkillM.GetSlot(i);
            if (slotSkill == null || !Managers.SkillM.CanUpgrade(i)) continue;

            hasUpgradeable = true;
            int capturedIndex = i;
            UI_SkillItem item = Managers.ObjectM.SpawnUI<UI_SkillItem>("UI_SkillItem", parent);
            await item.Init();
            item.SetInfo(slotSkill, _ => OnSkillClicked(capturedIndex),
                Managers.SkillM.GetSkillLevel(slotSkill), isUpgrade: true);
            _skillItems.Add(item);
        }

        if (!hasUpgradeable)
            Managers.UIM.ClosePopup();
    }

    private void OnSkillClicked(int slotIndex)
    {
        _selectedIndex = slotIndex;

        for (int i = 0; i < _skillItems.Count; i++)
            _skillItems[i].SetSelected(_skillItems[i].SkillData == Managers.SkillM.GetSlot(slotIndex));

        RefreshConfirmButton();
    }

    private void RefreshConfirmButton()
    {
        var btn = GetButton(typeof(Buttons), (int)Buttons.Button_Confirm);
        bool hasSelection = _selectedIndex >= 0;

        btn.interactable = hasSelection;
        GetImage(typeof(Images), (int)Images.Image_Confirm_BG).color     = hasSelection ? COLOR_BTN_ACTIVE     : COLOR_BTN_DEFAULT;
        GetImage(typeof(Images), (int)Images.Image_Confirm_Border).color = hasSelection ? COLOR_BORDER_ACTIVE  : COLOR_BORDER_DEFAULT;
        GetText(typeof(Texts), (int)Texts.Text_Confirm).text = hasSelection ? "선택 완료" : "스킬을 선택하세요";
    }

    private void OnConfirmClicked()
    {
        if (_selectedIndex < 0) return;

        Managers.SkillM.UpgradeSkill(_selectedIndex);
        Managers.SkillM.ConsumeSkillPoint();
        Managers.UIM.ClosePopup();
    }
}
