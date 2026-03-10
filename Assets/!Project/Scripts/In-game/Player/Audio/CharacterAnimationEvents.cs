using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    [SerializeField] CharacterAudioService _characterAudioService;

    private void Awake()
    {

    }

    public void PlayFootstep()
    {
        if (_characterAudioService == null) return;

        _characterAudioService.Play(CharacterAudioType.Footstep);
    }

    public void PlayHeavyFootstep()
    {
        if (_characterAudioService == null) return;

        _characterAudioService.Play(CharacterAudioType.HeavyFootstep);
    }
}
