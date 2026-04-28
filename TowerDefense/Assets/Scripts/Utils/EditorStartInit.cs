#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class EditorStartInit
{
    private const string TEST_STAGE_KEY = "TestMode_Stage";

    [MenuItem("TowerDefense/MyEditor/시작 씬부터 시작")]
    public static void SetupFromStartScene()
    {
        var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
        EditorSceneManager.playModeStartScene = sceneAsset;
        EditorApplication.isPlaying = true;
    }

    [MenuItem("TowerDefense/MyEditor/현재 씬부터 시작")]
    public static void StartFromThisScene()
    {
        EditorSceneManager.playModeStartScene = null;
        EditorApplication.isPlaying = true;
    }

    [MenuItem("TowerDefense/MyEditor/테스트모드 - Stage 1")]
    public static void TestStage1() => StartTestMode(1);

    [MenuItem("TowerDefense/MyEditor/테스트모드 - Stage 2")]
    public static void TestStage2() => StartTestMode(2);

    [MenuItem("TowerDefense/MyEditor/테스트모드 - Stage 3")]
    public static void TestStage3() => StartTestMode(3);

    [MenuItem("TowerDefense/MyEditor/테스트모드 - Stage 4")]
    public static void TestStage4() => StartTestMode(4);

    private static void StartTestMode(int stage)
    {
        EditorPrefs.SetInt(TEST_STAGE_KEY, stage);

        string gamescenePath = FindScenePath("GameScene");
        if (gamescenePath == null)
        {
            Debug.LogError("[TestMode] GameScene을 Build Settings에서 찾을 수 없음");
            return;
        }

        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(gamescenePath);
        EditorSceneManager.playModeStartScene = sceneAsset;
        EditorApplication.isPlaying = true;
    }

    private static string FindScenePath(string sceneName)
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.path.Contains(sceneName))
                return scene.path;
        }
        return null;
    }
}
#endif
