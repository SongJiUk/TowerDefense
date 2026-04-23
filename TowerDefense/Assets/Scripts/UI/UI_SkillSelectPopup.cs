using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 선택 팝업.
/// 카드를 클릭해 임시 선택 → 확인 버튼으로 확정한다.
/// </summary>
public class UI_SkillSelectPopup : UI_Base
{
    enum GameObjects { Content_Horizontal, SkillSelectObject }
    enum Buttons { Button_Confirm }
    enum Images { Image_Confirm_Border, Image_Confirm_BG }
    enum Texts { Text_Confirm }

    private static readonly Color COLOR_BTN_DEFAULT        = new Color(0.25f, 0.25f, 0.25f, 1f);
    private static readonly Color COLOR_BTN_ACTIVE         = new Color(0.15f, 0.70f, 0.20f, 1f);
    private static readonly Color COLOR_BORDER_DEFAULT     = new Color(0.35f, 0.35f, 0.35f, 1f);
    private static readonly Color COLOR_BORDER_ACTIVE      = new Color(0.20f, 0.90f, 0.28f, 1f);

    private bool _bindDone = false;
    private SkillData _selectedSkill;
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
        _selectedSkill = null;
        _skillItems.Clear();

        Transform parent = GetObject(typeof(GameObjects), (int)GameObjects.Content_Horizontal).transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);

        RefreshConfirmButton();

        List<SkillData> skills = GetAvailableSkills();
        foreach (SkillData skill in skills)
        {
            UI_SkillItem item = Managers.ObjectM.SpawnUI<UI_SkillItem>("UI_SkillItem", parent);
            await item.Init();
            item.SetInfo(skill, OnSkillClicked);
            _skillItems.Add(item);
        }
    }

    private List<SkillData> GetAvailableSkills()
    {
        var pool = new List<SkillData>();
        foreach (Define.SkillType skillType in System.Enum.GetValues(typeof(Define.SkillType)))
        {
            if (Managers.SkillM.HasSkill(skillType)) continue;
            SkillData data = Managers.ResourceM.Load<SkillData>(skillType.ToString());
            if (data != null) pool.Add(data);
        }

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Count <= 3 ? pool : pool.GetRange(0, 3);
    }

    private void OnSkillClicked(SkillData skillData)
    {
        _selectedSkill = skillData;

        foreach (UI_SkillItem item in _skillItems)
            item.SetSelected(item.SkillData == skillData);

        RefreshConfirmButton();
    }

    private void RefreshConfirmButton()
    {
        var btn = GetButton(typeof(Buttons), (int)Buttons.Button_Confirm);
        bool hasSelection = _selectedSkill != null;

        btn.interactable = hasSelection;
        GetImage(typeof(Images), (int)Images.Image_Confirm_BG).color     = hasSelection ? COLOR_BTN_ACTIVE     : COLOR_BTN_DEFAULT;
        GetImage(typeof(Images), (int)Images.Image_Confirm_Border).color = hasSelection ? COLOR_BORDER_ACTIVE  : COLOR_BORDER_DEFAULT;
        GetText(typeof(Texts), (int)Texts.Text_Confirm).text = hasSelection ? "선택 완료" : "스킬을 선택하세요";
    }

    private void OnConfirmClicked()
    {
        if (_selectedSkill == null) return;

        bool added = Managers.SkillM.TryAddSkill(_selectedSkill);
        if (!added)
        {
            Managers.SkillM.PendingReplaceSkill = _selectedSkill;
            Managers.UIM.ClosePopup();
            Managers.UIM.ShowPopup<UI_SkillReplacePopup>("UI_SkillReplacePopup");
            return;
        }

        Managers.UIM.ClosePopup();
    }
}
