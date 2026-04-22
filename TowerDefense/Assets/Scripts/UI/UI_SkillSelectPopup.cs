using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkillSelect 카드 선택 시 표시되는 스킬 선택 팝업.
/// 보유하지 않은 스킬 목록을 보여주고, 선택 시 SkillManager에 등록한다.
/// 슬롯이 가득 찬 경우 교체할 슬롯을 먼저 선택한다.
/// </summary>
public class UI_SkillSelectPopup : UI_Base
{
    enum Objects { Content_Horizontal }

    private bool _bindDone = false;

    async void Start()
    {
        await Init();
    }

    void OnEnable()
    {
        if (!_bindDone) return;
        BuildSkills().Forget();
    }

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindObject(typeof(Objects));
        _bindDone = true;

        await BuildSkills();
        return true;
    }

    private async UniTask BuildSkills()
    {
        Transform parent = GetObject(typeof(Objects), (int)Objects.Content_Horizontal).transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);

        List<SkillData> skills = GetAvailableSkills();
        foreach (SkillData skill in skills)
        {
            UI_SkillItem item = Managers.ObjectM.SpawnUI<UI_SkillItem>("UI_SkillItem", parent);
            await item.Init();
            item.SetInfo(skill, OnSkillSelected);
        }
    }

    /// <summary>미보유 스킬 중 랜덤 3개 반환.</summary>
    private List<SkillData> GetAvailableSkills()
    {
        var pool = new List<SkillData>();
        foreach (Define.SkillType skillType in System.Enum.GetValues(typeof(Define.SkillType)))
        {
            if (Managers.SkillM.HasSkill(skillType)) continue;
            SkillData data = Managers.ResourceM.Load<SkillData>(skillType.ToString());
            if (data != null) pool.Add(data);
        }

        // 피셔-예이츠 셔플
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Count <= 3 ? pool : pool.GetRange(0, 3);
    }

    private void OnSkillSelected(SkillData skillData)
    {
        bool added = Managers.SkillM.TryAddSkill(skillData);

        if (!added)
        {
            Managers.SkillM.PendingReplaceSkill = skillData;
            Managers.UIM.ClosePopup();
            Managers.UIM.ShowPopup<UI_SkillReplacePopup>("UI_SkillReplacePopup");
            return;
        }

        Managers.UIM.ClosePopup();
    }
}
