using Cysharp.Threading.Tasks;
using UnityEngine;
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

    // CLAUDE.md 스테이지 테마 색 (강조색 기준)
    private static readonly Color[] STAGE_ACCENT = {
        new Color(0.545f, 0.765f, 0.290f), // #8BC34A 숲
        new Color(1.000f, 0.702f, 0.000f), // #FFB300 사막
        new Color(0.502f, 0.847f, 1.000f), // #80D8FF 겨울
        new Color(0.800f, 0.000f, 0.200f), // #CC0033 악마성
    };
    private static readonly Color COLOR_LOCKED = new Color(0.3f, 0.3f, 0.3f);

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

            var btn = GetButton(typeof(Buttons), i);
            btn.interactable = unlocked;
            // 버튼 배경 이미지에 테마 색 적용 (잠금 시 회색)
            var btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = unlocked ? STAGE_ACCENT[i] : COLOR_LOCKED;
        }
    }

    private async void OnStageSelected(int stage)
    {
        Managers.SelectedStage = stage;
        Managers.PoolM.Push(gameObject);
        var diffPopup = Managers.ObjectM.SpawnUI<UI_DifficultySelectPopup>("UI_DifficultySelectPopup", transform.parent);
        if (diffPopup != null) await diffPopup.Init();
    }

    private void OnClose()
        => Managers.PoolM.Push(gameObject);
}
