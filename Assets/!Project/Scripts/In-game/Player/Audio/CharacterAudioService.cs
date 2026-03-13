using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum CharacterAudioType
{
    None = 0,
    Footstep = 1,
    PickUp = 2,
    HeavyBreath = 3,
    Jumpscare = 4,
    HeavyFootstep = 5,
    ChasingScream = 6,
    Detection = 7,

    JumpStart = 20,
    JumpLand = 21,
}

[Serializable] 
public class CharacterSound
{
    public CharacterAudioType Type;
    public EnhancedAudio Audio;

}

public class CharacterAudioService : NetworkBehaviour
{
    [SerializeField] List<CharacterSound> _audios = new List<CharacterSound>();

    public void Play(CharacterAudioType audioType, bool isNetworked = false)
    {
        CharacterSound characterSound = _audios.FirstOrDefault((x)=> x.Type == audioType);
        if (characterSound == null) throw new Exception($"[CharacterAudioService] Audio with type: {audioType} not found in audios list.");

        characterSound.Audio.Play();

        if(isNetworked)
        {
            RPC_RequestPlaySound(characterSound.Type);
        }
    }

    public void Stop(CharacterAudioType audioType, bool isNetworked = false)
    {
        CharacterSound characterSound = _audios.FirstOrDefault((x) => x.Type == audioType);
        if (characterSound == null) throw new Exception($"[CharacterAudioService] Audio with type: {audioType} not found in audios list.");

        characterSound.Audio.Stop();

        if (isNetworked)
        {
            RPC_RequestStopSound(characterSound.Type);
        }
    }

    public void HandleStaminaEmpty()
    {
        Play(CharacterAudioType.HeavyBreath, true);
    }

    public void HandlePickUp(IPickable pickable, int inventoryKey)
    {
        Play(CharacterAudioType.PickUp, true);
    }

    [ServerRpc(RequireOwnership = false)]
    void RPC_RequestPlaySound(CharacterAudioType audioType)
    {
        RPC_HandleObserversPlaySound(audioType);
    }

    [ObserversRpc]
    void RPC_HandleObserversPlaySound(CharacterAudioType audioType)
    {
        Play(audioType);
    }

    [ServerRpc(RequireOwnership = false)]
    void RPC_RequestStopSound(CharacterAudioType audioType)
    {
        RPC_HandleObserversStopSound(audioType);
    }

    [ObserversRpc]
    void RPC_HandleObserversStopSound(CharacterAudioType audioType)
    {
        Stop(audioType);
    }

}
