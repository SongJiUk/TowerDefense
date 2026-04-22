using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot : UI_Base
{
    enum Images { Image_Skill, Image_CoolDown, Image_SlotGlow }
    enum Texts { Text_SkillName, Text_SkillLevel }
    enum Buttons { Button_Skill }
    enum GameObjects { Object_ReadyDot }

    private int _slotIndex;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    async void Start()
    {
        await Init();
    }

    void OnDestroy()
    {
        Managers.SkillM.OnSlotChanged -= OnSlotChanged;
        Managers.SkillM.OnCooldownChanged -= OnCooldownChanged;
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        BindEvent(
            GetButton(typeof(Buttons), (int)Buttons.Button_Skill).gameObject,
            OnSkillClicked
        );

        Managers.SkillM.OnSlotChanged += OnSlotChanged;
        Managers.SkillM.OnCooldownChanged += OnCooldownChanged;

        Refresh();
        return true;
    }

    public void SetSlotIndex(int index)
    {
        _slotIndex = index;
        Refresh();
    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    private void Refresh()
    {
        if (!isInit) return;

        SkillData skill = Managers.SkillM.GetSlot(_slotIndex);

        bool hasSkill = skill != null;
        GetImage(typeof(Images), (int)Images.Image_Skill).gameObject.SetActive(hasSkill);
        GetText(typeof(Texts), (int)Texts.Text_SkillName).gameObject.SetActive(hasSkill);
        GetText(typeof(Texts), (int)Texts.Text_SkillLevel).gameObject.SetActive(hasSkill);

        if (!hasSkill)
        {
            GetImage(typeof(Images), (int)Images.Image_CoolDown).fillAmount = 0f;
            return;
        }

        GetText(typeof(Texts), (int)Texts.Text_SkillName).text = skill.skillName;
        GetText(typeof(Texts), (int)Texts.Text_SkillLevel).text = $"Lv.{Managers.SkillM.GetSkillLevel(skill)}";

        var sprite = Managers.ResourceM.GetAtlas(skill.skillType.ToString());
        if (sprite != null)
            GetImage(typeof(Images), (int)Images.Image_Skill).sprite = sprite;

        GetImage(typeof(Images), (int)Images.Image_CoolDown).fillAmount =
            Managers.SkillM.GetCooldownRatio(_slotIndex);

        GetImage(typeof(Images), (int)Images.Image_SlotGlow).color = skill.color;
    }

    // ─── 이벤트 ───────────────────────────────────────────────────────────────

    private void OnSlotChanged(int index, SkillData skill)
    {
        if (index != _slotIndex) return;
        Refresh();
    }

    private void OnCooldownChanged(int index, float ratio)
    {
        if (index != _slotIndex) return;
        if (!isInit) return;
        GetImage(typeof(Images), (int)Images.Image_CoolDown).fillAmount = ratio;

        if (ratio == 0) GetObject(typeof(GameObjects), (int)GameObjects.Object_ReadyDot).SetActive(true);
    }

    private void OnSkillClicked()
    {
        if (Managers.SkillM.GetSlot(_slotIndex) == null) return;
        Managers.SkillM.Activate(_slotIndex);
    }
}
