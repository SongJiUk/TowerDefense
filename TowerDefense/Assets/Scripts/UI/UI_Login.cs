using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 로그인 팝업 — 구글 로그인 / 게스트 버튼.
/// 로딩씬 또는 타이틀씬에서 Firebase 초기화 후 미로그인 시 열림.
/// </summary>
public class UI_Login : UI_Base
{
    enum Buttons { Button_Google, Button_Guest }

    public System.Action OnLoginSuccess;

    private bool _isLoggingIn;

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindButton(typeof(Buttons));
        GetButton(typeof(Buttons), (int)Buttons.Button_Google).onClick.AddListener(OnClickGoogle);
        GetButton(typeof(Buttons), (int)Buttons.Button_Guest).onClick.AddListener(OnClickGuest);
        return true;
    }

    private async void OnClickGoogle()
    {
        if (_isLoggingIn) return;
        _isLoggingIn = true;

        bool ok = await Managers.FirebaseM.GoogleLogin();
        if (ok)
        {
            await Managers.FirebaseM.ReadData();
            OnLoginSuccess?.Invoke();
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[UI_Login] 구글 로그인 실패");
        }

        _isLoggingIn = false;
    }

    private async void OnClickGuest()
    {
        if (_isLoggingIn) return;
        _isLoggingIn = true;

        bool ok = await Managers.FirebaseM.GuestLogin();
        if (ok)
        {
            await Managers.FirebaseM.ReadData();
            OnLoginSuccess?.Invoke();
            gameObject.SetActive(false);
        }

        _isLoggingIn = false;
    }
}
