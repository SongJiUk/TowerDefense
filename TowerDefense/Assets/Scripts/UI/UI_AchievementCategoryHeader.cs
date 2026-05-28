using Cysharp.Threading.Tasks;

/// <summary>
/// 업적 목록 카테고리 구분선 헤더. UI_AchievementPanel이 동적으로 생성.
///
/// [필요 계층 구조]
/// UI_AchievementCategoryHeader
/// ├── Text_Label   ← 카테고리 이름 (예: "⚔️  전투")
/// └── Text_Count   ← 달성 카운트 (예: "2 / 5")
/// </summary>
public class UI_AchievementCategoryHeader : UI_Base
{
    enum Texts { Text_Label, Text_Count }

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;
        BindText(typeof(Texts));
        return true;
    }

    public void Setup(string label, int unlockedCount, int totalCount)
    {
        if (!isInit) return;
        GetText(typeof(Texts), (int)Texts.Text_Label).text = label;
        GetText(typeof(Texts), (int)Texts.Text_Count).text = $"{unlockedCount} / {totalCount}";
    }
}
