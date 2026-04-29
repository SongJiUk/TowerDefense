using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 일시정지 팝업.
/// 오브젝트 이름: Button_Resume, Button_Quit, Image_BG
/// </summary>
public class UI_PausePopup : UI_Base
{
    enum Buttons { Button_Resume, Button_Quit }
    enum Images  { Image_BG }

    private bool _initialized;

    void OnEnable()  => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;
        _initialized = true;

        BindButton(typeof(Buttons));
        BindImage(typeof(Images));

        GetButton(typeof(Buttons), (int)Buttons.Button_Resume).onClick.AddListener(OnResume);
        GetButton(typeof(Buttons), (int)Buttons.Button_Quit).onClick.AddListener(OnQuit);
        return true;
    }

    private void OnResume()
    {
        Managers.PoolM.Push(gameObject);
    }

    private void OnQuit()
    {
        Managers.GameM.Reset();
        Managers.CardM.Clear();
        Managers.Clear();
        SceneManager.LoadScene("MainMenu");
    }
}
