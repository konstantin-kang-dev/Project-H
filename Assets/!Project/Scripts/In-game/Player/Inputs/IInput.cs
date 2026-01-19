using System;
using UnityEditor;
using UnityEngine;

public interface IInput
{
    public Vector2 CurrentMoveInput { get;}
    public Vector2 CurrentLookInput { get;}
}