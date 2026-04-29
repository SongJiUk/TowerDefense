using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 스테이지 선택 팝업 (타이틀씬).
/// 오브젝트 이름:
///   Button_Stage1 ~ Button_Stage4
///   Text_Stage1 ~ Text_Stage4
///   Image_Lock1 ~ Image_Lock4
///   Button_Close
/// </summary>
public class UI_StageSelectPopup : UI_Base
{
    enum Buttons { Button_Stage1, Button_Stage2, Button_Stage3, Button_Stage4, Button_Close }
    enum Texts   { Text_Stage1, Text_Stage2, Text_Stage3, Text_Stage4 }
    enum Images  { Image_Lock1, Image_Lock2, Image_Lock3, Image_Lock4 }

    private static readonly string[] STAGE_NAMES =
        { "1 - 숲", "2 - 사막", "3 - 겨울", "4 - 악마성" };

    private bool _initialized;

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;
        _initialized = true;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        for (int i = 0; i < 4; i++)
        {
            int stage = i + 1;
            GetButton(typeof(Buttons), i).onClick.AddListener(() => OnStageSelected(stage));
        }
        GetButton(typeof(Buttons), (int)Buttons.Button_Close).onClick.AddListener(OnClose);

        Refresh();
        return true;
    }

    private void Refresh()
    {
        for (int i = 0; i < 4; i++)
        {
            int stage = i + 1;
            bool unlocked = stage == 1 || Managers.SaveM.IsStageCleared(stage - 1);
            bool cleared  = Managers.SaveM.IsStageCleared(stage);

            string label = STAGE_NAMES[i] + (cleared ? " ✓" : "");
            GetText(typeof(Texts), i).text = label;
            GetImage(typeof(Images), i).gameObject.SetActive(!unlocked);
            GetButton(typeof(Buttons), i).interactable = unlocked;
        }
    }

    private void OnStageSelected(int stage)
    {
        Managers.SelectedStage = stage;
        Managers.PoolM.Push(gameObject);
        // 난이도 선택 팝업 열기
        var diffPopup = Managers.ObjectM.SpawnUI<UI_DifficultySelectPopup>("UI_DifficultySelectPopup", transform.parent);
        if (diffPopup != null) diffPopup.gameObject.SetActive(true);
    }

    private void OnClose()
        => Managers.PoolM.Push(gameObject);
}
