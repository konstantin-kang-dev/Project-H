using UnityEngine;

public class GameplayUI : BasicCustomWindow
{
    public override void SetVisibility(bool visible, bool doInstantly)
    {
        base.SetVisibility(visible, doInstantly);

        if (visible)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    protected override void BindControls()
    {
        GlobalInputManager.Input.OnEscPressed += HandleEscPressed;
    }

    protected override void UnbindControls()
    {
        GlobalInputManager.Input.OnEscPressed -= HandleEscPressed;
    }

    void HandleEscPressed()
    {
        WindowsNavigator.Instance.OpenWindow(CustomWindowType.GameMenu);
    }
}
