using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum AnimatorState
{
    None = -1,
    Idle = 0,
    Walk = 1,
    Run = 2,
}