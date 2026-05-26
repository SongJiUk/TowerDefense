using Cysharp.Threading.Tasks;
using UnityEngine;

public class UI_Login : UI_Base
{
    enum GameObjects { Panel_NotLoggedIn, Panel_ProfileGuest, Panel_ProfileGoogle }
    enum Buttons
    {
        // 미로그인
        Button_Guest, Button_Google,
        // 게스트 프로필
        Button_GuestEdit, Button_GuestStart, Button_GuestLinkGoogle, Button_GuestSwitch,
        // 구글 프로필
        Button_GoogleEdit, Button_GoogleStart, Button_GoogleSwitch
    }
    enum Texts
    {
        Text_GuestNickname,
        Text_GoogleNickname, Text_GoogleEmail,
        Text_Version
    }

    public System.Action OnLoginSuccess;

    private bool _isLoggingIn;

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        // 미로그인 버튼
        GetButton(typeof(Buttons), (int)Buttons.Button_Guest).onClick.AddListener(OnClickGuest);
        GetButton(typeof(Buttons), (int)Buttons.Button_Google).onClick.AddListener(OnClickGoogle);

        // 게스트 프로필 버튼
        GetButton(typeof(Buttons), (int)Buttons.Button_GuestEdit).onClick.AddListener(OnClickEditNickname);
        GetButton(typeof(Buttons), (int)Buttons.Button_GuestStart).onClick.AddListener(OnClickStart);
        GetButton(typeof(Buttons), (int)Buttons.Button_GuestLinkGoogle).onClick.AddListener(OnClickLinkGoogle);
        GetButton(typeof(Buttons), (int)Buttons.Button_GuestSwitch).onClick.AddListener(OnClickSwitch);

        // 구글 프로필 버튼
        GetButton(typeof(Buttons), (int)Buttons.Button_GoogleEdit).onClick.AddListener(OnClickEditNickname);
        GetButton(typeof(Buttons), (int)Buttons.Button_GoogleStart).onClick.AddListener(OnClickStart);
        GetButton(typeof(Buttons), (int)Buttons.Button_GoogleSwitch).onClick.AddListener(OnClickSwitch);

        // 버전 텍스트
        GetText(typeof(Texts), (int)Texts.Text_Version).text =
            $"v{Application.version} · © {System.DateTime.Now.Year} Realm Guard";

        RefreshPanel();
        return true;
    }

    // ─── 패널 상태 ────────────────────────────────────────────────────────────

    private void RefreshPanel()
    {
        bool loggedIn = Managers.FirebaseM != null && Managers.FirebaseM.IsLoggedIn();
        bool isGuest  = loggedIn && Managers.FirebaseM.CurrentUser.IsAnonymous;

        GetObject(typeof(GameObjects), (int)GameObjects.Panel_NotLoggedIn).SetActive(!loggedIn);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_ProfileGuest).SetActive(loggedIn && isGuest);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_ProfileGoogle).SetActive(loggedIn && !isGuest);

        if (!loggedIn) return;

        var data = Managers.SaveM?.Data;
        string nickname = data?.PlayerName ?? "";

        if (isGuest)
        {
            GetText(typeof(Texts), (int)Texts.Text_GuestNickname).text = nickname;
        }
        else
        {
            GetText(typeof(Texts), (int)Texts.Text_GoogleNickname).text = nickname;
            GetText(typeof(Texts), (int)Texts.Text_GoogleEmail).text =
                Managers.FirebaseM.CurrentUser.Email ?? "";
        }
    }

    // ─── 미로그인 ─────────────────────────────────────────────────────────────

    private async void OnClickGuest()
    {
        if (_isLoggingIn) return;
        _isLoggingIn = true;

        bool ok = await Managers.FirebaseM.GuestLogin();
        if (ok)
        {
            await Managers.FirebaseM.ReadData();
            RefreshPanel();
        }

        _isLoggingIn = false;
    }

    private async void OnClickGoogle()
    {
        if (_isLoggingIn) return;
        _isLoggingIn = true;

        bool ok = await Managers.FirebaseM.GoogleLogin();
        if (ok)
        {
            await Managers.FirebaseM.ReadData();
            RefreshPanel();
        }

        _isLoggingIn = false;
    }

    // ─── 프로필 공통 ──────────────────────────────────────────────────────────

    private void OnClickStart()
    {
        OnLoginSuccess?.Invoke();
        Managers.ResourceM.Destroy(gameObject);
    }

    private void OnClickEditNickname()
    {
        var popup = Managers.ObjectM.SpawnUI<UI_NicknamePopup>("UI_NicknamePopup", transform.parent);
        if (popup == null) return;
        popup.OnComplete = () =>
        {
            RefreshPanel();
            UI_TitleScene.Instance?.RefreshPlayerName();
        };
        popup.Init().Forget();
    }

    private void OnClickSwitch()
    {
        Managers.FirebaseM.SignOut();
        RefreshPanel();
    }

    // ─── 게스트 전용 ─────────────────────────────────────────────────────────

    private async void OnClickLinkGoogle()
    {
        if (_isLoggingIn) return;
        _isLoggingIn = true;

        bool ok = await Managers.FirebaseM.LinkGoogleToGuest();
        if (ok) RefreshPanel();

        _isLoggingIn = false;
    }
}
