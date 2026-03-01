using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

public interface ICustomWindow
{
    CustomWindowType WindowType { get; }
    event Action<bool> OnVisibilityChange;
    void SetVisibility(bool visible, bool doInstantly);
    void Clear();
}