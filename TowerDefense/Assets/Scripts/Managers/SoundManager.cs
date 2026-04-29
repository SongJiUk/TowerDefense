using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM / SFX 재생 매니저. Managers.SoundM으로 접근.
/// Init(root)에서 AudioSource를 동적 생성하므로 Addressable 클립은 키 기반으로 재생.
/// </summary>
public class SoundManager
{
    private const string BGM_VOL_KEY = "BGMVolume";
    private const string SFX_VOL_KEY = "SFXVolume";
    private const int SFX_POOL_SIZE = 8;

    private AudioSource _bgm;
    private readonly List<AudioSource> _sfxPool = new();

    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    public void Init(GameObject root)
    {
        BgmVolume = PlayerPrefs.GetFloat(BGM_VOL_KEY, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);

        var audioRoot = new GameObject("@Audio");
        audioRoot.transform.SetParent(root.transform);

        _bgm = audioRoot.AddComponent<AudioSource>();
        _bgm.loop = true;
        _bgm.playOnAwake = false;
        _bgm.volume = BgmVolume;

        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            var sfxGo = new GameObject($"SFX_{i}");
            sfxGo.transform.SetParent(audioRoot.transform);
            var src = sfxGo.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.volume = SfxVolume;
            _sfxPool.Add(src);
        }
    }

    // ─── BGM ──────────────────────────────────────────────────────────────────

    public void PlayBGM(string key)
    {
        var clip = Managers.ResourceM.Load<AudioClip>(key);
        if (clip == null) return;
        if (_bgm.clip == clip && _bgm.isPlaying) return;
        _bgm.clip = clip;
        _bgm.Play();
    }

    public void StopBGM()
    {
        _bgm?.Stop();
        if (_bgm != null) _bgm.clip = null;
    }

    // ─── SFX ──────────────────────────────────────────────────────────────────

    public void PlaySFX(string key)
    {
        var clip = Managers.ResourceM.Load<AudioClip>(key);
        if (clip == null) return;
        PlaySFX(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        var src = GetFreeSource();
        src.clip = clip;
        src.Play();
    }

    // ─── 볼륨 ─────────────────────────────────────────────────────────────────

    public void SetBgmVolume(float v)
    {
        BgmVolume = Mathf.Clamp01(v);
        if (_bgm != null) _bgm.volume = BgmVolume;
        PlayerPrefs.SetFloat(BGM_VOL_KEY, BgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float v)
    {
        SfxVolume = Mathf.Clamp01(v);
        foreach (var src in _sfxPool)
            if (src != null) src.volume = SfxVolume;
        PlayerPrefs.SetFloat(SFX_VOL_KEY, SfxVolume);
        PlayerPrefs.Save();
    }

    // ─── 내부 ─────────────────────────────────────────────────────────────────

    private AudioSource GetFreeSource()
    {
        foreach (var src in _sfxPool)
            if (!src.isPlaying) return src;
        return _sfxPool[0];
    }
}
