using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 팝업. BGM/SFX 볼륨 슬라이더, 그래픽 품질.
/// 오브젝트 이름:
///   Slider_BGM, Slider_SFX
///   Dropdown_Quality
///   Button_Close
/// </summary>
public class UI_SettingsPopup : UI_Base
{
    enum Sliders  { Slider_BGM, Slider_SFX }
    enum Buttons  { Button_Close }
    enum Dropdowns { Dropdown_Quality }

    private bool _initialized;

    void OnEnable()  => Managers.UIM.RequestPause();
    void OnDisable() => Managers.UIM.ReleasePause();

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;
        _initialized = true;

        BindSlider(typeof(Sliders));
        BindButton(typeof(Buttons));
        BindDropdown(typeof(Dropdowns));

        var bgm = GetSlider(typeof(Sliders), (int)Sliders.Slider_BGM);
        var sfx = GetSlider(typeof(Sliders), (int)Sliders.Slider_SFX);

        bgm.value = Managers.SoundM.BgmVolume;
        sfx.value = Managers.SoundM.SfxVolume;

        bgm.onValueChanged.AddListener(v => Managers.SoundM.SetBgmVolume(v));
        sfx.onValueChanged.AddListener(v => Managers.SoundM.SetSfxVolume(v));

        var quality = GetDropdown(typeof(Dropdowns), (int)Dropdowns.Dropdown_Quality);
        quality.value = PlayerPrefs.GetInt("GraphicsQuality", 1);
        quality.onValueChanged.AddListener(OnQualityChanged);

        GetButton(typeof(Buttons), (int)Buttons.Button_Close).onClick.AddListener(OnClose);
        return true;
    }

    private void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("GraphicsQuality", index);
        PlayerPrefs.Save();
    }

    private void OnClose()
        => Managers.PoolM.Push(gameObject);
}
