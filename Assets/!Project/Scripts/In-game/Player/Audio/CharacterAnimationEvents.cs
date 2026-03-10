using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    [SerializeField] CharacterAudioService _characterAudioService;

    private void Awake()
    {

    }

    public void PlayAudio(CharacterAudioType audioType)
    {
        if (_characterAudioService == null) return;

        _characterAudioService.Play(audioType);
    }
}
