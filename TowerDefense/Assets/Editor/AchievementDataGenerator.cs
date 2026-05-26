using UnityEngine;
using UnityEditor;
using System.IO;

public class AchievementDataGenerator
{
    private const string DATA_PATH = "Assets/Data/Achievement";
    private const string DB_PATH   = "Assets/Data/Achievement/AchievementDatabase.asset";

    [MenuItem("TowerDefense/Generate Achievement Data")]
    public static void Generate()
    {
        if (!Directory.Exists(DATA_PATH))
            Directory.CreateDirectory(DATA_PATH);

        var entries = new (string id, string title, string desc, int target)[]
        {
            // 전투
            ("kill_100",    "첫 번째 학살",     "적 100마리 처치",    100),
            ("kill_500",    "숙련된 수호자",     "적 500마리 처치",    500),
            ("kill_1000",   "전장의 지배자",     "적 1000마리 처치",  1000),
            ("boss_1",      "보스 사냥꾼",       "보스 1회 처치",        1),
            ("boss_10",     "보스 킬러",         "보스 10회 처치",      10),
            ("wave_10",     "파도를 넘어",       "웨이브 10개 클리어",  10),
            ("wave_30",     "흔들리지 않는 성",  "웨이브 30개 클리어", 30),
            ("wave_50",     "난공불락",          "웨이브 50개 클리어", 50),
            // 스테이지
            ("stage_1",     "숲의 수호자",       "스테이지 1 클리어",    1),
            ("stage_2",     "사막의 방패",       "스테이지 2 클리어",    1),
            ("stage_3",     "얼음 요새",         "스테이지 3 클리어",    1),
            ("stage_4",     "악마성 정복자",     "스테이지 4 클리어",    1),
            ("clear_normal","노멀 정복",         "Normal 전 스테이지 클리어", 1),
            ("clear_hard",  "하드 정복",         "Hard 전 스테이지 클리어",   1),
            ("clear_hell",  "헬 정복",           "Hell 전 스테이지 클리어",   1),
            // 성장
            ("level_5",     "성장하는 영웅",     "레벨 5 달성",          1),
            ("level_10",    "강인한 수호자",     "레벨 10 달성",         1),
            ("level_20",    "전설의 영웅",       "레벨 20 달성",         1),
            // 기타
            ("first_login", "영웅의 탄생",       "첫 로그인",            1),
            ("set_nickname","나만의 이름",       "닉네임 설정",          1),
        };

        var db = AssetDatabase.LoadAssetAtPath<AchievementDatabase>(DB_PATH)
                 ?? ScriptableObject.CreateInstance<AchievementDatabase>();
        db.achievements.Clear();

        foreach (var (id, title, desc, target) in entries)
        {
            string path = $"{DATA_PATH}/Achievement_{id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<AchievementData>(path)
                       ?? ScriptableObject.CreateInstance<AchievementData>();

            data.id          = id;
            data.title       = title;
            data.description = desc;
            data.targetValue = target;

            if (AssetDatabase.LoadAssetAtPath<AchievementData>(path) == null)
                AssetDatabase.CreateAsset(data, path);
            else
                EditorUtility.SetDirty(data);

            db.achievements.Add(data);
        }

        if (AssetDatabase.LoadAssetAtPath<AchievementDatabase>(DB_PATH) == null)
            AssetDatabase.CreateAsset(db, DB_PATH);
        else
            EditorUtility.SetDirty(db);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AchievementGenerator] 업적 {entries.Length}개 생성 완료 → {DB_PATH}");
    }
}
