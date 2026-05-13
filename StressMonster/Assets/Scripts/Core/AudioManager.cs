using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _bgmFadeSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Fade Settings")]
    [SerializeField] private float _bgmFadeDuration = 1f;

    [Header("Volume")]
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private float _bgmVolume = 0.7f;
    [SerializeField] private float _sfxVolume = 1f;
    [SerializeField] private bool _isMuted = false;

    private Coroutine _bgmFadeCoroutine;
    private AudioClip _currentBgm;

    private const string MasterVolumeKey = "Audio_MasterVolume";
    private const string BgmVolumeKey = "Audio_BGMVolume";
    private const string SfxVolumeKey = "Audio_SFXVolume";
    private const string MuteKey = "Audio_IsMuted";

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }
    }

    public float BGMVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }
    }

    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            ApplyVolumes();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
        LoadSettings();
    }

    private void InitializeAudioSources()
    {
        if (_bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.SetParent(transform);
            _bgmSource = bgmObj.AddComponent<AudioSource>();
        }

        if (_bgmFadeSource == null)
        {
            GameObject bgmFadeObj = new GameObject("BGM_FadeSource");
            bgmFadeObj.transform.SetParent(transform);
            _bgmFadeSource = bgmFadeObj.AddComponent<AudioSource>();
        }

        if (_sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);
            _sfxSource = sfxObj.AddComponent<AudioSource>();
        }

        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmFadeSource.loop = true;
        _bgmFadeSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
    }

    public void PlayBGM(AudioClip clip, bool loop = true, bool fade = true)
    {
        if (clip == null)
            return;

        if (_currentBgm == clip && _bgmSource.isPlaying)
            return;

        _currentBgm = clip;

        if (fade && _bgmSource.isPlaying)
        {
            if (_bgmFadeCoroutine != null)
                StopCoroutine(_bgmFadeCoroutine);
            _bgmFadeCoroutine = StartCoroutine(CrossfadeBGM(clip, loop));
        }
        else
        {
            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
                _bgmFadeCoroutine = null;
            }

            _bgmSource.clip = clip;
            _bgmSource.loop = loop;
            _bgmSource.volume = _isMuted ? 0f : _masterVolume * _bgmVolume;
            _bgmSource.Play();
        }
    }

    public void StopBGM(bool fade = true)
    {
        if (fade && _bgmSource.isPlaying)
        {
            if (_bgmFadeCoroutine != null)
                StopCoroutine(_bgmFadeCoroutine);
            _bgmFadeCoroutine = StartCoroutine(FadeOutBGM());
        }
        else
        {
            _bgmSource.Stop();
            _bgmSource.clip = null;
            _currentBgm = null;
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
            return;

        float finalVolume = _isMuted ? 0f : _masterVolume * _sfxVolume * volume;
        _sfxSource.PlayOneShot(clip, finalVolume);
    }

    private IEnumerator CrossfadeBGM(AudioClip newClip, bool loop)
    {
        _bgmFadeSource.clip = _bgmSource.clip;
        _bgmFadeSource.time = _bgmSource.time;
        _bgmFadeSource.volume = _bgmSource.volume;
        _bgmFadeSource.loop = _bgmSource.loop;
        _bgmFadeSource.Play();

        _bgmSource.clip = newClip;
        _bgmSource.loop = loop;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        float timer = 0f;
        float startFadeVolume = _bgmFadeSource.volume;

        while (timer < _bgmFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / _bgmFadeDuration;

            _bgmFadeSource.volume = Mathf.Lerp(startFadeVolume, 0f, t);
            _bgmSource.volume = Mathf.Lerp(0f, _isMuted ? 0f : _masterVolume * _bgmVolume, t);

            yield return null;
        }

        _bgmFadeSource.Stop();
        _bgmFadeSource.clip = null;
        _bgmSource.volume = _isMuted ? 0f : _masterVolume * _bgmVolume;
        _bgmFadeCoroutine = null;
    }

    private IEnumerator FadeOutBGM()
    {
        float timer = 0f;
        float startVolume = _bgmSource.volume;

        while (timer < _bgmFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / _bgmFadeDuration;
            _bgmSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        _bgmSource.Stop();
        _bgmSource.clip = null;
        _currentBgm = null;
        _bgmFadeCoroutine = null;
    }

    private void ApplyVolumes()
    {
        if (_bgmSource != null)
            _bgmSource.volume = _isMuted ? 0f : _masterVolume * _bgmVolume;

        if (_bgmFadeSource != null && _bgmFadeSource.isPlaying)
            _bgmFadeSource.volume = _isMuted ? 0f : _masterVolume * _bgmVolume;

        AudioListener.volume = 1f;
    }

    public void ToggleMute()
    {
        IsMuted = !_isMuted;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, _masterVolume);
        PlayerPrefs.SetFloat(BgmVolumeKey, _bgmVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
        PlayerPrefs.SetInt(MuteKey, _isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(MasterVolumeKey))
            _masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey);
        if (PlayerPrefs.HasKey(BgmVolumeKey))
            _bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey);
        if (PlayerPrefs.HasKey(SfxVolumeKey))
            _sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey);
        if (PlayerPrefs.HasKey(MuteKey))
            _isMuted = PlayerPrefs.GetInt(MuteKey) == 1;

        ApplyVolumes();
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }
}
