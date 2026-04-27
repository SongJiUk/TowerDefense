using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot : UI_Base
{
    enum Images { Image_Skill, Image_CoolDown, Image_SlotGlow }
    enum Texts { Text_SkillName, Text_SkillLevel, Text_CoolDown }
    enum Buttons { Button_Skill }
    enum GameObjects { Object_ReadyDot }

    private int _slotIndex;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

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

        var btn = GetButton(typeof(Buttons), (int)Buttons.Button_Skill);
        BindEvent(btn.gameObject, OnSkillClicked);

        BindEvent(btn.gameObject, () =>
        {
            if (Managers.SkillM.GetCooldownRatio(_slotIndex) <= 0f)
                transform.DOScale(1.12f, 0.12f).SetEase(Ease.OutBack).SetUpdate(true);
        }, _type: Define.UIEvent.PointerEnter);

        BindEvent(btn.gameObject, null,
            _ => transform.DOScale(1f, 0.12f).SetEase(Ease.OutQuad).SetUpdate(true),
            Define.UIEvent.OnPointerExit);

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
        GetText(typeof(Texts), (int)Texts.Text_SkillName).color = skill.color;
        GetText(typeof(Texts), (int)Texts.Text_SkillLevel).text = $"Lv.{Managers.SkillM.GetSkillLevel(skill)}";

        var sprite = Managers.ResourceM.GetAtlas(skill.skillType.ToString());
        if (sprite != null)
            GetImage(typeof(Images), (int)Images.Image_Skill).sprite = sprite;

        float ratio = Managers.SkillM.GetCooldownRatio(_slotIndex);
        var cooldownImg = GetImage(typeof(Images), (int)Images.Image_CoolDown);
        cooldownImg.fillAmount = ratio;
        cooldownImg.gameObject.SetActive(ratio > 0f);
        GetObject(typeof(GameObjects), (int)GameObjects.Object_ReadyDot).SetActive(ratio <= 0f);

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
        var cooldownImg = GetImage(typeof(Images), (int)Images.Image_CoolDown);
        cooldownImg.fillAmount = ratio;
        cooldownImg.gameObject.SetActive(ratio > 0f);

        var cdText = GetText(typeof(Texts), (int)Texts.Text_CoolDown);
        float remaining = Managers.SkillM.GetCooldownRemaining(_slotIndex);
        cdText.text = remaining > 0f ? $"{remaining:F1}" : "";
        cdText.gameObject.SetActive(remaining > 0f);

        GetObject(typeof(GameObjects), (int)GameObjects.Object_ReadyDot).SetActive(ratio <= 0f);
    }

    private void OnSkillClicked()
    {
        if (Managers.SkillM.GetSlot(_slotIndex) == null) return;
        Managers.SkillM.Activate(_slotIndex);
    }
}
