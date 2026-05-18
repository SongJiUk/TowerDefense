using UnityEngine;
using Firebase.Database;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Newtonsoft.Json;

public partial class FirebaseManager
{
    private CancellationTokenSource _writeCts;

    // ─── 저장 ─────────────────────────────────────────────────────────────────

    public async UniTask WriteData()
    {
        if (DB == null || CurrentUser == null) return;

        // 연속 저장 요청 시 이전 것 취소 → 항상 최신 데이터만 저장
        _writeCts?.Cancel();
        _writeCts?.Dispose();
        _writeCts = new CancellationTokenSource();
        var token = _writeCts.Token;

        SaveData data = Managers.SaveM?.Data;
        if (data == null) return;

        string json    = JsonConvert.SerializeObject(data);
        var    userRef = DB.Child("users").Child(CurrentUser.UserId).Child("save");

        const int maxRetry = 3;
        for (int i = 0; i < maxRetry; i++)
        {
            if (token.IsCancellationRequested) return;
            try
            {
                await userRef.SetRawJsonValueAsync(json).AsUniTask();
                Debug.Log("[Firebase] 저장 완료");
                return;
            }
            catch (Exception e)
            {
                if (i == maxRetry - 1)
                    Debug.LogError($"[Firebase] 저장 {maxRetry}회 실패: {e.Message}");
                else
                {
                    Debug.LogWarning($"[Firebase] 저장 실패 ({i + 1}/{maxRetry}), 재시도...");
                    await UniTask.WaitForSeconds(2f, ignoreTimeScale: true, cancellationToken: token);
                }
            }
        }
    }

    // ─── 불러오기 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 서버 데이터 불러와 SaveManager에 적용. 로그인 직후 1회 호출.
    /// 서버 데이터가 없으면 신규 유저로 처리.
    /// </summary>
    public async UniTask<bool> ReadData()
    {
        if (DB == null || CurrentUser == null) return false;

        try
        {
            var snap = await DB.Child("users").Child(CurrentUser.UserId).Child("save")
                               .GetValueAsync().AsUniTask();

            SaveData loaded;
            if (snap.Exists)
            {
                string json = snap.GetRawJsonValue();
                loaded = JsonConvert.DeserializeObject<SaveData>(json) ?? new SaveData();
                Debug.Log("[Firebase] 데이터 불러오기 완료");
            }
            else
            {
                loaded = new SaveData();
                Debug.Log("[Firebase] 신규 유저 — 기본 데이터 사용");
            }

            // 로그인 사용자 이름 덮어쓰기 방지 (ApplyUserToSaveData가 이미 설정)
            loaded.IsGuest    = Managers.SaveM.Data.IsGuest;
            loaded.PlayerName = Managers.SaveM.Data.PlayerName;

            Managers.SaveM.ApplyLoaded(loaded);
            Managers.SaveM.SwitchToFirebase();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] 불러오기 실패: {e.Message}");
            return false;
        }
    }
}
