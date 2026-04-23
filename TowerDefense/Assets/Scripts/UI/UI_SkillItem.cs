using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillItem : UI_Base
{
    enum Images
    {
        Image_Border,
        Image_Category_Border,
        Image_Icon_Border, Image_Icon_BG, Image_Icon,
    }
    enum Texts { Text_CurrentLevel, Text_MaxLevel, Text_SkillName, Text_SkillType, Text_Description, Text_CoolTime }
    enum Buttons { Button_Card }

    private SkillData _skillData;
    private Action<SkillData> _onSelected;
    private int _level;
    private bool _isUpgrade;

    public SkillData SkillData => _skillData;

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        BindEvent(
            GetButton(typeof(Buttons), (int)Buttons.Button_Card).gameObject,
            OnSkillClicked
        );

        return true;
    }

    public void SetInfo(SkillData skillData, Action<SkillData> onSelected, int level = 0, bool isUpgrade = false)
    {
        _skillData = skillData;
        _onSelected = onSelected;
        _level = level;
        _isUpgrade = isUpgrade;
        transform.DOKill();
        transform.localScale = Vector3.one;
        Refresh();
    }

    private void Refresh()
    {
        if (!isInit || _skillData == null) return;

        GetText(typeof(Texts), (int)Texts.Text_SkillName).text    = _skillData.skillName;
        GetText(typeof(Texts), (int)Texts.Text_SkillType).text    = GetSkillTypeLabel(_skillData.skillType);
        bool hasStep = _isUpgrade && _level < _skillData.upgradeSteps.Length;
        SkillUpgradeStep step = hasStep ? _skillData.upgradeSteps[_level] : null;

        GetText(typeof(Texts), (int)Texts.Text_Description).text  = hasStep
            ? BuildStatDesc(_skillData, step)
            : _skillData.Description;
        GetText(typeof(Texts), (int)Texts.Text_CurrentLevel).text = _level > 0 ? $"{_level}" : "0";
        GetText(typeof(Texts), (int)Texts.Text_MaxLevel).text     = "/ 3";

        var sprite = Managers.ResourceM.GetAtlas(_skillData.skillType.ToString());
        if (sprite != null)
            GetImage(typeof(Images), (int)Images.Image_Icon).sprite = sprite;

        Color bgColor     = GetTypeBGColor(_skillData.skillType);
        Color borderColor = GetTypeBorderColor(_skillData.skillType);

        GetImage(typeof(Images), (int)Images.Image_Icon_BG).color         = bgColor;
        GetImage(typeof(Images), (int)Images.Image_Border).color          = borderColor;
        GetImage(typeof(Images), (int)Images.Image_Category_Border).color = borderColor;
        GetImage(typeof(Images), (int)Images.Image_Icon_Border).color     = borderColor;

        GetText(typeof(Texts), (int)Texts.Text_SkillType).color = borderColor;
        GetText(typeof(Texts), (int)Texts.Text_CoolTime).text   = hasStep && step.cooldownReduction > 0f
            ? $"쿨타임 {_skillData.cooldown:F1}초 ->{_skillData.cooldown - step.cooldownReduction:F1}초"
            : $"쿨타임 {_skillData.cooldown}초";
    }

    public void SetSelected(bool selected)
    {
        transform.DOKill();
        var border = GetImage(typeof(Images), (int)Images.Image_Border);
        Color borderColor = GetTypeBorderColor(_skillData.skillType);

        if (selected)
        {
            border.DOColor(Color.Lerp(borderColor, Color.white, 0.5f), 0.15f).SetUpdate(true);
            transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        else
        {
            border.DOColor(borderColor, 0.15f).SetUpdate(true);
            transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    private static string GetSkillTypeLabel(Define.SkillType skillType) => skillType switch
    {
        Define.SkillType.ArrowRain      => "즉발",
        Define.SkillType.LightningStorm => "즉발",
        Define.SkillType.Freeze         => "지속",
        Define.SkillType.PoisonMist     => "지속",
        Define.SkillType.Block          => "설치",
        _ => ""
    };

    private static Color GetTypeBGColor(Define.SkillType skillType) => skillType switch
    {
        Define.SkillType.ArrowRain      => new Color(0.18f, 0.08f, 0.02f, 1f), // 주황 계열
        Define.SkillType.LightningStorm => new Color(0.10f, 0.04f, 0.18f, 1f), // 보라 계열
        Define.SkillType.Freeze         => new Color(0.02f, 0.10f, 0.20f, 1f), // 파랑 계열
        Define.SkillType.PoisonMist     => new Color(0.04f, 0.15f, 0.04f, 1f), // 초록 계열
        Define.SkillType.Block          => new Color(0.12f, 0.10f, 0.08f, 1f), // 갈색 계열
        _ => Color.black
    };

    private static Color GetTypeBorderColor(Define.SkillType skillType) => skillType switch
    {
        Define.SkillType.ArrowRain      => new Color(0.90f, 0.55f, 0.10f, 1f), // 금주황
        Define.SkillType.LightningStorm => new Color(0.55f, 0.20f, 0.90f, 1f), // 보라
        Define.SkillType.Freeze         => new Color(0.20f, 0.60f, 0.90f, 1f), // 하늘
        Define.SkillType.PoisonMist     => new Color(0.30f, 0.75f, 0.20f, 1f), // 초록
        Define.SkillType.Block          => new Color(0.70f, 0.60f, 0.45f, 1f), // 베이지
        _ => Color.white
    };

    private static string BuildStatDesc(SkillData data, SkillUpgradeStep step)
    {
        var sb = new System.Text.StringBuilder();
        if (step.damageMultiplier > 1f)
            sb.AppendLine($"데미지  {data.baseDamage:F0} ->{data.baseDamage * step.damageMultiplier:F0}");
        if (step.rangeBonus > 0f)
            sb.AppendLine($"범위  {data.baseRange:F1} ->{data.baseRange + step.rangeBonus:F1}");
        if (step.skillDuration > 0f)
            sb.AppendLine($"지속  {data.baseDuration:F1}초 ->{data.baseDuration + step.skillDuration:F1}초");
        return sb.Length > 0 ? sb.ToString().TrimEnd() : data.Description;
    }

    private void OnSkillClicked()
    {
        _onSelected?.Invoke(_skillData);
    }
}
