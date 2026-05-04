using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀씬 전체 제어.
/// 흐름: 인트로 애니메이션 → 로딩(백그라운드) → 탭투스타트 버튼 활성화 → 클릭 → 메인메뉴 슬라이드인
///
/// Unity 오브젝트 이름 규칙:
///   텍스트  : Text_TapToStart, Text_LogoRealm, Text_LogoGuard, Text_BestRecord
///   버튼    : Button_TapToStart, Button_Start, Button_Difficulty, Button_Achievement, Button_Settings, Button_Quit
///   오브젝트: Panel_Logo, Panel_Title, Panel_Menu, TextObject
///   이미지  : Image_Crown
/// </summary>
public class UI_TitleScene : UI_Base
{
    enum Texts { Text_TapToStart, Text_LogoRealm, Text_LogoGuard, Text_Subtitle, Text_BestRecord }
    enum Buttons { Button_TapToStart, Button_Start, Button_Difficulty, Button_Achievement, Button_Settings, Button_Quit }
    enum GameObjects { Panel_Logo, Panel_Title, Panel_Menu, TextObject, Panel_Record }
    enum Images { Image_Crown }

    private RectTransform _logoRect;
    private RectTransform _menuRect;
    private Vector2 _logoOriginPos;
    private Vector2 _menuOriginPos;

    private Tween _tapBlink;
    private Tween _crownTween;
    private Tween _swayTween;
    private Tween _textGlowTween;

    private bool _menuOpen    = false;
    private bool _introComplete;
    private bool _loadComplete;

    async void Start()
    {
        await Init();
        StartLoadAsync().Forget();
    }

