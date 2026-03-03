using Sirenix.OdinInspector;
using System;
using UnityEngine;

[RequireComponent(typeof(BasicWindowVisuals))]
public class BasicCustomWindow : SerializedMonoBehaviour, ICustomWindow
{
    [field: SerializeField] public CustomWindowType WindowType { get; private set; }
    protected BasicWindowVisuals _windowVisuals;
    public bool IsVisible { get; private set; } = false;

    public event Action<bool> OnVisibilityChange;

    public virtual void SetVisibility(bool visible, bool doInstantly)
    {
        if (_windowVisuals == null) SetupComponents();

        if (visible)
        {
            _windowVisuals.ProcessInAnimation(doInstantly);
            BindControls();
            //Debug.Log($"[BasicCustomWindow | {WindowType}] Showed window");
        }
        else
        {
            _windowVisuals.ProcessOutAnimation(doInstantly);
            UnbindControls();
            //Debug.Log($"[BasicCustomWindow | {WindowType}] Hidden window");

        }

        IsVisible = visible;

        OnVisibilityChange?.Invoke(visible);
    }

    void SetupComponents()
    {
        _windowVisuals = GetComponent<BasicWindowVisuals>();
    }

    protected virtual void BindControls()
    {
        GlobalInputManager.Input.OnEscPressed += HandleEscPressed;
    }

    protected virtual void UnbindControls()
    {
        GlobalInputManager.Input.OnEscPressed -= HandleEscPressed;
    }

    void HandleEscPressed()
    {
        SetVisibility(false, false);
        WindowsNavigator.Instance.GoBack();
    }

    public void Clear()
    {
        UnbindControls();
    }

}
