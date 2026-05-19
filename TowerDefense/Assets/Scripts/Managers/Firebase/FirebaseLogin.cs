using Firebase.Auth;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Google;
using Firebase;

public partial class FirebaseManager
{
    // ★ Firebase 콘솔 → 프로젝트 설정 → 내 앱(Android) → OAuth 2.0 웹 클라이언트 ID
    private const string WEB_CLIENT_ID = "839794068027-dfje6ht82mcjbtdj9np14thekccplcub.apps.googleusercontent.com";

    private GoogleSignInConfiguration _googleConfig;

    public void SetupGoogleConfig()
    {
        if (_googleConfig != null) return;
        _googleConfig = new GoogleSignInConfiguration
        {
            WebClientId = WEB_CLIENT_ID,
            RequestIdToken = true,
            RequestEmail = true
        };
        GoogleSignIn.Configuration = _googleConfig;
    }

    // ─── 구글 로그인 ──────────────────────────────────────────────────────────

    public async UniTask<bool> GoogleLogin()
    {
#if UNITY_EDITOR
        Debug.LogWarning("[Firebase] 구글 로그인은 에디터에서 동작하지 않습니다. 디바이스에서 테스트하세요.");
        return false;
#elif UNITY_ANDROID || UNITY_IOS
        try
        {
            GoogleSignIn.DefaultInstance.SignOut();
            GoogleSignIn.DefaultInstance.Disconnect();

            GoogleSignIn.Configuration = _googleConfig;
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

            if (googleUser == null || string.IsNullOrEmpty(googleUser.IdToken))
            {
                Debug.LogError("[Firebase] Google SignIn 실패: 토큰 없음");
                return false;
            }

            var credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            var result      = await Auth.SignInWithCredentialAsync(credential);

            CurrentUser = result;
            ApplyUserToSaveData(CurrentUser);

            PlayerPrefs.SetInt("HasSeenLogin", 1);
            PlayerPrefs.Save();

            Debug.Log($"[Firebase] 구글 로그인 성공: {result.UserId}");
            return true;
        }
        catch (Google.GoogleSignIn.SignInException e)
        {
            Debug.LogError($"[Firebase] Google SignIn Error: {e.Status} / {e.Message}");
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Firebase] 예외: {e.Message}");
            return false;
        }
#endif
    }

    // ─── 게스트 로그인 ────────────────────────────────────────────────────────

    public async UniTask<bool> GuestLogin()
    {
        if (Auth == null) return false;
        try
        {
            var user = await Auth.SignInAnonymouslyAsync();
            if (user == null) return false;

            CurrentUser = user;
            ApplyUserToSaveData(CurrentUser);

            PlayerPrefs.SetInt("HasSeenLogin", 1);
            PlayerPrefs.Save();

            Debug.Log($"[Firebase] 게스트 로그인 성공: {user.UserId}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Firebase] 게스트 로그인 실패: {e.Message}");
            return false;
        }
    }

    // ─── 구글 계정 연동 (게스트 → 구글) ──────────────────────────────────────

    public async UniTask<bool> LinkGoogleToGuest()
    {
        if (Auth.CurrentUser == null || !Auth.CurrentUser.IsAnonymous)
        {
            Debug.LogWarning("[Firebase] 게스트 계정이 아닙니다. 연동 불가.");
            return false;
        }

        try
        {
            var gUser = await GoogleSignIn.DefaultInstance.SignIn();
            if (gUser == null || string.IsNullOrEmpty(gUser.IdToken)) return false;

            var credential = GoogleAuthProvider.GetCredential(gUser.IdToken, null);
            var result = await Auth.CurrentUser.LinkWithCredentialAsync(credential);

            CurrentUser = result;
            ApplyUserToSaveData(CurrentUser);

            Debug.Log("[Firebase] 구글 계정 연동 성공");
            return true;
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"[Firebase] 연동 실패: {(AuthError)e.ErrorCode} / {e.Message}");
            throw;
        }
    }

    // ─── 유틸 ────────────────────────────────────────────────────────────────

    private void ApplyUserToSaveData(FirebaseUser user)
    {
        if (user == null) return;

        var data = Managers.SaveM?.Data;
        if (data == null) return;

        data.IsGuest = user.IsAnonymous;

        if (user.IsAnonymous)
        {
            // 기존 닉네임이 기본값이면 랜덤 생성, 아니면 유지
            if (string.IsNullOrEmpty(data.PlayerName) || data.PlayerName == "용사의 탑")
                data.PlayerName = NicknameGenerator.Generate();
        }
        else
        {
            data.PlayerName = !string.IsNullOrEmpty(user.DisplayName) ? user.DisplayName
                            : !string.IsNullOrEmpty(user.Email)       ? user.Email
                            : data.PlayerName;
        }
    }
}
