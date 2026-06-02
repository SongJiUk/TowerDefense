using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 업적 목록 전체 패널. 메인메뉴에서 열고 X 버튼으로 닫는다.
/// 열릴 때 동적으로 카테고리 헤더 + 아이템 생성, 닫힐 때 전체 Destroy.
///
/// Addressables에 등록 필요:
///   "UI_AchievementPanel"        — 이 프리팹
///   "UI_AchievementCategoryHeader" — 헤더 프리팹
///   "UI_AchievementItem"           — 아이템 카드 프리팹
///   모두 PrevLoad 레이블 포함
///
/// [필요 계층 구조]
/// UI_AchievementPanel
/// ├── Button_Close
/// ├── Text_Count          ← "6 / 20"
/// ├── Image_OverallFill   ← fillAmount으로 전체 진행률 표시
/// └── ScrollView
///     └── Viewport
///         └── Object_Content  ← VerticalLayoutGroup + ContentSizeFitter
/// </summary>
public class UI_AchievementPanel : UI_Base
{
    enum Texts       { Text_Count, Text_FillPercent }
    enum Images      { Image_OverallFill }
    enum Buttons     { Button_Close }
    enum GameObjects { Object_Content }

    private static readonly (string label, string[] ids)[] CATEGORIES =
    {
        ("⚔️  전투",     new[] { "kill_100", "kill_500", "kill_1000", "boss_1", "boss_10" }),
        ("🌊  웨이브",   new[] { "wave_10", "wave_30", "wave_50" }),
        ("🏰  스테이지", new[] { "stage_1", "stage_2", "stage_3", "stage_4", "clear_normal", "clear_hard", "clear_hell" }),
        ("🌟  성장",     new[] { "level_5", "level_10", "level_20" }),
        ("📋  기타",     new[] { "first_login", "set_nickname" }),
    };

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton(typeof(Buttons), (int)Buttons.Button_Close).onClick.AddListener(OnClose);

        await BuildListAsync();
        RefreshOverall();

        return true;
    }

    private async UniTask BuildListAsync()
    {
        var db = Managers.AchievementM?.Database;
        if (db == null) return;

        var headerPrefab = Managers.ResourceM.Load<GameObject>("UI_AchievementCategoryHeader");
        var itemPrefab   = Managers.ResourceM.Load<GameObject>("UI_AchievementItem");
        if (headerPrefab == null || itemPrefab == null)
        {
            Debug.LogError("[UI_AchievementPanel] 프리팹 로드 실패. PrevLoad 레이블 확인 필요.");
            return;
        }

        var content = GetObject(typeof(GameObjects), (int)GameObjects.Object_Content).transform;

        foreach (var (label, ids) in CATEGORIES)
        {
            var entries = new List<(AchievementData data, int progress, bool unlocked)>();
            int unlockedCount = 0;

            foreach (var id in ids)
            {
                var data = db.Get(id);
                if (data == null) continue;
                bool unlocked = Managers.AchievementM.IsUnlocked(id);
                int  progress = Managers.AchievementM.GetProgress(id);
                entries.Add((data, progress, unlocked));
                if (unlocked) unlockedCount++;
            }

            var headerGo = Object.Instantiate(headerPrefab, content);
            var header   = headerGo.GetComponent<UI_AchievementCategoryHeader>();
            if (header != null)
            {
                await header.Init();
                header.Setup(label, unlockedCount, entries.Count);
            }

            // 달성된 항목 먼저 (안정 정렬 — 카테고리 내 원본 순서 유지)
            entries = entries.OrderByDescending(e => e.unlocked ? 1 : 0).ToList();

            foreach (var (data, progress, unlocked) in entries)
            {
                var itemGo = Object.Instantiate(itemPrefab, content);
                var item   = itemGo.GetComponent<UI_AchievementItem>();
                if (item == null) continue;
                await item.Init();
                item.Setup(data, progress, unlocked);
            }
        }
    }

    private void RefreshOverall()
    {
        var db = Managers.AchievementM?.Database;
        if (db == null) return;

        int total    = db.achievements.Count;
        int unlocked = 0;
        foreach (var a in db.achievements)
            if (Managers.AchievementM.IsUnlocked(a.id)) unlocked++;

        float ratio = total > 0 ? (float)unlocked / total : 0f;

        GetText(typeof(Texts), (int)Texts.Text_Count).text = $"{unlocked}";
        GetText(typeof(Texts), (int)Texts.Text_FillPercent).text = $"{Mathf.RoundToInt(ratio * 100)}%";
        GetImage(typeof(Images), (int)Images.Image_OverallFill).fillAmount = ratio;
    }

    private void OnClose() => Managers.UIM.ClosePopup();
}
