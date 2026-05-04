using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 씬 HUD — 골드 · 웨이브 표시.
/// 타워 선택 UI는 UI_TowerSelectPopup이 담당.
/// </summary>
public class UI_GameScene : UI_Scene
{
    enum Texts { Text_Gold, Text_Wave, Text_HP, Text_Level, Text_Exp, Text_SkillPoint, Text_WaveAnnounce }
    enum Buttons { Button_SkillUpgrade, Button_SkillCancel, Button_Speed, Button_Pause }
    enum Images
    {
        Image_Top, Image_Bottom, Image_LevelFillBG, Image_LevelFill, Image_TopGlow, Image_BottomGlow
    , Image_WaveSlotGlow, Image_WaveSlot, Image_WaveFillBG, Image_WaveFill, Top_Screen, Bottom_Screen, Image_SkillPointGlow, Image_SkillPointBG
    }
    enum GameObjects { Content_SkillHorizontal, Object_WaveAnnounce }

    Transform parent;
    private RectTransform _skillBtnRect;
    private Vector2 _skillBtnOriginPos;
    private float _displayHp = -1f;
    private bool _isDoubleSpeed = false;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    async void Start()
    {
        await GameSceneBootstrap.ReadyTask;
        await Init();
    }

    void OnDestroy()
    {
        Managers.GameM.OnGoldChanged -= RefreshGold;
        Managers.GameM.OnExpChanged -= RefreshExp;
        Managers.GameM.OnLevelUp -= LevelUp;
        Managers.WaveM.OnWaveStart -= OnWaveStart;
        Managers.WaveM.OnWaveComplete -= OnWaveComplete;
        Managers.WaveM.OnAllWavesComplete -= OnAllWavesComplete;
        Managers.GameM.OnGameOver -= OnGameOver;
        Managers.GameM.OnGameClear -= OnAllWavesComplete;
        Managers.SkillM.OnSlotChanged -= OnSkillSlotChanged;
        Managers.SkillM.OnSkillPointsChanged -= RefreshSkillPoints;
        Managers.SkillM.OnTargetingStarted -= OnTargetingStarted;
        Managers.SkillM.OnTargetingCancelled -= OnTargetingCancelled;
        if (Managers.ICore is Core coreForUnsub) coreForUnsub.OnHpChanged -= RefreshCoreHp;
        _skillBtnRect?.DOKill();
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindObject(typeof(GameObjects));

        parent = GetObject(typeof(GameObjects), (int)GameObjects.Content_SkillHorizontal).transform;

        var skillBtn = GetButton(typeof(Buttons), (int)Buttons.Button_SkillUpgrade);
        skillBtn.onClick.AddListener(OnSkillUpgradeClicked);
        _skillBtnRect = skillBtn.GetComponent<RectTransform>();
        _skillBtnOriginPos = _skillBtnRect.anchoredPosition;
        skillBtn.gameObject.SetActive(false);

        var cancelBtn = GetButton(typeof(Buttons), (int)Buttons.Button_SkillCancel);
        cancelBtn.onClick.AddListener(() => Managers.SkillM.CancelTargeting());
        cancelBtn.gameObject.SetActive(false);

        GetButton(typeof(Buttons), (int)Buttons.Button_Speed).onClick.AddListener(OnSpeedToggle);
        GetButton(typeof(Buttons), (int)Buttons.Button_Pause).onClick.AddListener(OnPauseClicked);

        Managers.SkillM.OnTargetingStarted += OnTargetingStarted;
        Managers.SkillM.OnTargetingCancelled += OnTargetingCancelled;

        GetObject(typeof(GameObjects), (int)GameObjects.Object_WaveAnnounce).SetActive(false);

        Managers.GameM.OnGoldChanged += RefreshGold;
        Managers.GameM.OnExpChanged += RefreshExp;
        Managers.GameM.OnLevelUp += LevelUp;
        Managers.WaveM.OnWaveStart += OnWaveStart;
        Managers.WaveM.OnWaveComplete += OnWaveComplete;
        Managers.WaveM.OnAllWavesComplete += OnAllWavesComplete;
        Managers.GameM.OnGameOver += OnGameOver;
        Managers.GameM.OnGameClear += OnAllWavesComplete;
        Managers.SkillM.OnSlotChanged += OnSkillSlotChanged;
        Managers.SkillM.OnSkillPointsChanged += RefreshSkillPoints;

        if (Managers.ICore is Core coreForSub)
            coreForSub.OnHpChanged += RefreshCoreHp;

        RefreshGold(Managers.GameM.Gold);
        RefreshWave(Managers.WaveM.CurrentWave);
        RefreshSkillPoints(Managers.SkillM.SkillPoints);
        if (Managers.ICore is Core core)
            RefreshCoreHp(core.CurrentHp);

        ApplyTheme(Managers.WaveM.CurrentStage);

        return true;
    }

