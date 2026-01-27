using System;
using UnityEditor;
using UnityEngine;

public interface IInput
{
    Vector2 CurrentMoveInput { get;}
    Vector2 CurrentLookInput { get;}
    event Action OnInteract;
    bool IsSprinting();
}