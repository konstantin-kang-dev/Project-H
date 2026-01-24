using System;
using UnityEditor;
using UnityEngine;


[Serializable]
public enum GameState
{
    PreparingToStart = 0,
    Started = 1,
    Ended = 2,
}