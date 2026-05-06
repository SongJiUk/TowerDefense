using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 팝업 — 소리 / 게임 / 화면 탭.
/// ※ Slider_BGM, Slider_SFX 이름 정확히 맞출 것.
/// ※ 각 Toggle에 UI_ToggleSwitch 컴포넌트 추가 + _track/_handle 연결.
/// </summary>
public class UI_SettingsPopup : UI_Base
{
    enum Buttons
    {
        Button_Close,
        Button_Tab_Sound, Button_Tab_Game, Button_Tab_Screen,
        Button_Korean, Button_English,
        Button_Low, Button_Middle, Button_High,
        Button_Cancel, Button_Save
    }
    enum Sliders     { Slider_BGM, Slider_SFX }
    enum Toggles     { Toggle_Vibration, Toggle_Damage, Toggle_FPS, Toggle_Screen_Shake, Toggle_Particle }
    enum Texts       { Text_BGM, Text_SFX }
    enum GameObjects
    {
        Panel_Sound, Panel_Game, Panel_Screen,
        Object_Sound_Disable, Object_Game_Disable, Object_Screen_Disable
    }

    // 팝업 열릴 때마다 저장되는 초기값 (Cancel 롤백용)
    private float  _initBGM, _initSFX;
    private int    _initVibration, _initDamage, _initFPS, _initScreenShake, _initParticle;
    private string _initLanguage;
    private int    _initQuality;

    private bool _bound;

    void OnEnable()  => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();

    public override async UniTask<bool> Init()
    {
        if (!_bound)
        {
            if (!await base.Init()) return false;
            _bound = true;

            BindButton(typeof(Buttons));
            BindSlider(typeof(Sliders));
            BindToggle(typeof(Toggles));
            BindText(typeof(Texts));
            BindObject(typeof(GameObjects));

            SetupListeners();
        }

        LoadInitialValues();
        RefreshUI();
        SwitchTab(0);
        return true;
    }

    // ─── 최초 1회: 리스너 등록 ────────────────────────────────────────────────

    private void SetupListeners()
    {
        var bgmTxt = GetText(typeof(Texts), (int)Texts.Text_BGM);
        var sfxTxt = GetText(typeof(Texts), (int)Texts.Text_SFX);

        GetSlider(typeof(Sliders), (int)Sliders.Slider_BGM).onValueChanged.AddListener(v =>
        {
            Managers.SoundM.SetBgmVolume(v);
            bgmTxt.text = Mathf.RoundToInt(v * 100).ToString();
        });
        GetSlider(typeof(Sliders), (int)Sliders.Slider_SFX).onValueChanged.AddListener(v =>
        {
            Managers.SoundM.SetSfxVolume(v);
            sfxTxt.text = Mathf.RoundToInt(v * 100).ToString();
        });

        SetupToggle(Toggles.Toggle_Vibration,   "Vibration");
        SetupToggle(Toggles.Toggle_Damage,       "DamageText");
        SetupToggle(Toggles.Toggle_FPS,          "FPSDisplay");
        SetupToggle(Toggles.Toggle_Screen_Shake, "ScreenShake");
        SetupToggle(Toggles.Toggle_Particle,     "Particle");

        GetButton(typeof(Buttons), (int)Buttons.Button_Korean) .onClick.AddListener(() => { PlayerPrefs.SetString("Language", "ko"); RefreshLanguageButtons(); });
        GetButton(typeof(Buttons), (int)Buttons.Button_English).onClick.AddListener(() => { PlayerPrefs.SetString("Language", "en"); RefreshLanguageButtons(); });
        GetButton(typeof(Buttons), (int)Buttons.Button_Low)   .onClick.AddListener(() => { SetQuality(0); RefreshQualityButtons(); });
        GetButton(typeof(Buttons), (int)Buttons.Button_Middle).onClick.AddListener(() => { SetQuality(1); RefreshQualityButtons(); });
        GetButton(typeof(Buttons), (int)Buttons.Button_High)  .onClick.AddListener(() => { SetQuality(2); RefreshQualityButtons(); });

        GetButton(typeof(Buttons), (int)Buttons.Button_Close)     .onClick.AddListener(OnCancel);
        GetButton(typeof(Buttons), (int)Buttons.Button_Tab_Sound) .onClick.AddListener(() => SwitchTab(0));
        GetButton(typeof(Buttons), (int)Buttons.Button_Tab_Game)  .onClick.AddListener(() => SwitchTab(1));
        GetButton(typeof(Buttons), (int)Buttons.Button_Tab_Screen).onClick.AddListener(() => SwitchTab(2));
        GetButton(typeof(Buttons), (int)Buttons.Button_Cancel)    .onClick.AddListener(OnCancel);
        GetButton(typeof(Buttons), (int)Buttons.Button_Save)      .onClick.AddListener(OnSave);
    }

    private void SetupToggle(Toggles t, string key)
    {
        GetToggle(typeof(Toggles), (int)t).onValueChanged.AddListener(isOn =>
            PlayerPrefs.SetInt(key, isOn ? 1 : 0));
    }

    // ─── 열릴 때마다: 초기값 스냅샷 + UI 갱신 ────────────────────────────────

