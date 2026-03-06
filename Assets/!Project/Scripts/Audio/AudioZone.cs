using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioZone : MonoBehaviour
{
    [SerializeField] private AudioMixerSnapshot snapshot;
    [SerializeField] private AudioMixerSnapshot defaultSnapshot;
    [SerializeField] private float transitionTime = 0.7f;

    private static readonly Stack<AudioMixerSnapshot> zoneStack = new();

    private void Awake()
    {
        zoneStack.Push(defaultSnapshot);
        defaultSnapshot.TransitionTo(transitionTime);
    }
    public void OnChildTriggerEnter(Collider other)
    {
        zoneStack.Push(snapshot);
        snapshot.TransitionTo(transitionTime);
    }

    public void OnChildTriggerExit(Collider other)
    {
        zoneStack.TryPop(out _);

        if (zoneStack.TryPeek(out var previous))
            previous.TransitionTo(transitionTime);
        else
            defaultSnapshot.TransitionTo(transitionTime);
    }
}
