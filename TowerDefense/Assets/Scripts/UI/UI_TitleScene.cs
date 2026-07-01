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
///   텍스트  : Text_TapToStart, Text_LogoRealm, Text_BestRecord
///   버튼    : Button_TapToStart, Button_Start, Button_Difficulty, Button_Achievement, Button_Settings, Button_Quit
///   오브젝트: Panel_Logo, Panel_Title, Panel_Menu, TextObject
///   이미지  : Image_Crown
/// </summary>
public class UI_TitleScene : UI_Base
{
    enum Texts { Text_TapToStart, Text_LogoRealm, Text_Subtitle, Text_BestRecord, Text_PlayerName }
    enum Buttons { Button_TapToStart, Button_Start, Button_Difficulty, Button_Achievement, Button_Settings, Button_Quit, Button_SwitchAccount }
    enum GameObjects { Panel_Logo, Panel_Title, Panel_Menu, TextObject, Panel_Record, Panel_Fade }
    enum Images { Image_Crown }

    public static UI_TitleScene Instance { get; private set; }

    /// <summary>게임씬에서 복귀 시 true로 설정 — 인트로를 건너뛰고 메뉴로 직행.</summary>
    public static bool ReturnFromGame;

    private RectTransform _logoRect;
    private RectTransform _menuRect;
    private Vector2 _logoOriginPos;
    private Vector2 _menuOriginPos;
    private Vector2 _menuOriginLocalPos;

    private Tween _tapBlink;
    private Tween _crownTween;
    private Tween _swayTween;
    private Tween _textGlowTween;

    private bool _menuOpen = false;
    private bool _introComplete;
    private bool _loadComplete;

    async void Start()
    {
        Instance = this;
        await Init();
        StartLoadAsync().Forget();

        // 게임에서 복귀 시 로드 완료까지 블랙 유지, 끝나면 한 번에 페이드인
        if (ReturnFromGame)
            await UniTask.WaitUntil(() => _loadComplete, cancellationToken: destroyCancellationToken);

        await SceneFader.FadeIn();
    }

