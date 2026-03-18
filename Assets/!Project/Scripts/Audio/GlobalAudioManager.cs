using Saves;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameAudio
{

    [Serializable]
    public enum SoundType
    {
        None = 0,
        UIClick = 1,
        UIHover = 2,
        UISwitch = 3,
        UINotification = 4,
        UILobbyJoin = 5,
        UILobbyLeave = 6,
        UIGameVictory = 7,
        UIGameLose = 8,
        UILocationIntro = 9,

        MenuAmbient = 20,
        GameAmbient = 21,
        EnemySpotted = 22,
    }

    [Serializable]
    public enum GameAudioType
    {
        None = 0,
        Environment = 1,
        Interface = 2,
    }

    public class GlobalAudioManager : SerializedMonoBehaviour
    {

        public static GlobalAudioManager Instance { get; private set; }
        AudioSave _relevantAudioSave;

        [SerializeField] Dictionary<SoundType, EnhancedAudio> _audios = new Dictionary<SoundType, EnhancedAudio>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Play(SoundType type)
        {
            if (!_audios.ContainsKey(type)) throw new Exception($"[GlobalAudioManager] Audio with type: {type} not found in audios list.");

            _audios[type].Play();
        }

        public void Stop(SoundType type)
        {
            if (!_audios.ContainsKey(type)) throw new Exception($"[GlobalAudioManager] Audio with type: {type} not found in audios list.");

            _audios[type].Stop();
        }

        public void ApplySave(AudioSave save)
        {
            _relevantAudioSave = save;

            EnhancedAudio[] enhancedAudios = FindObjectsByType<EnhancedAudio>(FindObjectsSortMode.InstanceID);
            foreach (var enhancedAudio in enhancedAudios)
            {
                SetupVolume(enhancedAudio);
            }
        }

        public void SetupVolume(EnhancedAudio enhancedAudio)
        {
            //if (_relevantAudioSave == null) throw new Exception($"[GlobalAudioManager] Relevant audio save doesn't exist.");
            if (_relevantAudioSave == null) return;

            switch (enhancedAudio.GameAudioType)
            {
                case GameAudioType.Environment:
                    float environmentVolume = (_relevantAudioSave.GlobalVolume / 100f) * (_relevantAudioSave.EnvironmentVolume / 100f);
                    enhancedAudio.SetVolume(environmentVolume);
                    break;
                case GameAudioType.Interface:
                    float interfaceVolume = (_relevantAudioSave.GlobalVolume / 100f) * (_relevantAudioSave.InterfaceVolume / 100f);
                    enhancedAudio.SetVolume(interfaceVolume);
                    break;
            }
        }

    }
}
