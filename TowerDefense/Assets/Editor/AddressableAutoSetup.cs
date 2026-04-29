using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

/// <summary>
/// TowerDefense/MyEditor/Addressables PrevLoad 자동 설정
/// 실행하면 PrevLoad 그룹에 필요한 에셋을 모두 등록하고 키를 파일명으로 설정.
/// </summary>
public static class AddressableAutoSetup
{
    private const string LABEL = "PrevLoad";
    private const string GROUP_NAME = "PrevLoad";

    [MenuItem("TowerDefense/MyEditor/Addressables PrevLoad 자동 설정")]
    public static void Run()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[AddressableSetup] Addressable Settings 없음. Window → Asset Management → Addressables → Groups에서 먼저 초기화하세요.");
            return;
        }

        if (!settings.GetLabels().Contains(LABEL))
            settings.AddLabel(LABEL);

        var group = settings.FindGroup(GROUP_NAME)
                 ?? settings.CreateGroup(GROUP_NAME, false, false, false, null);

        int count = 0;

        // ── 개별 파일 (재귀 포함) ─────────────────────────────────────────────

        // 카드 데이터 (TypeA/B/C/D 하위폴더 포함)
        RegisterFolder(settings, group, "Assets/Data/Card", "*.asset", true, ref count);

        // 적·타워·스테이지·스킬·시너지 데이터
        RegisterFolder(settings, group, "Assets/Data/Enemies",  "*.asset", false, ref count);
        RegisterFolder(settings, group, "Assets/Data/Towers",   "*.asset", false, ref count);
        RegisterFolder(settings, group, "Assets/Data/Stages",   "*.asset", false, ref count);
        RegisterFolder(settings, group, "Assets/Data/Skills",   "*.asset", false, ref count);
        RegisterFolder(settings, group, "Assets/Data/Synergy",  "*.asset", false, ref count);

        // LevelData (Assets/Data 직속)
        RegisterAsset(settings, group, "Assets/Data/LevelData.asset", ref count);

        // 적·타워 프리팹
        RegisterFolder(settings, group, "Assets/Prefabs/Enemies", "*.prefab", false, ref count);
        RegisterFolder(settings, group, "Assets/Prefabs/Tower",   "*.prefab", false, ref count);

        // UI 프리팹
        RegisterFolder(settings, group, "Assets/Prefabs/UI", "*.prefab", false, ref count);

        // 단독 프리팹
        RegisterAsset(settings, group, "Assets/Prefabs/Marker.prefab", ref count);

        AssetDatabase.SaveAssets();
        Debug.Log($"[AddressableSetup] 완료 — {count}개 에셋을 PrevLoad 그룹에 등록했습니다.");
    }

    // ── 내부 헬퍼 ─────────────────────────────────────────────────────────────

    private static void RegisterFolder(
        AddressableAssetSettings settings,
        AddressableAssetGroup group,
        string folder, string filter,
        bool recursive, ref int count)
    {
        if (!AssetDatabase.IsValidFolder(folder)) return;

        string[] guids = AssetDatabase.FindAssets("", new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(filter.TrimStart('*'))) continue;

            // 재귀 아닐 때 직접 하위만 허용
            if (!recursive)
            {
                string relative = path.Substring(folder.Length + 1);
                if (relative.Contains("/")) continue;
            }

            RegisterEntry(settings, group, path, guid, ref count);
        }
    }

    private static void RegisterAsset(
        AddressableAssetSettings settings,
        AddressableAssetGroup group,
        string assetPath, ref int count)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid)) return;
        RegisterEntry(settings, group, assetPath, guid, ref count);
    }

    private static void RegisterEntry(
        AddressableAssetSettings settings,
        AddressableAssetGroup group,
        string path, string guid, ref int count)
    {
        var entry = settings.FindAssetEntry(guid)
                 ?? settings.CreateOrMoveEntry(guid, group);

        if (entry.parentGroup != group)
            settings.MoveEntry(entry, group);

        entry.address = Path.GetFileNameWithoutExtension(path);
        entry.SetLabel(LABEL, true);
        count++;
    }
}
