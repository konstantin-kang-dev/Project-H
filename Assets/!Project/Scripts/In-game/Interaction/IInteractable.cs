using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IInteractable
{
    List<IPickable> RequiredPickables { get; }
    void Interact();
    bool CanInteract();
    void SetHighlight(bool value);
}