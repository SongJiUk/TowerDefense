using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    /// <summary>진동 설정이 켜진 경우에만 진동.</summary>
    public void Vibrate()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (IsVibrationOn)
            Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// 그래픽 품질 적용. 프로젝트에 Low/Mid/High용 URP 에셋이 별도로 없어,
    /// 단일 URP 에셋의 런타임 프로퍼티(렌더 스케일·그림자 거리·MSAA)를 직접 조정해 체감 차이를 만든다.
    /// </summary>
    public void ApplyGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);

        if (QualitySettings.renderPipeline is not UniversalRenderPipelineAsset urpAsset) return;

        switch (index)
        {
            case 0: // Low
                urpAsset.renderScale = 0.75f;
                urpAsset.shadowDistance = 15f;
                urpAsset.msaaSampleCount = 1;
                break;
            case 1: // Middle
                urpAsset.renderScale = 0.9f;
                urpAsset.shadowDistance = 30f;
                urpAsset.msaaSampleCount = 2;
                break;
            default: // High
                urpAsset.renderScale = 1f;
                urpAsset.shadowDistance = 50f;
                urpAsset.msaaSampleCount = 4;
                break;
        }
    }
}
