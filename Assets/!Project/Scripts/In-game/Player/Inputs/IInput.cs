using System;
using UnityEditor;
using UnityEngine;

public interface IInput
{
    Vector2 CurrentMoveInput { get;}
    Vector2 CurrentLookInput { get;}

    event Action OnInteract;
    event Action OnDrop; 
    event Action OnNextInventorySlot;
    event Action OnPreviousInventorySlot;

    bool IsSprinting();
}