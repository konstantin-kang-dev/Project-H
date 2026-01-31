using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IInteractable
{
    InteractableConfig Config { get; }
    Transform Transform { get; }
    void Interact(Player player, IPickable pickable);
    bool CanInteract();
    void SetHighlight(bool value);
    void ResetAll();
    event Action<IInteractable, bool> OnInteractStateChange;
}