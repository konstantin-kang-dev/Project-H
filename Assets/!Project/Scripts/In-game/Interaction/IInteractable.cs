using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IInteractable
{
    void Interact(Player player, IPickable pickable);
    bool CanInteract();
    void SetHighlight(bool value);
    void ResetAll();
}