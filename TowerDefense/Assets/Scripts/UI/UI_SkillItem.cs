using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UI_SkillItem : UI_Base
{
    enum Images { Image_Icon }
    enum Texts { Text_Level, Text_SkillName, Text_Description }
    enum Buttons { Button_Skill }

    private SkillData _skillData;
    private Action<SkillData> _onSelected;
    private int _level;

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        BindEvent(
            GetButton(typeof(Buttons), (int)Buttons.Button_Skill).gameObject,
            OnSkillClicked
        );

        return true;
    }

    public void SetInfo(SkillData skillData, Action<SkillData> onSelected, int level = 0)
    {
        _skillData = skillData;
        _onSelected = onSelected;
        _level = level;
        Refresh();
    }

    private void Refresh()
    {
        if (!isInit || _skillData == null) return;

        GetText(typeof(Texts), (int)Texts.Text_SkillName).text = _skillData.skillName;
        GetText(typeof(Texts), (int)Texts.Text_Description).text = $"쿨타임: {_skillData.cooldown}초";
        GetText(typeof(Texts), (int)Texts.Text_Level).text = _level > 0 ? $"Lv.{_level}" : "";

        var sprite = Managers.ResourceM.GetAtlas(_skillData.skillType.ToString());
        if (sprite != null)
            GetImage(typeof(Images), (int)Images.Image_Icon).sprite = sprite;
    }

    private void OnSkillClicked()
    {
        _onSelected?.Invoke(_skillData);
    }
}
