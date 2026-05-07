using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TestStartMenu
{
    private const string PREF_KEY = "TD_IsTestMode";

    [MenuItem("TowerDefense/MyEditor/테스트 시작")]
    private static void StartTestMode()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[TestStart] 이미 플레이 중입니다.");
            return;
        }

        string scenePath = GetScenePath("GameScene");
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError("[TestStart] GameScene이 빌드 세팅에 없습니다. File > Build Settings에 추가하세요.");
            return;
        }

        SessionState.SetBool(PREF_KEY, true);
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

        EditorApplication.playModeStateChanged += RestoreStartScene;
        EditorApplication.isPlaying = true;
    }

    private static void RestoreStartScene(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingPlayMode) return;
        EditorSceneManager.playModeStartScene = null;
        EditorApplication.playModeStateChanged -= RestoreStartScene;
    }

    private static string GetScenePath(string sceneName)
    {
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.path.Contains(sceneName)) return scene.path;
        return string.Empty;
    }
}
