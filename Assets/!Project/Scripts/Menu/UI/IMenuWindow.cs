using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public interface IMenuWindow
{
    MenuWindowType WindowType { get; }
    void SetVisibility(bool visible, bool doInstantly);
}