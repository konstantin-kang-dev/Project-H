using DG.Tweening;
using GameAudio;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnhancedAudio : MonoBehaviour
{
    [SerializeField] public GameAudioType GameAudioType = GameAudioType.None;
    [SerializeField] List<AudioClip> _audioList = new List<AudioClip>();
    [SerializeField] bool _randomizeSounds = true;

    [Header("Fade")]
    [SerializeField] bool _doFade = false;
    [SerializeField] float _inDuration = 0.5f;
    [SerializeField] float _outDuration = 0.5f;
    Tween _fadeTween;

    float _initialVolume = 0f;
    float _volumeMultiplier = 1f;
    float _totalVolume => _initialVolume * _volumeMultiplier;

    int _prevKey = -1;
    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _initialVolume = _audioSource.volume;
    }

    private void Start()
    {
        GlobalAudioManager.Instance.SetupVolume(this);
    }

    public void Play(float customVolume = -1f)
    {
        if (_audioSource.isPlaying) Stop();

        if (_doFade)
        {
            if(_fadeTween != null)
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }
        }

        if (_audioList.Count == 0) return;

        if(_audioList.Count == 1)
        {
            _audioSource.clip = _audioList[0];
        }
        else
        {
            int key = ProjectUtils.GetRandomExcluding(0, _audioList.Count, _prevKey);
            _audioSource.clip = _audioList[key];
            _prevKey = key;
        }

        if(customVolume >= 0f)
        {
            _audioSource.volume = _totalVolume * customVolume;
        }
        else
        {
            _audioSource.volume = _totalVolume;
        }

        _audioSource.Play();

        if (_doFade)
        {
            float volume = customVolume >= 0f ? _totalVolume * customVolume : _totalVolume;
            _fadeTween = _audioSource.DOFade(volume, _inDuration).From(0f);
        }
    }

    public void Stop()
    {
        if (_doFade)
        {
            if(_fadeTween != null)
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }

            _fadeTween = _audioSource.DOFade(0f, _outDuration).From(_totalVolume).OnComplete(() =>
            {
                _audioSource.Stop();
            });
        }
        else
        {
            _audioSource.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        _volumeMultiplier = volume;
        _audioSource.volume = _totalVolume;
        Debug.Log($"[EnhancedAudio] Set volume: {volume}");
    }
}