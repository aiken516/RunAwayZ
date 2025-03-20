using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundManager : TSingleton<SoundManager>
{
    public AudioSource _bgmSource;
    public AudioSource _sfxSource;

    private Dictionary<string, AudioClip> _bgmClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _sfxClips = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public struct NamedAudioClip
    {
        public string Name;
        public AudioClip Clip;
    }

    public NamedAudioClip[] BGMClipList;
    public NamedAudioClip[] SFXClipList;

    void Awake()
    {
        base.Awake();
        InitializeAudioClips();
    }

    private void InitializeAudioClips()
    {
        foreach (var clip in BGMClipList)
        {
            if (!_bgmClips.ContainsKey(clip.Name))
            { 
                _bgmClips.Add(clip.Name, clip.Clip);
            }
        }

        foreach (var clip in SFXClipList)
        {
            if (!_sfxClips.ContainsKey(clip.Name))
            {
                _sfxClips.Add(clip.Name, clip.Clip);
            }
        }
    }

    public void PlayBGM(string bgmName, float fadeDuration = 1.0f)
    {
        if (_bgmClips.ContainsKey(bgmName))
        {
            if (_currentBGMCoroutine != null)
            { 
                StopCoroutine(_currentBGMCoroutine);
            }
            _currentBGMCoroutine = StartCoroutine(FadeOutBGM(fadeDuration, () =>
            {
                _bgmSource.spatialBlend = 0;
                _bgmSource.clip = _bgmClips[bgmName];
                _bgmSource.Play();
                _currentBGMCoroutine = StartCoroutine(FadeInBGM(fadeDuration));
            }));
        }
    }

    public void PlayerSFX(string sfxName)
    {
        if (_sfxClips.ContainsKey(sfxName))
        {
            _sfxSource.PlayOneShot(_sfxClips[sfxName]);
        }
    }

    public void PlaySFX(string sfxName, Vector3 position)
    {
        if (_sfxClips.ContainsKey(sfxName))
        {
            AudioSource.PlayClipAtPoint(_sfxClips[sfxName], position);
        }
    }

    public void StopBGM() => _bgmSource.Stop();
    public void StopSFX() => _sfxSource.Stop();
    public void PauseBGM() => _bgmSource.Pause();
    public void UnPauseBGM() => _bgmSource.UnPause();
    public void PauseSFX() => _sfxSource.Pause();
    public void UnPauseSFX() => _sfxSource.UnPause();

    public void SetBGMVolume(float volume)
    { 
        _bgmSource.volume = Mathf.Clamp(volume, 0 , 1);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    private Coroutine _currentBGMCoroutine;

    private IEnumerator FadeOutBGM(float duration, Action onFadeEnd)
    {
        float startVolume = _bgmSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            _bgmSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        _bgmSource.volume = 0;
        onFadeEnd?.Invoke();
    }

    private IEnumerator FadeInBGM(float duration)
    {
        float startVolume = 0.0f; 
        _bgmSource.volume = 0.0f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            _bgmSource.volume = Mathf.Lerp(startVolume, 1.0f, t / duration);
            yield return null;
        }

        _bgmSource.volume = 1.0f;
    }
}