    void OnDestroy()
    {
        _tapBlink?.Kill();
        _crownTween?.Kill();
        _swayTween?.Kill();
        _textGlowTween?.Kill();
    }

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));

        // 탭투스타트 버튼 — 로딩 완료 전까지 비활성
        var tapBtn = GetButton(typeof(Buttons), (int)Buttons.Button_TapToStart);
        tapBtn.onClick.AddListener(OpenMenu);
        tapBtn.gameObject.SetActive(false);

        // 메뉴 버튼
        GetButton(typeof(Buttons), (int)Buttons.Button_Start).onClick.AddListener(OnStartClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Difficulty).onClick.AddListener(OnDifficultyClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Achievement).onClick.AddListener(OnAchievementClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Settings).onClick.AddListener(OnSettingsClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Quit).onClick.AddListener(OnQuitClicked);

        _logoRect = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Logo).GetComponent<RectTransform>();
        _menuRect = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Menu).GetComponent<RectTransform>();
        _logoOriginPos = _logoRect.anchoredPosition;
        _menuOriginPos = _menuRect.anchoredPosition;

        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Menu).SetActive(false);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Record).SetActive(false);
        GetText(typeof(Texts), (int)Texts.Text_TapToStart).gameObject.SetActive(false);

        // 인트로 초기 상태 — 모두 숨김
        GetObject(typeof(GameObjects), (int)GameObjects.TextObject).transform.localScale = Vector3.zero;

        var crown = GetImage(typeof(Images), (int)Images.Image_Crown);
        crown.color = new Color(2.5f, 2.0f, 0.5f, 0f);

        var subtitle = GetText(typeof(Texts), (int)Texts.Text_Subtitle);
        subtitle.color = new Color(subtitle.color.r, subtitle.color.g, subtitle.color.b, 0f);

        // REALM 글로우 · 왕관 HDR — 처음부터 설정해두고 알파로만 숨김
        var realmText = GetText(typeof(Texts), (int)Texts.Text_LogoRealm);
        realmText.color = new Color(3f, 2.5f, 0.5f, 1f);
        var mat = realmText.fontMaterial;
        if (mat.HasProperty(ShaderUtilities.ID_GlowPower))
            mat.SetFloat(ShaderUtilities.ID_GlowPower, 0.3f);

        PlayIntroAsync().Forget();
        return true;
    }

    // ─── 인트로 시퀀스 ────────────────────────────────────────────────────────

    private async UniTaskVoid PlayIntroAsync()
    {
        await UniTask.Delay(300, cancellationToken: destroyCancellationToken);

        // 1. REALM GUARD 텍스트 스케일 0 → 1
        var textObj = GetObject(typeof(GameObjects), (int)GameObjects.TextObject);
        textObj.transform.DOScale(1f, 0.55f).SetEase(Ease.OutBack);
        await UniTask.Delay(700, cancellationToken: destroyCancellationToken);

        // 2. 왕관 페이드인
        var crown = GetImage(typeof(Images), (int)Images.Image_Crown);
        crown.DOFade(1f, 0.4f);
        await UniTask.Delay(550, cancellationToken: destroyCancellationToken);

        StartCrownLoop(crown.rectTransform);
        StartTextGlowLoop();

        // 3. Text_Subtitle 페이드인
        await UniTask.Delay(200, cancellationToken: destroyCancellationToken);
        GetText(typeof(Texts), (int)Texts.Text_Subtitle).DOFade(1f, 0.4f).SetEase(Ease.OutQuad);

        // 4. 화면을 터치하여 시작 — 인트로 끝나면 텍스트 표시
        await UniTask.Delay(600, cancellationToken: destroyCancellationToken);
        var tapText = GetText(typeof(Texts), (int)Texts.Text_TapToStart);
        tapText.gameObject.SetActive(true);
        tapText.color = new Color(tapText.color.r, tapText.color.g, tapText.color.b, 1f);
        _tapBlink = tapText.DOFade(0f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        _introComplete = true;
        TryActivateTapButton();
    }

    // ─── 왕관 루프 ───────────────────────────────────────────────────────────

    private void StartCrownLoop(RectTransform crownRect)
    {
        float startY = crownRect.anchoredPosition.y;
        _crownTween = crownRect
            .DOAnchorPosY(startY + 10f, 1.0f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        _swayTween = crownRect
            .DOLocalRotate(new Vector3(0f, 0f, 6f), 1.4f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ─── TMP 글로우 + Bloom 루프 ─────────────────────────────────────────────

    private void StartTextGlowLoop()
    {
        var mat = GetText(typeof(Texts), (int)Texts.Text_LogoRealm).fontMaterial;
        if (!mat.HasProperty(ShaderUtilities.ID_GlowPower)) return;

        // 색상·초기 GlowPower는 Init에서 이미 설정 — 여기서는 진동만
        _textGlowTween = DOTween
            .To(() => mat.GetFloat(ShaderUtilities.ID_GlowPower),
                v  => mat.SetFloat(ShaderUtilities.ID_GlowPower, v),
                0.5f, 1.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ─── 로딩 ─────────────────────────────────────────────────────────────────

    private async UniTaskVoid StartLoadAsync()
    {
        Debug.Log("[TitleScene] 로딩 시작");
        try
        {
            await Managers.ResourceM.LoadGroupAsync<Object>("PrevLoad", (key, cur, total) =>
                Debug.Log($"[TitleScene] {cur}/{total} : {key}")
            );
            Debug.Log("[TitleScene] PrevLoad 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TitleScene] 로딩 실패: {e.Message}");
        }

        Managers.GameM.LevelData = Managers.ResourceM.Load<LevelData>("LevelData");
        Debug.Log($"[TitleScene] LevelData: {(Managers.GameM.LevelData != null ? "OK" : "NULL")}");

        Managers.CardM.Init();
        Managers.SaveM.ApplyToGame();

        Debug.Log("[TitleScene] 로딩 완료 → 탭투스타트 활성화");
        OnLoadComplete();
    }

    private void OnLoadComplete()
    {
        var bestText = GetText(typeof(Texts), (int)Texts.Text_BestRecord);
        var data = Managers.SaveM.Data;
        bestText.text = data.BestWave > 0
            ? $"최고기록 : Lv {data.Level}, 스테이지 {data.BestStage} - {data.BestWave}"
            : "최고기록 : -";

        _loadComplete = true;
        TryActivateTapButton();
    }

    private void TryActivateTapButton()
    {
        if (!_introComplete || !_loadComplete) return;
        GetButton(typeof(Buttons), (int)Buttons.Button_TapToStart).gameObject.SetActive(true);
    }

    // ─── 메뉴 전환 ────────────────────────────────────────────────────────────

    private void OpenMenu()
    {
        if (_menuOpen) return;
        _menuOpen = true;
        _tapBlink?.Kill();

        GetButton(typeof(Buttons), (int)Buttons.Button_TapToStart).gameObject.SetActive(false);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Title).SetActive(false);

        // 로고 왼쪽으로
        _logoRect.DOAnchorPosX(_logoOriginPos.x - 350f, 0.4f).SetEase(Ease.OutCubic);

        // 메뉴 오른쪽에서 슬라이드인
        var menuGo = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Menu);
        menuGo.SetActive(true);
        _menuRect.anchoredPosition = new Vector2(_menuOriginPos.x + 800f, _menuOriginPos.y);
        _menuRect.DOAnchorPosX(_menuOriginPos.x, 0.4f).SetEase(Ease.OutCubic);

        // Panel_Record 퍼지며 등장 (메뉴 슬라이드 끝난 후)
        var record = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Record);
        record.transform.localScale = Vector3.zero;
        record.SetActive(true);
        record.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(0.35f);
    }

    // ─── 버튼 ─────────────────────────────────────────────────────────────────

    private void OnStartClicked()
    {
        var best = (Define.Difficulty)Managers.DifficultyM.MaxUnlocked;
        Managers.DifficultyM.Select(best);
        Managers.SelectedStage = 1;
        Managers.GameM.Reset();
        Managers.CardM.Clear();
        Managers.Clear();
        SceneManager.LoadScene("GameScene");
    }

    private void OnDifficultyClicked() => Managers.UIM.ShowPopup<UI_DifficultySelectPopup>("UI_DifficultySelectPopup");
    private void OnAchievementClicked() => Managers.UIM.ShowPopup<UI_AchievementPopup>("UI_AchievementPopup");
    private void OnSettingsClicked() => Managers.UIM.ShowPopup<UI_SettingsPopup>("UI_SettingsPopup");
    private void OnQuitClicked() => Application.Quit();
}
