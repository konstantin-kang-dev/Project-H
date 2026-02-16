using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IInteractable
{
    InteractableConfig Config { get; }
    Transform Transform { get; }
    bool InteractionState { get; }
    void Interact(IPickable pickable);
    bool CanInteract();
    void SetHighlight(bool value);
    void ResetAll();
    event Action<IInteractable, bool> OnInteractStateChange;
}