    private void LoadInitialValues()
    {
        _initBGM         = PlayerPrefs.GetFloat("BGMVolume",     0.8f);
        _initSFX         = PlayerPrefs.GetFloat("SFXVolume",     0.9f);
        _initVibration   = PlayerPrefs.GetInt("Vibration",       1);
        _initDamage      = PlayerPrefs.GetInt("DamageText",      1);
        _initFPS         = PlayerPrefs.GetInt("FPSDisplay",      0);
        _initScreenShake = PlayerPrefs.GetInt("ScreenShake",     1);
        _initParticle    = PlayerPrefs.GetInt("Particle",        1);
        _initLanguage    = PlayerPrefs.GetString("Language",    "ko");
        _initQuality     = PlayerPrefs.GetInt("GraphicsQuality", 1);
    }

    private void RefreshUI()
    {
        var bgmTxt = GetText(typeof(Texts), (int)Texts.Text_BGM);
        var sfxTxt = GetText(typeof(Texts), (int)Texts.Text_SFX);

        GetSlider(typeof(Sliders), (int)Sliders.Slider_BGM).SetValueWithoutNotify(_initBGM);
        GetSlider(typeof(Sliders), (int)Sliders.Slider_SFX).SetValueWithoutNotify(_initSFX);
        bgmTxt.text = Mathf.RoundToInt(_initBGM * 100).ToString();
        sfxTxt.text = Mathf.RoundToInt(_initSFX * 100).ToString();

        RefreshToggle(Toggles.Toggle_Vibration,   _initVibration   == 1);
        RefreshToggle(Toggles.Toggle_Damage,       _initDamage      == 1);
        RefreshToggle(Toggles.Toggle_FPS,          _initFPS         == 1);
        RefreshToggle(Toggles.Toggle_Screen_Shake, _initScreenShake == 1);
        RefreshToggle(Toggles.Toggle_Particle,     _initParticle    == 1);

        RefreshLanguageButtons();
        RefreshQualityButtons();
    }

    private void RefreshLanguageButtons()
    {
        bool isKo = PlayerPrefs.GetString("Language", "ko") == "ko";
        GetButton(typeof(Buttons), (int)Buttons.Button_Korean) .image.color = isKo  ? BTN_ACTIVE : BTN_INACTIVE;
        GetButton(typeof(Buttons), (int)Buttons.Button_English).image.color = !isKo ? BTN_ACTIVE : BTN_INACTIVE;
    }

    private void RefreshQualityButtons()
    {
        int q = PlayerPrefs.GetInt("GraphicsQuality", 1);
        GetButton(typeof(Buttons), (int)Buttons.Button_Low)   .image.color = q == 0 ? BTN_ACTIVE : BTN_INACTIVE;
        GetButton(typeof(Buttons), (int)Buttons.Button_Middle).image.color = q == 1 ? BTN_ACTIVE : BTN_INACTIVE;
        GetButton(typeof(Buttons), (int)Buttons.Button_High)  .image.color = q == 2 ? BTN_ACTIVE : BTN_INACTIVE;
    }

    private void RefreshToggle(Toggles t, bool isOn)
    {
        var toggle = GetToggle(typeof(Toggles), (int)t);
        toggle.SetIsOnWithoutNotify(isOn);
        toggle.GetComponent<UI_ToggleSwitch>().Refresh();
    }

    // ─── 탭 전환 ──────────────────────────────────────────────────────────────

    private static readonly Color BTN_ACTIVE   = new Color(0.784f, 0.525f, 0.039f, 1f);  // #C8860A
    private static readonly Color BTN_INACTIVE = new Color(0.149f, 0.110f, 0.078f, 1f);  // #261C14

    private void SwitchTab(int index)
    {
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Sound) .SetActive(index == 0);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Game)  .SetActive(index == 1);
        GetObject(typeof(GameObjects), (int)GameObjects.Panel_Screen).SetActive(index == 2);

        GetObject(typeof(GameObjects), (int)GameObjects.Object_Sound_Disable) .SetActive(index != 0);
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Game_Disable)  .SetActive(index != 1);
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Screen_Disable).SetActive(index != 2);

        SetTabActive(Buttons.Button_Tab_Sound,  index == 0);
        SetTabActive(Buttons.Button_Tab_Game,   index == 1);
        SetTabActive(Buttons.Button_Tab_Screen, index == 2);
    }

    private void SetTabActive(Buttons btn, bool active)
    {
        GetButton(typeof(Buttons), (int)btn).image.color = active ? BTN_ACTIVE : BTN_INACTIVE;
    }

    // ─── 저장 / 취소 ──────────────────────────────────────────────────────────

    private void OnSave()
    {
        PlayerPrefs.Save();
        OnClose();
    }

    private void OnCancel()
    {
        Managers.SoundM.SetBgmVolume(_initBGM);
        Managers.SoundM.SetSfxVolume(_initSFX);
        PlayerPrefs.SetInt("Vibration",       _initVibration);
        PlayerPrefs.SetInt("DamageText",      _initDamage);
        PlayerPrefs.SetInt("FPSDisplay",      _initFPS);
        PlayerPrefs.SetInt("ScreenShake",     _initScreenShake);
        PlayerPrefs.SetInt("Particle",        _initParticle);
        PlayerPrefs.SetString("Language",     _initLanguage);
        PlayerPrefs.SetInt("GraphicsQuality", _initQuality);
        QualitySettings.SetQualityLevel(_initQuality, true);
        OnClose();
    }

    private void OnClose() => Managers.PoolM.Push(gameObject);

    private static void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("GraphicsQuality", index);
    }
}