    public override void ApplyTheme(StageData stage)
    {
        if (stage == null) return;
        GetImage(typeof(Images), (int)Images.Image_Top).color = stage.uiBGColor;
        GetImage(typeof(Images), (int)Images.Image_Bottom).color = stage.uiBGColor;

        GetImage(typeof(Images), (int)Images.Image_TopGlow).color = stage.uiBorderColor;
        GetImage(typeof(Images), (int)Images.Image_BottomGlow).color = stage.uiBorderColor;
        GetImage(typeof(Images), (int)Images.Image_WaveSlotGlow).color = stage.uiBorderColor;
        GetImage(typeof(Images), (int)Images.Image_LevelFillBG).color = stage.uiBorderColor;
        GetImage(typeof(Images), (int)Images.Image_WaveFillBG).color = stage.uiBorderColor;
        GetImage(typeof(Images), (int)Images.Top_Screen).color = stage.uiBorderColor;
        GetImage(typeof(Images), (int)Images.Bottom_Screen).color = stage.uiBorderColor;
        GetImage(typeof(Images), (int)Images.Image_SkillPointGlow).color = stage.uiBorderColor;
        GetText(typeof(Texts), (int)Texts.Text_Wave).color = stage.uiBorderColor;
        GetText(typeof(Texts), (int)Texts.Text_SkillPoint).color = stage.uiBorderColor;
        GetText(typeof(Texts), (int)Texts.Text_Level).color = stage.uiBorderColor;

        GetImage(typeof(Images), (int)Images.Image_WaveSlot).color = stage.uiTextColor;
        GetImage(typeof(Images), (int)Images.Image_LevelFill).color = stage.uiTextColor;
        GetImage(typeof(Images), (int)Images.Image_WaveFill).color = stage.uiTextColor;
        GetImage(typeof(Images), (int)Images.Image_SkillPointBG).color = stage.uiTextColor;
    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    private void RefreshGold(int gold)
    {
        GetText(typeof(Texts), (int)Texts.Text_Gold).text = $"{gold}";
    }

    private void RefreshWave(int wave)
    {
        GetText(typeof(Texts), (int)Texts.Text_Wave).text = $"{wave}";
        int total = Managers.WaveM.TotalWaves;
        if (total > 0)
            GetImage(typeof(Images), (int)Images.Image_WaveFill).fillAmount = (float)(wave - 1) / total;
    }

    private void RefreshCoreHp(float hp)
    {
        float maxHp = (Managers.ICore as Core)?.MaxHp ?? 1f;
        float normalized = Mathf.Clamp01(hp / maxHp) * 100f;

        if (_displayHp < 0f)
        {
            _displayHp = normalized;
            GetText(typeof(Texts), (int)Texts.Text_HP).text = $"{Mathf.RoundToInt(normalized)}";
            return;
        }
        var txt = GetText(typeof(Texts), (int)Texts.Text_HP);
        DG.Tweening.DOTween.To(
            () => _displayHp,
            x => { _displayHp = x; txt.text = $"{Mathf.RoundToInt(x)}"; },
            normalized, 0.4f
        ).SetEase(DG.Tweening.Ease.OutQuad).SetUpdate(true);
    }

    // ─── 웨이브 알림 ──────────────────────────────────────────────────────────

    private static readonly Color COLOR_WAVE_START = new Color(1.0f, 0.55f, 0.1f);
    private static readonly Color COLOR_WAVE_CLEAR = new Color(0.3f, 1.0f, 0.3f);

    private void OnWaveStart(int wave)
    {
        RefreshWave(wave);
        CountdownThenAnnounce(wave).Forget();
    }

    private async UniTaskVoid CountdownThenAnnounce(int wave)
    {
        await UniTask.WaitUntil(() => isInit, cancellationToken: destroyCancellationToken);

        var panel = GetObject(typeof(GameObjects), (int)GameObjects.Object_WaveAnnounce);
        var txt   = GetText(typeof(Texts), (int)Texts.Text_WaveAnnounce);
        int count = Mathf.FloorToInt(Managers.WaveM.CurrentStage?.waveStartDelay ?? 3f);

        txt.color = COLOR_WAVE_START;
        panel.SetActive(true);

        for (int i = count; i >= 1; i--)
        {
            txt.text = i.ToString();
            // DelayType.DeltaTime → timeScale 0이면 일시정지
            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.DeltaTime,
                cancellationToken: destroyCancellationToken);
        }

        txt.text = $"Wave {wave} 시작!";
        await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.DeltaTime,
            cancellationToken: destroyCancellationToken);

