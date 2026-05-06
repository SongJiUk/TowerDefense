using UnityEngine;

/// <summary>
/// 설정값 중앙 관리. PlayerPrefs에서 읽어 각 시스템에 제공.
/// 사용: Managers.SettingsM.IsScreenShakeOn / .Vibrate()
/// </summary>
public class SettingsManager
{
    public bool   IsVibrationOn   => PlayerPrefs.GetInt("Vibration",   1) == 1;
    public bool   IsDamageTextOn  => PlayerPrefs.GetInt("DamageText",  1) == 1;
    public bool   IsFPSOn         => PlayerPrefs.GetInt("FPSDisplay",  0) == 1;
    public bool   IsScreenShakeOn => PlayerPrefs.GetInt("ScreenShake", 1) == 1;
    public bool   IsParticleOn    => PlayerPrefs.GetInt("Particle",    1) == 1;
    public string Language        => PlayerPrefs.GetString("Language", "ko");

    /// <summary>진동 설정이 켜진 경우에만 진동.</summary>
    public void Vibrate()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (IsVibrationOn)
            Handheld.Vibrate();
#endif
    }
}