    void OnDestroy()
    {
        Instance = null;
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
        tapBtn.onClick.AddListener(OnTapToStart);
        tapBtn.gameObject.SetActive(false);

        // 메뉴 버튼
        GetButton(typeof(Buttons), (int)Buttons.Button_Start).onClick.AddListener(OnStartClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Difficulty).onClick.AddListener(OnDifficultyClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Achievement).onClick.AddListener(OnAchievementClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Settings).onClick.AddListener(OnSettingsClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Quit).onClick.AddListener(OnQuitClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_SwitchAccount).onClick.AddListener(OnSwitchAccountClicked);

        _logoRect = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Logo).GetComponent<RectTransform>();
        _menuRect = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Menu).GetComponent<RectTransform>();
        _logoOriginPos = _logoRect.anchoredPosition;
        _menuOriginPos = _menuRect.anchoredPosition;
        _menuOriginLocalPos = _menuRect.localPosition;

        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Menu).SetActive(false);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Record).SetActive(false);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Fade).SetActive(false);
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
        if (ReturnFromGame)
        {
            // Init에서 숨겨둔 로고 요소들을 애니메이션 없이 즉시 완성 상태로 복원
            GetObject(typeof(GameObjects), (int)GameObjects.TextObject).transform.localScale = Vector3.one;
            GetImage(typeof(Images), (int)Images.Image_Crown).color = new Color(2.5f, 2.0f, 0.5f, 1f);
            var sub = GetText(typeof(Texts), (int)Texts.Text_Subtitle);
            sub.color = new Color(sub.color.r, sub.color.g, sub.color.b, 1f);
            // 메뉴가 바로 열리므로 로고를 왼쪽 위치로 미리 이동
            _logoRect.anchoredPosition = new Vector2(_logoOriginPos.x - 350f, _logoOriginPos.y);

            _introComplete = true;
            TryActivateTapButton();
            return;
        }

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
                v => mat.SetFloat(ShaderUtilities.ID_GlowPower, v),
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
        Managers.AchievementM.Init(Managers.ResourceM.Load<AchievementDatabase>("AchievementDatabase"));
        Managers.CardM.Init();

        await InitFirebase();

        Managers.SaveM.ApplyToGame();

        // 스테이지 결정 + WaveM 초기화 — 로딩 완료 시점에 모두 끝냄
        PrepareStage();

        Debug.Log("[TitleScene] 로딩 완료");
        OnLoadComplete();
    }

    private async UniTask InitFirebase()
    {
        await Managers.FirebaseM.Init();

        if (Managers.FirebaseM.IsLoggedIn())
            await Managers.FirebaseM.ReadData();
    }

    private void OnLoadComplete()
    {
        var data = Managers.SaveM.Data;

        GetText(typeof(Texts), (int)Texts.Text_BestRecord).text = data.BestWave > 0
            ? $"최고기록 : 스테이지 {data.BestStage} - 웨이브 {data.BestWave}"
            : "최고기록 : -";

        GetText(typeof(Texts), (int)Texts.Text_PlayerName).text = data.PlayerName;

        _loadComplete = true;
        Managers.SoundM?.PlayBGM("BGM_Title");
        TryActivateTapButton();
    }

    public void RefreshPlayerName()
    {
        GetText(typeof(Texts), (int)Texts.Text_PlayerName).text = Managers.SaveM.Data.PlayerName;
    }

    private void TryActivateTapButton()
    {
        if (!_introComplete || !_loadComplete) return;

        if (ReturnFromGame)
        {
            ReturnFromGame = false;
            _menuOpen = true;
            GetObject(typeof(GameObjects), (int)GameObjects.Panel_Title).SetActive(false);

            var menuGo = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Menu);
            menuGo.SetActive(true);
            _menuRect.anchoredPosition = _menuOriginPos;

            var record = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Record);
            record.SetActive(true);
            record.transform.localScale = Vector3.one;
            return;
        }

        GetButton(typeof(Buttons), (int)Buttons.Button_TapToStart).gameObject.SetActive(true);
    }

    // ─── 메뉴 전환 ────────────────────────────────────────────────────────────

    private void SlideInMenuPanel()
    {
        var menuGo = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Menu);
        menuGo.SetActive(true);
        _menuRect.anchoredPosition = new Vector2(_menuOriginPos.x + 800f, _menuOriginPos.y);
        _menuRect.DOAnchorPosX(_menuOriginPos.x, 0.4f).SetEase(Ease.OutCubic).SetUpdate(true);

        var record = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Record);
        record.transform.localScale = Vector3.zero;
        record.SetActive(true);
        record.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(0.35f).SetUpdate(true);
    }

    private void SlideInLoginPanel()
    {
        var loginPanel = Managers.ObjectM.SpawnUI<UI_Login>("UI_Login", transform);
        if (loginPanel == null) return;

        loginPanel.OnLoginSuccess = () =>
        {
            RefreshPlayerName();
            SlideInMenuPanel();
        };
        loginPanel.Init().Forget();

        SlideInLoginPanelAsync(loginPanel.GetComponent<RectTransform>()).Forget();
    }

    private async UniTaskVoid SlideInLoginPanelAsync(RectTransform loginRect)
    {
        loginRect.localPosition = new Vector3(1450f, _menuOriginLocalPos.y, 0f);
        await UniTask.NextFrame(cancellationToken: destroyCancellationToken);
        loginRect.DOLocalMoveX(250f, 0.4f).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    // ─── 버튼 ─────────────────────────────────────────────────────────────────

    private void PrepareStage()
    {
        var data = Managers.SaveM.Data;

        bool allCleared = data.IsStageCleared(1) && data.IsStageCleared(2)
                       && data.IsStageCleared(3) && data.IsStageCleared(4);

        if (allCleared)
        {
            data.ResetStageFlags();
            Managers.SaveM.SaveCurrent();
            int maxIndex = System.Enum.GetValues(typeof(Define.Difficulty)).Length - 1;
            var next = (Define.Difficulty)Mathf.Min((int)Managers.DifficultyM.Selected + 1, maxIndex);
            Managers.DifficultyM.Select(next);
            Managers.SelectedStage = 1;
        }
        else
        {
            int nextStage = 1;
            for (int s = 1; s <= 4; s++)
            {
                if (!data.IsStageCleared(s)) { nextStage = s; break; }
            }
            Managers.SelectedStage = nextStage;
        }

        string stageKey = $"Stage{Managers.SelectedStage}Data";
        StageData stageData = Managers.ResourceM.Load<StageData>(stageKey);
        if (stageData != null)
            Managers.WaveM.Init(stageData);
        else
            Debug.LogError($"[TitleScene] StageData 로드 실패: '{stageKey}'");
    }

    private void OnStartClicked()
    {
        Managers.IsTestMode = false;
        FadeAndLoad("GameScene").Forget();
    }

    public async UniTaskVoid FadeAndLoad(string scene)
    {
        var fadeGo = GetObject(typeof(GameObjects), (int)GameObjects.Panel_Fade);
        fadeGo.SetActive(true);
        var group = fadeGo.GetComponent<CanvasGroup>();
        if (group == null) group = fadeGo.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.DOFade(1f, 0.4f).SetEase(Ease.InQuad).SetUpdate(true);
        await UniTask.Delay(400, ignoreTimeScale: true, cancellationToken: destroyCancellationToken);
        Managers.GameM.Reset();
        Managers.CardM.Clear();
        Managers.PoolM.Clear();
        Managers.UIM.Clear();
        Managers.SynergyM.Clear();
        SceneManager.LoadScene(scene);
    }

    private void OnDifficultyClicked() => Managers.UIM.ShowPopup<UI_DifficultySelectPopup>("UI_DifficultySelectPopup");
    private void OnAchievementClicked() => Managers.UIM.ShowPopup<UI_AchievementPanel>("UI_AchievementPanel");
    private void OnSettingsClicked()    => Managers.UIM.ShowPopup<UI_SettingsPopup>("UI_SettingsPopup");
    private void OnQuitClicked()        => Application.Quit();

    private void OnTapToStart()
    {
        if (_menuOpen) return;
        _menuOpen = true;
        _tapBlink?.Kill();

        GetButton(typeof(Buttons), (int)Buttons.Button_TapToStart).gameObject.SetActive(false);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Title).SetActive(false);
        _logoRect.DOAnchorPosX(_logoOriginPos.x - 350f, 0.4f).SetEase(Ease.OutCubic);

        if (Managers.FirebaseM != null && Managers.FirebaseM.IsLoggedIn())
            SlideInMenuPanel();
        else
            SlideInLoginPanel();
    }

    private void OnSwitchAccountClicked()
    {
        Managers.FirebaseM.SignOut();
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Menu).SetActive(false);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Record).SetActive(false);
        _menuOpen = false;
        SlideInLoginPanel();
    }
}
