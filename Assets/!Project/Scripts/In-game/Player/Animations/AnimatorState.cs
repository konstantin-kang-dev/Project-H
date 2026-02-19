using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum AnimatorState
{
    None = 0,
    Idle = 10,
    Walk = 20,
    Crouch = 30,
    Sprint = 40,
    Jump = 100,
    KnockDown = 200,
}