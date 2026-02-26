using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    [SerializeField] PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    public void PlayFootstep()
    {
        if (_playerController == null) return;

        _playerController.CharacterAudioService.Play(CharacterAudioType.Footstep);
    }
}
