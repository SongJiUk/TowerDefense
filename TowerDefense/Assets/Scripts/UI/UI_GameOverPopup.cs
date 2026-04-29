using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 코어 HP 0 → 게임오버 팝업.
///
/// Unity 오브젝트 이름 규칙:
///   텍스트 : Text_Title, Text_WaveCount
///   버튼   : Button_Retry, Button_MainMenu
///   이미지 : Image_BG
/// </summary>
public class UI_GameOverPopup : UI_Base
{
    enum Texts { Text_Title, Text_WaveCount }
    enum Buttons { Button_Retry, Button_MainMenu }
    enum Images { Image_BG }

    private bool _initialized;
    private RectTransform _rect;

    void OnEnable() => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;
        _initialized = true;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));

        GetButton(typeof(Buttons), (int)Buttons.Button_Retry).onClick.AddListener(OnRetryClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_MainMenu).onClick.AddListener(OnMainMenuClicked);

        _rect = GetComponent<RectTransform>();
        return true;
    }

    public async UniTaskVoid Show(int wave)
    {
        if (!_initialized) await Init();

        Managers.SaveM?.OnGameOver();

        GetText(typeof(Texts), (int)Texts.Text_Title).text = "Game Over";
        GetText(typeof(Texts), (int)Texts.Text_WaveCount).text = $"Wave {wave} 도달";

        _rect.localScale = Vector3.one * 0.7f;
        var img = GetImage(typeof(Images), (int)Images.Image_BG);
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);

        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_rect.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        seq.Join(img.DOFade(1f, 0.25f));
    }

    private void OnRetryClicked()
    {
        Managers.GameM.Reset();
        Managers.CardM.Clear();
        Managers.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenuClicked()
    {
        Managers.GameM.Reset();
        Managers.CardM.Clear();
        Managers.Clear();
        SceneManager.LoadScene("MainMenu");
    }
}
