using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnhancedAudio : SerializedMonoBehaviour
{
    [SerializeField] List<AudioClip> _audioList = new List<AudioClip>();
    [SerializeField] bool _randomizeSounds = true;

    int _prevKey = -1;
    AudioSource _audioSource;

    private void Awake()
    {
        
    }

    public void Play()
    {
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

        _audioSource.Play();
    }
}