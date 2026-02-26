using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum CharacterAudioType
{
    None = 0,
    Footstep = 1,
    PickUp = 2,
}

public class CharacterAudioService : SerializedMonoBehaviour
{
    [SerializeField] Dictionary<CharacterAudioType, EnhancedAudio> _audios = new Dictionary<CharacterAudioType, EnhancedAudio>();

    public void Play(CharacterAudioType audioType)
    {
        if (!_audios.ContainsKey(audioType)) throw new Exception($"[CharacterAudioService] Audio with type: {audioType} not found in audios list.");

        _audios[audioType].Play();
    }
}
