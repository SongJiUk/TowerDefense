using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 전체 웨이브 클리어 → 스테이지 클리어 팝업.
///
/// Unity 오브젝트 이름 규칙:
///   텍스트 : Text_Title, Text_StageNum
///   버튼   : Button_NextStage, Button_MainMenu
///   이미지 : Image_BG
/// </summary>
public class UI_StageCompletePopup : UI_Base
{
    enum Texts { Text_Title, Text_StageNum }
    enum Buttons { Button_NextStage, Button_MainMenu }
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

        GetButton(typeof(Buttons), (int)Buttons.Button_NextStage).onClick.AddListener(OnNextStageClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_MainMenu).onClick.AddListener(OnMainMenuClicked);

        _rect = GetComponent<RectTransform>();
        return true;
    }

    public async UniTaskVoid Show(int stage)
    {
        if (!_initialized) await Init();

        GetText(typeof(Texts), (int)Texts.Text_Title).text = "Stage Clear!";
        GetText(typeof(Texts), (int)Texts.Text_StageNum).text = $"Stage {stage} 클리어";

        bool hasNextStage = stage < 4;
        GetButton(typeof(Buttons), (int)Buttons.Button_NextStage).gameObject.SetActive(hasNextStage);

        _rect.localScale = Vector3.one * 0.7f;
        var img = GetImage(typeof(Images), (int)Images.Image_BG);
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);

        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_rect.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        seq.Join(img.DOFade(1f, 0.25f));
    }

    private void OnNextStageClicked()
    {
        int next = Managers.SelectedStage + 1;
        if (next > 4) next = 4;
        Managers.SelectedStage = next;
        Managers.GameM.ResetGold();
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
