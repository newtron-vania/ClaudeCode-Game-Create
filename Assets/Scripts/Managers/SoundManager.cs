using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 사운드 및 음악 재생 관리 매니저
/// BGM, SFX 재생 및 볼륨 조절 담당
/// </summary>
public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private int _maxSfxSources = 10;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Volume Settings")]
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private float _bgmVolume = 1f;
    [SerializeField] private float _sfxVolume = 1f;

    // SFX AudioSource 풀
    private List<AudioSource> _sfxSourcePool = new List<AudioSource>();
    private Queue<AudioSource> _availableSfxSources = new Queue<AudioSource>();

    // 현재 재생 중인 BGM
    private string _currentBgmAddress = "";
    private Coroutine _bgmFadeCoroutine;

    // 볼륨 프로퍼티
    public float MasterVolume => _masterVolume;
    public float BgmVolume => _bgmVolume;
    public float SfxVolume => _sfxVolume;

    // 재생 상태
    public bool IsBgmPlaying => _bgmSource != null && _bgmSource.isPlaying;
    public string CurrentBgm => _currentBgmAddress;

    protected override void Awake()
    {
        base.Awake();
        InitializeAudioSources();
        ApplyVolumeSettings();
        Debug.Log("[INFO] SoundManager::Awake - SoundManager initialized");
    }

    #region 초기화

    /// <summary>
    /// AudioSource 초기화
    /// </summary>
    private void InitializeAudioSources()
    {
        // BGM AudioSource 생성
        if (_bgmSource == null)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.volume = _bgmVolume;
        }

        // SFX AudioSource 풀 생성
        for (int i = 0; i < _maxSfxSources; i++)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = _sfxVolume;

            _sfxSourcePool.Add(sfxSource);
            _availableSfxSources.Enqueue(sfxSource);
        }

        Debug.Log($"[INFO] SoundManager::InitializeAudioSources - Created {_maxSfxSources} SFX sources");
    }

    /// <summary>
    /// 볼륨 설정 적용
    /// </summary>
    private void ApplyVolumeSettings()
    {
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("MasterVolume", LinearToDecibel(_masterVolume));
            _audioMixer.SetFloat("BGMVolume", LinearToDecibel(_bgmVolume));
            _audioMixer.SetFloat("SFXVolume", LinearToDecibel(_sfxVolume));
        }
        else
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = _bgmVolume * _masterVolume;
            }

            foreach (var sfxSource in _sfxSourcePool)
            {
                sfxSource.volume = _sfxVolume * _masterVolume;
            }
        }
    }

    #endregion

    #region BGM 재생

    /// <summary>
    /// BGM 재생 (Addressables)
    /// </summary>
    /// <param name="address">오디오 클립 Addressable 주소</param>
    /// <param name="fadeTime">페이드 인 시간 (초)</param>
    public void PlayBGM(string address, float fadeTime = 1f)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning("[WARNING] SoundManager::PlayBGM - Address is null or empty");
            return;
        }

        // 같은 BGM이 이미 재생 중이면 무시
        if (_currentBgmAddress == address && IsBgmPlaying)
        {
            Debug.Log($"[INFO] SoundManager::PlayBGM - BGM already playing: {address}");
            return;
        }

        // Addressables로 오디오 클립 로드
        ResourceManager.Instance.LoadAsync<AudioClip>(address, (clip) =>
        {
            if (clip != null)
            {
                PlayBGMClip(clip, address, fadeTime);
            }
            else
            {
                Debug.LogError($"[ERROR] SoundManager::PlayBGM - Failed to load BGM: {address}");
            }
        });
    }

    /// <summary>
    /// BGM 클립 재생 (내부용)
    /// </summary>
    private void PlayBGMClip(AudioClip clip, string address, float fadeTime)
    {
        // 이전 페이드 코루틴 중지
        if (_bgmFadeCoroutine != null)
        {
            StopCoroutine(_bgmFadeCoroutine);
        }

        _currentBgmAddress = address;
        _bgmSource.clip = clip;
        _bgmSource.Play();

        // 페이드 인
        if (fadeTime > 0f)
        {
            _bgmFadeCoroutine = StartCoroutine(FadeBGM(0f, _bgmVolume * _masterVolume, fadeTime));
        }
        else
        {
            _bgmSource.volume = _bgmVolume * _masterVolume;
        }

        Debug.Log($"[INFO] SoundManager::PlayBGMClip - BGM started: {address}");
    }

    /// <summary>
    /// BGM 중지
    /// </summary>
    /// <param name="fadeTime">페이드 아웃 시간 (초)</param>
    public void StopBGM(float fadeTime = 1f)
    {
        if (!IsBgmPlaying)
        {
            return;
        }

        // 이전 페이드 코루틴 중지
        if (_bgmFadeCoroutine != null)
        {
            StopCoroutine(_bgmFadeCoroutine);
        }

        if (fadeTime > 0f)
        {
            _bgmFadeCoroutine = StartCoroutine(FadeBGMAndStop(fadeTime));
        }
        else
        {
            _bgmSource.Stop();
            _currentBgmAddress = "";
            Debug.Log("[INFO] SoundManager::StopBGM - BGM stopped");
        }
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        if (IsBgmPlaying)
        {
            _bgmSource.Pause();
            Debug.Log("[INFO] SoundManager::PauseBGM - BGM paused");
        }
    }

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        if (_bgmSource.clip != null && !_bgmSource.isPlaying)
        {
            _bgmSource.UnPause();
            Debug.Log("[INFO] SoundManager::ResumeBGM - BGM resumed");
        }
    }

    #endregion

    #region SFX 재생

    /// <summary>
    /// SFX 재생 (Addressables)
    /// </summary>
    /// <param name="address">오디오 클립 Addressable 주소</param>
    /// <param name="volume">볼륨 (0.0 ~ 1.0)</param>
    public void PlaySFX(string address, float volume = 1f)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning("[WARNING] SoundManager::PlaySFX - Address is null or empty");
            return;
        }

        ResourceManager.Instance.LoadAsync<AudioClip>(address, (clip) =>
        {
            if (clip != null)
            {
                PlaySFXClip(clip, volume);
            }
            else
            {
                Debug.LogError($"[ERROR] SoundManager::PlaySFX - Failed to load SFX: {address}");
            }
        });
    }

    /// <summary>
    /// SFX 클립 재생 (내부용)
    /// </summary>
    private void PlaySFXClip(AudioClip clip, float volume)
    {
        AudioSource sfxSource = GetAvailableSfxSource();

        if (sfxSource == null)
        {
            Debug.LogWarning("[WARNING] SoundManager::PlaySFXClip - No available SFX source");
            return;
        }

        sfxSource.clip = clip;
        sfxSource.volume = volume * _sfxVolume * _masterVolume;
        sfxSource.Play();

        // 재생 완료 후 풀로 반환
        StartCoroutine(ReturnSfxSourceToPool(sfxSource, clip.length));
    }

    /// <summary>
    /// 3D 위치에서 SFX 재생
    /// </summary>
    /// <param name="address">오디오 클립 Addressable 주소</param>
    /// <param name="position">재생 위치</param>
    /// <param name="volume">볼륨 (0.0 ~ 1.0)</param>
    public void PlaySFXAtPoint(string address, Vector3 position, float volume = 1f)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning("[WARNING] SoundManager::PlaySFXAtPoint - Address is null or empty");
            return;
        }

        ResourceManager.Instance.LoadAsync<AudioClip>(address, (clip) =>
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, volume * _sfxVolume * _masterVolume);
                Debug.Log($"[INFO] SoundManager::PlaySFXAtPoint - SFX played at {position}");
            }
            else
            {
                Debug.LogError($"[ERROR] SoundManager::PlaySFXAtPoint - Failed to load SFX: {address}");
            }
        });
    }

    /// <summary>
    /// 사용 가능한 SFX AudioSource 가져오기
    /// </summary>
    private AudioSource GetAvailableSfxSource()
    {
        // 사용 가능한 소스가 없으면 재생 중이 아닌 소스 찾기
        if (_availableSfxSources.Count == 0)
        {
            foreach (var source in _sfxSourcePool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return null;
        }

        return _availableSfxSources.Dequeue();
    }

    /// <summary>
    /// SFX AudioSource를 풀로 반환
    /// </summary>
    private IEnumerator ReturnSfxSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        _availableSfxSources.Enqueue(source);
    }

    #endregion

    #region 볼륨 조절

    /// <summary>
    /// 마스터 볼륨 설정
    /// </summary>
    /// <param name="volume">볼륨 (0.0 ~ 1.0)</param>
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);

        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("MasterVolume", LinearToDecibel(_masterVolume));
        }
        else
        {
            ApplyVolumeSettings();
        }

        Debug.Log($"[INFO] SoundManager::SetMasterVolume - Volume set to {_masterVolume}");
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    /// <param name="volume">볼륨 (0.0 ~ 1.0)</param>
    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);

        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("BGMVolume", LinearToDecibel(_bgmVolume));
        }
        else if (_bgmSource != null)
        {
            _bgmSource.volume = _bgmVolume * _masterVolume;
        }

        Debug.Log($"[INFO] SoundManager::SetBGMVolume - Volume set to {_bgmVolume}");
    }

    /// <summary>
    /// SFX 볼륨 설정
    /// </summary>
    /// <param name="volume">볼륨 (0.0 ~ 1.0)</param>
    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);

        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("SFXVolume", LinearToDecibel(_sfxVolume));
        }
        else
        {
            foreach (var sfxSource in _sfxSourcePool)
            {
                if (sfxSource.isPlaying)
                {
                    sfxSource.volume = _sfxVolume * _masterVolume;
                }
            }
        }

        Debug.Log($"[INFO] SoundManager::SetSFXVolume - Volume set to {_sfxVolume}");
    }

    /// <summary>
    /// 모든 사운드 음소거/해제
    /// </summary>
    /// <param name="mute">음소거 여부</param>
    public void MuteAll(bool mute)
    {
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("MasterVolume", mute ? -80f : LinearToDecibel(_masterVolume));
        }
        else
        {
            AudioListener.volume = mute ? 0f : 1f;
        }

        Debug.Log($"[INFO] SoundManager::MuteAll - Mute set to {mute}");
    }

    #endregion

    #region 유틸리티

    /// <summary>
    /// BGM 페이드 코루틴
    /// </summary>
    private IEnumerator FadeBGM(float startVolume, float targetVolume, float duration)
    {
        _bgmSource.volume = startVolume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        _bgmSource.volume = targetVolume;
        _bgmFadeCoroutine = null;
    }

    /// <summary>
    /// BGM 페이드 아웃 후 중지
    /// </summary>
    private IEnumerator FadeBGMAndStop(float duration)
    {
        float startVolume = _bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _bgmSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        _bgmSource.Stop();
        _bgmSource.volume = _bgmVolume * _masterVolume;
        _currentBgmAddress = "";
        _bgmFadeCoroutine = null;

        Debug.Log("[INFO] SoundManager::FadeBGMAndStop - BGM stopped after fade");
    }

    /// <summary>
    /// 선형 볼륨을 데시벨로 변환
    /// </summary>
    private float LinearToDecibel(float linear)
    {
        if (linear <= 0f)
        {
            return -80f;
        }
        return Mathf.Log10(linear) * 20f;
    }

    #endregion

    #region 정리

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // BGM 중지
        if (_bgmSource != null)
        {
            _bgmSource.Stop();
        }

        // 모든 SFX 중지
        foreach (var sfxSource in _sfxSourcePool)
        {
            if (sfxSource.isPlaying)
            {
                sfxSource.Stop();
            }
        }

        Debug.Log("[INFO] SoundManager::OnDestroy - SoundManager destroyed");
    }

    #endregion
}
