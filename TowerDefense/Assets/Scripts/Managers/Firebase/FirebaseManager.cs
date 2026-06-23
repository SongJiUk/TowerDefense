using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// Firebase 초기화 및 인증 상태 관리.
/// FirebaseLogin.cs (로그인), FirebaseDB.cs (저장/로드) 와 partial class로 분리.
/// </summary>
public partial class FirebaseManager : MonoBehaviour
{
    public FirebaseAuth Auth { get; private set; }
    public DatabaseReference DB { get; private set; }
    public FirebaseUser CurrentUser { get; private set; }

    public bool IsReady { get; private set; } = false;

    private bool _isInitializing = true;

    public async UniTask Init()
    {
        if (IsReady)
            return;

        var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[Firebase] 초기화 실패: {status}");
            return;
        }

        Auth = FirebaseAuth.DefaultInstance;
        DB   = FirebaseDatabase.DefaultInstance.RootReference;
        Auth.StateChanged += OnAuthStateChanged;

        SetupGoogleConfig();

        CurrentUser = Auth.CurrentUser;
        _isInitializing = false;
        IsReady = true;

        Debug.Log($"[Firebase] 초기화 완료. 현재 유저: {CurrentUser?.UserId ?? "없음"}");
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (_isInitializing) return;
        if (Auth.CurrentUser == CurrentUser) return;

        CurrentUser = Auth.CurrentUser;
        Debug.Log(CurrentUser != null
            ? $"[Firebase] 로그인: {CurrentUser.UserId}"
            : "[Firebase] 로그아웃");
    }

    void OnDestroy()
    {
        if (Auth != null)
            Auth.StateChanged -= OnAuthStateChanged;
    }

    public bool IsLoggedIn() => CurrentUser != null;

    public void SignOut()
    {
        if (Auth == null) return;
        Auth.StateChanged -= OnAuthStateChanged;
        Auth.SignOut();
        CurrentUser = null;
        Debug.Log("[Firebase] 로그아웃 완료");
    }
}
