using System;
using UnityEditor;
using UnityEngine;

public interface IInput
{
    Vector2 CurrentMoveInput { get;}
    Vector2 CurrentLookInput { get;}

    event Action<Vector2> OnMove;
    event Action<Vector2> OnLook;
    event Action<bool> OnSprint;
    event Action OnInteract;
    event Action OnDrop; 
    event Action OnNextInventorySlot;
    event Action OnPreviousInventorySlot;
    event Action OnInteractWithItem;
    event Action OnOpenChat;
}