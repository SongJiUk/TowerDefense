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
    enum Buttons { Button_SkillUpgrade }
    enum Images
    {
        Image_Top, Image_Bottom, Image_LevelFillBG, Image_LevelFill, Image_TopGlow, Image_BottomGlow
    , Image_WaveSlotGlow, Image_WaveSlot, Image_WaveFillBG, Image_WaveFill, Top_Screen, Bottom_Screen, Image_SkillPointGlow, Image_SkillPointBG
    }
    enum GameObjects { Content_SkillHorizontal }


    Transform parent;
    private RectTransform _skillBtnRect;
    private Vector2 _skillBtnOriginPos;
    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    async void Start()
    {
        await Init();
    }

    void OnDestroy()
    {
        Managers.GameM.OnGoldChanged -= RefreshGold;
        Managers.GameM.OnExpChanged -= RefreshExp;
        Managers.GameM.OnLevelUp -= LevelUp;
        Managers.WaveM.OnWaveStart -= OnWaveStart;
        Managers.WaveM.OnAllWavesComplete -= OnAllWavesComplete;
        Managers.GameM.OnGameOver -= OnGameOver;
        Managers.SkillM.OnSlotChanged -= OnSkillSlotChanged;
        Managers.SkillM.OnSkillPointsChanged -= RefreshSkillPoints;
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

        //GetButton(typeof(Buttons), (int)Buttons.StartWave_Button).onClick.AddListener(OnStartWaveClicked);

        parent = GetObject(typeof(GameObjects), (int)GameObjects.Content_SkillHorizontal).transform;

        var skillBtn = GetButton(typeof(Buttons), (int)Buttons.Button_SkillUpgrade);
        skillBtn.onClick.AddListener(OnSkillUpgradeClicked);
        _skillBtnRect = skillBtn.GetComponent<RectTransform>();
        _skillBtnOriginPos = _skillBtnRect.anchoredPosition;
        skillBtn.gameObject.SetActive(false);

        Managers.GameM.OnGoldChanged += RefreshGold;
        Managers.GameM.OnExpChanged += RefreshExp;
        Managers.GameM.OnLevelUp += LevelUp;
        Managers.WaveM.OnWaveStart += OnWaveStart;
        Managers.WaveM.OnWaveComplete += OnWaveComplete;
        Managers.WaveM.OnAllWavesComplete += OnAllWavesComplete;
        Managers.GameM.OnGameOver += OnGameOver;
        Managers.SkillM.OnSlotChanged += OnSkillSlotChanged;
        Managers.SkillM.OnSkillPointsChanged += RefreshSkillPoints;

        RefreshGold(Managers.GameM.Gold);
        RefreshWave(Managers.WaveM.CurrentWave);
        RefreshSkillPoints(Managers.SkillM.SkillPoints);

        ApplyTheme(Managers.WaveM.CurrentStage);

        return true;
    }
    /// <summary>
    /// </summary>
    /// <param name="stage"></param> <summary>
    /// 
    /// </summary>
    /// <param name="stage"></param>
    public override void ApplyTheme(StageData stage)
    {
        if (stage == null) return;
        GetImage(typeof(Images), (int)Images.Image_Top).color = stage.uiBarBG;
        GetImage(typeof(Images), (int)Images.Image_Bottom).color = stage.uiBarBG;

        GetImage(typeof(Images), (int)Images.Image_TopGlow).color = stage.uiLineColor;
        GetImage(typeof(Images), (int)Images.Image_BottomGlow).color = stage.uiLineColor;
        GetImage(typeof(Images), (int)Images.Image_WaveSlotGlow).color = stage.uiLineColor;
        GetImage(typeof(Images), (int)Images.Image_LevelFillBG).color = stage.uiLineColor;
        GetImage(typeof(Images), (int)Images.Image_WaveFillBG).color = stage.uiLineColor;
        GetImage(typeof(Images), (int)Images.Top_Screen).color = stage.uiLineColor;
        GetImage(typeof(Images), (int)Images.Bottom_Screen).color = stage.uiLineColor;
        GetImage(typeof(Images), (int)Images.Image_SkillPointGlow).color = stage.uiLineColor;
        GetText(typeof(Texts), (int)Texts.Text_Wave).color = stage.uiLineColor;
        GetText(typeof(Texts), (int)Texts.Text_SkillPoint).color = stage.uiLineColor;
        GetText(typeof(Texts), (int)Texts.Text_Level).color = stage.uiLineColor;


        GetImage(typeof(Images), (int)Images.Image_WaveSlot).color = stage.uiAccentColor;
        GetImage(typeof(Images), (int)Images.Image_LevelFill).color = stage.uiAccentColor;
        GetImage(typeof(Images), (int)Images.Image_WaveFill).color = stage.uiAccentColor;
        GetImage(typeof(Images), (int)Images.Image_SkillPointBG).color = stage.uiAccentColor;

    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    private void RefreshGold(int gold)
    {
        GetText(typeof(Texts), (int)Texts.Text_Gold).text = $"{gold}";
    }

    private void OnWaveStart(int wave)
    {
        GetText(typeof(Texts), (int)Texts.Text_Wave).text = $"{wave}";
        ShowAnnounce($"Wave {wave} 시작!");
    }

    private void ShowAnnounce(string message)
    {
        var txt = GetText(typeof(Texts), (int)Texts.Text_WaveAnnounce);
        txt.text = message;
        txt.DOKill();
        txt.transform.DOKill();

        txt.alpha = 0f;
        txt.transform.localScale = Vector3.one * 0.8f;

        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(txt.DOFade(1f, 0.2f));
        seq.Join(txt.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        seq.AppendInterval(1.2f);
        seq.Append(txt.DOFade(0f, 0.3f));
    }

    private void RefreshExp(int exp, int maxExp)
    {
        float amount = (float)exp / maxExp;
        GetText(typeof(Texts), (int)Texts.Text_Exp).text = $"{(int)(amount * 100)}%";
        GetImage(typeof(Images), (int)Images.Image_LevelFill).fillAmount = amount;
    }

    private async void LevelUp(int level, int currentExp)
    {
        GetText(typeof(Texts), (int)Texts.Text_Level).text = level.ToString();
        int required = Managers.GameM.LevelData.GetRequiredExp(level);
        float amount = required > 0 ? (float)currentExp / required : 0f;
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
            // 0→1: 아래에서 올라오는 입장 애니메이션
            btn.gameObject.SetActive(true);
            _skillBtnRect.anchoredPosition = _skillBtnOriginPos + Vector2.down * 80f;
            _skillBtnRect.DOAnchorPos(_skillBtnOriginPos, 0.45f).SetEase(Ease.OutBack);
        }
        else
        {
            // 이미 표시 중: 포인트 증가 강조 (펀치 스케일)
            _skillBtnRect.DOPunchScale(Vector3.one * 0.25f, 0.35f, 6, 0.5f);
        }
    }

    // ─── 버튼 핸들러 ─────────────────────────────────────────────────────────

    private void OnSkillUpgradeClicked()
    {
        Managers.UIM.ShowPopup<UI_SkillUpgradePopup>("UI_SkillUpgradePopup");
    }

    private void OnStartWaveClicked()
    {
        Managers.WaveM.StartNextWave();
    }

    private void OnWaveComplete(int wave)
    {
        ShowAnnounce($"Wave {wave} 클리어!");
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
        popup.Show(Managers.WaveM.CurrentWave).Forget();
    }
}
