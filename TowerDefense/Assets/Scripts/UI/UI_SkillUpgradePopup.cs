using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// SkillPointUp 카드 선택 시 보유 스킬 중 업그레이드할 스킬을 선택하는 팝업.
/// 마스터 레벨(upgradeSteps 최대)에 도달한 스킬은 표시하지 않는다.
/// 업그레이드 가능한 스킬이 없으면 즉시 닫는다.
/// </summary>
public class UI_SkillUpgradePopup : UI_Base
{
    enum Objects { Content_Horizontal }

    private bool _bindDone = false;

    async void Start() => await Init();

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

        bool hasUpgradeable = false;
        for (int i = 0; i < 3; i++)
        {
            SkillData slotSkill = Managers.SkillM.GetSlot(i);
            if (slotSkill == null || !Managers.SkillM.CanUpgrade(i)) continue;

            hasUpgradeable = true;
            int capturedIndex = i;
            UI_SkillItem item = Managers.ObjectM.SpawnUI<UI_SkillItem>("UI_SkillItem", parent);
            await item.Init();
            item.SetInfo(slotSkill, _ =>
            {
                Managers.SkillM.UpgradeSkill(capturedIndex);
                Managers.SkillM.ConsumeSkillPoint();
                Managers.UIM.ClosePopup();
            }, Managers.SkillM.GetSkillLevel(slotSkill));
        }

        if (!hasUpgradeable)
            Managers.UIM.ClosePopup();
    }
}
