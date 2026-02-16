using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum AnimatorState
{
    None = -1,
    Idle = 0,
    Walk = 1,
    Sprint = 2,
    Crouch = 3,
    Kill = 4,
    KnockDown = 5,
}