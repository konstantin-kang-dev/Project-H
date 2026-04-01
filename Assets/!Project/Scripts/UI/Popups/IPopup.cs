using UnityEngine;

public interface IPopup
{
    bool IsVisible { get; }
    void SetVisibility(bool visible, bool doInstantly);
    void SetMessage(string message);
}
