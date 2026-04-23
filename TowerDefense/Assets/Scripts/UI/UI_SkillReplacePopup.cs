using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 스킬 슬롯이 가득 찼을 때 교체할 스킬을 선택하는 팝업.
/// SkillManager.PendingReplaceSkill로 새 스킬을 받아 선택한 슬롯과 교체한다.
/// </summary>
public class UI_SkillReplacePopup : UI_Base
{
    enum Objects { Content_Horizontal }

    private bool _bindDone = false;

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

        for (int i = 0; i < 3; i++)
        {
            SkillData slotSkill = Managers.SkillM.GetSlot(i);
            if (slotSkill == null) continue;

            int capturedIndex = i;
            UI_SkillItem item = Managers.ObjectM.SpawnUI<UI_SkillItem>("UI_SkillItem", parent);
            await item.Init();
            item.SetInfo(slotSkill, _ =>
            {
                Managers.SkillM.SetSlot(capturedIndex, Managers.SkillM.PendingReplaceSkill);
                Managers.SkillM.PendingReplaceSkill = null;
                Managers.UIM.ClosePopup();
            }, Managers.SkillM.GetSkillLevel(slotSkill));
        }
    }
}
