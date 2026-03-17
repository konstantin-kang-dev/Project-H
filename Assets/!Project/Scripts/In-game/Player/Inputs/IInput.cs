using System;
using UnityEditor;
using UnityEngine;

public interface IInput
{
    PlayerControls PlayerControls { get; }
    bool IsLocked { get; }
    Vector2 CurrentMoveInput { get;}
    Vector2 CurrentLookInput { get;}

    void SetLock(bool value);

    event Action<Vector2> OnMove;
    event Action<Vector2> OnLook;
    event Action OnJump;
    event Action<bool> OnSprint; 
    event Action OnCrouchToggle;
    event Action OnInteract;
    event Action OnInteractReleased;
    event Action OnDrop; 
    event Action<int> OnInventorySlotKey;
    event Action OnNextInventorySlot;
    event Action OnPreviousInventorySlot;
    event Action OnInteractWithItem;
    event Action OnOpenChat;
    event Action OnEscPressed;
}

[Serializable]
public enum InputActionType
{
    None = -1,
    MoveForward = 0,
    MoveBackward = 1,
    MoveRight = 2,
    MoveLeft = 3,

    Sprint = 10,
    Jump = 11,
    Crouch = 12,
    
    Interact = 20,
    InteractItem = 21,
}