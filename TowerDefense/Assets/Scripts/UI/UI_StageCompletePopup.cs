using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 전체 웨이브 클리어 → 스테이지 클리어 팝업.
///
/// Unity 오브젝트 이름 규칙:
///   텍스트 : Text_Subtitle, Text_KillCount, Text_Gold, Text_Time
///   버튼   : Button_NextStage, Button_MainMenu
///   이미지 : Image_Trophy
/// </summary>
public class UI_StageCompletePopup : UI_Base
{
    enum Texts { Text_Subtitle, Text_KillCount, Text_Gold, Text_Time }
    enum Buttons { Button_NextStage, Button_MainMenu }
    enum Images { Image_Trophy }

    private bool _initialized;
    private RectTransform _rect;
    private Tween _trophyTween;

    void OnEnable() => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();
    void OnDestroy() => _trophyTween?.Kill();

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

        Managers.GameM.StopTimer();
        Managers.SaveM?.OnStageClear(stage);

        int totalWaves = Managers.WaveM.TotalWaves;
        GetText(typeof(Texts), (int)Texts.Text_Subtitle).text = $"모든 {totalWaves}웨이브 클리어!";
        GetText(typeof(Texts), (int)Texts.Text_KillCount).text = Managers.GameM.KillCount.ToString("N0");
        GetText(typeof(Texts), (int)Texts.Text_Gold).text = Managers.GameM.Gold.ToString("N0");
        GetText(typeof(Texts), (int)Texts.Text_Time).text = FormatTime(Managers.GameM.ElapsedTime);

        GetButton(typeof(Buttons), (int)Buttons.Button_NextStage).gameObject.SetActive(stage < 4);

        _rect.localScale = Vector3.one * 0.7f;
        var trophy = GetImage(typeof(Images), (int)Images.Image_Trophy);
        trophy.color = new Color(trophy.color.r, trophy.color.g, trophy.color.b, 0f);

        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_rect.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        seq.Join(trophy.DOFade(1f, 0.25f));
        seq.OnComplete(() =>
        {
            var trophyRect = trophy.rectTransform;
            var startY = trophyRect.anchoredPosition.y;
            _trophyTween = trophyRect
                .DOAnchorPosY(startY + 12f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        });
    }

    private string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        return $"{m}:{s:D2}";
    }

    private void OnNextStageClicked()
    {
        int next = Managers.SelectedStage + 1;
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
