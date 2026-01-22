using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum WindowAnimationType
{
    Fade = 0,
    SwipeLeft = 1,
    SwipeRight = 2,
    SwipeDown = 3,
    PopUp = 4,
}

[Serializable]
public enum WindowAnimationDirection
{
    In = 0,
    Out = 1,
}