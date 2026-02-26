using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum InteractableObjectAudioType
{
    None = 0,
    DropCollision = 1,
    PickUp = 2,
    InteractionStateActive = 3,
    InteractionStateInactive = 4,
}

public class InteractableObjectAudioService : SerializedMonoBehaviour
{
    [SerializeField] Dictionary<InteractableObjectAudioType, EnhancedAudio> _audios = new Dictionary<InteractableObjectAudioType, EnhancedAudio>();
    
    public void Play(InteractableObjectAudioType audioType, float customVolume = -1f)
    {
        if (!_audios.ContainsKey(audioType)) throw new Exception($"[InteractableObjectAudioService] Audio with type: {audioType} not found in audios list.");

        _audios[audioType].Play(customVolume);
    }
}