        panel.SetActive(false);
        Managers.WaveM.BeginSpawning();
    }

    private async UniTaskVoid ShowAnnounce(string message, Color textColor)
    {
        await UniTask.WaitUntil(() => isInit, cancellationToken: destroyCancellationToken);

        var panel = GetObject(typeof(GameObjects), (int)GameObjects.Object_WaveAnnounce);
        var txt   = GetText(typeof(Texts), (int)Texts.Text_WaveAnnounce);

        txt.text  = message;
        txt.color = textColor;
        panel.SetActive(true);

        await UniTask.Delay(2000, cancellationToken: destroyCancellationToken);

        panel.SetActive(false);
    }

    private void RefreshExp(int exp, int maxExp)
    {
        float amount = maxExp > 0 ? Mathf.Clamp01((float)exp / maxExp) : 0f;
        GetText(typeof(Texts), (int)Texts.Text_Exp).text = $"{(int)(amount * 100)}%";
        GetImage(typeof(Images), (int)Images.Image_LevelFill).fillAmount = amount;
    }

    private async void LevelUp(int level, int currentExp)
    {
        GetText(typeof(Texts), (int)Texts.Text_Level).text = level.ToString();
        // 바를 0으로 초기화한 뒤 새 레벨 진척도 설정
        GetImage(typeof(Images), (int)Images.Image_LevelFill).fillAmount = 0f;
        int required = Managers.GameM.LevelData.GetRequiredExp(level);
        float amount = required > 0 ? Mathf.Clamp01((float)currentExp / required) : 0f;
        GetText(typeof(Texts), (int)Texts.Text_Exp).text = $"{(int)(amount * 100)}%";
        GetImage(typeof(Images), (int)Images.Image_LevelFill).fillAmount = amount;

        var popup = Managers.ObjectM.SpawnUI<UI_LevelUpPopup>("UI_LevelUpPopup", transform);
        await popup.Init();
        popup.SetInfo().Forget();
    }

    private UI_SkillSlot[] _skillSlots = new UI_SkillSlot[3];

    private async void OnSkillSlotChanged(int index, SkillData skill)
    {
        if (skill != null && _skillSlots[index] == null)
        {
            UI_SkillSlot slot = Managers.ObjectM.SpawnUI<UI_SkillSlot>("UI_SkillSlot", parent);
            await slot.Init();
            slot.SetSlotIndex(index);
            _skillSlots[index] = slot;
        }
        else if (skill == null && _skillSlots[index] != null)
        {
            Managers.ObjectM.DespawnUI(_skillSlots[index].gameObject);
            _skillSlots[index] = null;
        }

        RefreshSkillPoints(Managers.SkillM.SkillPoints);
    }

    private void RefreshSkillPoints(int points)
    {
        var btn = GetButton(typeof(Buttons), (int)Buttons.Button_SkillUpgrade);
        GetText(typeof(Texts), (int)Texts.Text_SkillPoint).text = $"+{points}";

        bool hasUpgradable = false;
        for (int i = 0; i < 3; i++)
        {
            if (Managers.SkillM.GetSlot(i) != null && Managers.SkillM.CanUpgrade(i))
            {
                hasUpgradable = true;
                break;
            }
        }

        if (points <= 0 || !hasUpgradable)
        {
            btn.gameObject.SetActive(false);
            return;
        }

        _skillBtnRect.DOKill();

        if (!btn.gameObject.activeSelf)
        {
            btn.gameObject.SetActive(true);
            _skillBtnRect.anchoredPosition = _skillBtnOriginPos + Vector2.down * 80f;
            _skillBtnRect.DOAnchorPos(_skillBtnOriginPos, 0.45f).SetEase(Ease.OutBack);
        }
        else
        {
            _skillBtnRect.DOPunchScale(Vector3.one * 0.25f, 0.35f, 6, 0.5f);
        }
    }

    // ─── 버튼 핸들러 ─────────────────────────────────────────────────────────

    private void OnSkillUpgradeClicked()
    {
        Managers.UIM.ShowPopup<UI_SkillUpgradePopup>("UI_SkillUpgradePopup");
    }

    private void OnTargetingStarted(float range)
    {
        if (!isInit) return;
        GetButton(typeof(Buttons), (int)Buttons.Button_SkillCancel).gameObject.SetActive(true);
    }

    private void OnTargetingCancelled()
    {
        if (!isInit) return;
        GetButton(typeof(Buttons), (int)Buttons.Button_SkillCancel).gameObject.SetActive(false);
    }

    private void OnStartWaveClicked()
    {
        Managers.WaveM.StartNextWave();
    }

    private void OnSpeedToggle()
    {
        _isDoubleSpeed = !_isDoubleSpeed;
        Time.timeScale = _isDoubleSpeed ? 2f : 1f;
        var txt = GetButton(typeof(Buttons), (int)Buttons.Button_Speed).GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (txt != null) txt.text = _isDoubleSpeed ? "x2" : "x1";
    }

    private void OnPauseClicked()
    {
        var popup = Managers.ObjectM.SpawnUI<UI_PausePopup>("UI_PausePopup", transform);
        if (popup != null) _ = popup.Init();
    }

    private void OnWaveComplete(int wave, int bonus)
    {
        float mult = Managers.WaveM.LastWaveBonusMultiplier;
        string extra = mult > 1.01f ? $"\n<size=70%>클리어 보너스 +{(mult - 1f) * 100f:F0}%</size>" : "";
        ShowAnnounce($"Wave {wave} 클리어!\n<color=#FFD700>+{bonus}G</color>{extra}", COLOR_WAVE_CLEAR).Forget();
        int total = Managers.WaveM.TotalWaves;
        if (total > 0)
            GetImage(typeof(Images), (int)Images.Image_WaveFill).fillAmount = (float)wave / total;
    }

    private async void OnAllWavesComplete()
    {
        var popup = Managers.ObjectM.SpawnUI<UI_StageCompletePopup>("UI_StageCompletePopup", transform);
        await popup.Init();
        popup.Show(Managers.SelectedStage).Forget();
    }

    private async void OnGameOver()
    {
        var popup = Managers.ObjectM.SpawnUI<UI_GameOverPopup>("UI_GameOverPopup", transform);
        await popup.Init();
        popup.Show(Managers.WaveM.CurrentWave, Managers.WaveM.TotalWaves).Forget();
    }
}
