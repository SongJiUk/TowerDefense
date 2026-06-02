using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 업적 카드 하나. UI_AchievementPanel이 동적으로 Instantiate해서 사용.
///
/// [필요 계층 구조]
/// UI_AchievementItem
/// ├── Image_CardBorder      ← 달성 시 금색, 미달성 시 기본색
/// ├── Image_Icon            ← 업적 아이콘 (추후 교체)
/// ├── Object_Check          ← 달성 시만 표시 (체크마크 아이콘)
/// └── Panel_Right
///     ├── Text_Title        ← 업적 제목
///     ├── Object_DoneBadge  ← "DONE" 뱃지 (달성 시만 표시)
///     ├── Text_Desc         ← 업적 설명
///     ├── Object_ProgressBar← 진행중 (targetValue > 1, 미달성)
///     │   ├── Image_ProgressFill
///     │   └── Text_ProgressCount  ← "428 / 1,000"
///     └── Text_NotDone      ← "달성 전" (targetValue == 1, 미달성)
/// </summary>
public class UI_AchievementItem : UI_Base
{
    enum Texts       { Text_Title, Text_Desc, Text_ProgressCount, Text_NotDone }
    enum Images      { Image_ProgressFill, Image_CardBorder, Image_Icon }
    enum GameObjects { Object_DoneBadge, Object_ProgressBar, Object_Check }

    private static readonly Color COLOR_DONE   = new Color(1f, 0.78f, 0.2f);
    private static readonly Color COLOR_NORMAL = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindObject(typeof(GameObjects));
        return true;
    }

    public void Setup(AchievementData data, int progress, bool unlocked)
    {
        if (data == null) return;

        GetText(typeof(Texts), (int)Texts.Text_Title).text = data.title;
        GetText(typeof(Texts), (int)Texts.Text_Desc).text  = data.description;

        var icon = Managers.ResourceM.GetAtlas(data.id);
        if (icon != null)
            GetImage(typeof(Images), (int)Images.Image_Icon).sprite = icon;

        bool trackable = data.targetValue >= 1;

        GetObject(typeof(GameObjects), (int)GameObjects.Object_DoneBadge).SetActive(unlocked);
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Check).SetActive(unlocked);

        if (unlocked)
        {
            GetObject(typeof(GameObjects), (int)GameObjects.Object_ProgressBar).SetActive(false);
            GetText(typeof(Texts), (int)Texts.Text_NotDone).gameObject.SetActive(false);
            GetImage(typeof(Images), (int)Images.Image_CardBorder).color = COLOR_DONE;
        }
        else if (trackable)
        {
            GetObject(typeof(GameObjects), (int)GameObjects.Object_ProgressBar).SetActive(true);
            GetText(typeof(Texts), (int)Texts.Text_NotDone).gameObject.SetActive(false);
            float ratio = Mathf.Clamp01((float)progress / data.targetValue);
            GetImage(typeof(Images), (int)Images.Image_ProgressFill).fillAmount = ratio;
            GetText(typeof(Texts), (int)Texts.Text_ProgressCount).text = $"{progress:N0} / {data.targetValue:N0}";
            GetImage(typeof(Images), (int)Images.Image_CardBorder).color = COLOR_NORMAL;
        }
        else
        {
            GetObject(typeof(GameObjects), (int)GameObjects.Object_ProgressBar).SetActive(false);
            GetText(typeof(Texts), (int)Texts.Text_NotDone).gameObject.SetActive(true);
            GetImage(typeof(Images), (int)Images.Image_CardBorder).color = COLOR_NORMAL;
        }
    }
}
