using UnityEngine;

public class AudioZoneTrigger : MonoBehaviour
{
    AudioZone _audioZone;

    private void Awake()
    {
        _audioZone = GetComponentInParent<AudioZone>();
    }

    private void OnTriggerEnter(Collider other) => _audioZone.OnChildTriggerEnter(other);
    private void OnTriggerExit(Collider other) => _audioZone.OnChildTriggerExit(other);
}
