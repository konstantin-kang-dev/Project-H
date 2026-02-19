using System;
using UnityEditor;
using UnityEngine;

public interface IInput
{
    Vector2 CurrentMoveInput { get;}
    Vector2 CurrentLookInput { get;}

    event Action<Vector2> OnMove;
    event Action<Vector2> OnLook;
    event Action OnJump;
    event Action<bool> OnSprint; 
    event Action OnCrouchToggle;
    event Action OnInteract;
    event Action OnDrop; 
    event Action<int> OnInventorySlotKey;
    event Action OnNextInventorySlot;
    event Action OnPreviousInventorySlot;
    event Action OnInteractWithItem;
    event Action OnOpenChat;
}