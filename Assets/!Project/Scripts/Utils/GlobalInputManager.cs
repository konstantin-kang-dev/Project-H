using UnityEngine;
using System.Collections;
using Saves;
using UnityEngine.InputSystem;

public class GlobalInputManager
{
    public static IInput Input;

    public static bool IsInitialized { get; private set;  } = false;
    public static void Init()
    {
        Input = new DefaultInput();
        IsInitialized = true;
    }

    public static void ApplySave(ControlsSave save)
    {
        if (!IsInitialized) return;
        string keyboardKey = "<Keyboard>/";
        PlayerControls playerControls = Input.PlayerControls;

        playerControls.Player.Move.ApplyBindingOverride(1, keyboardKey + save.GetBind(InputActionType.MoveForward));
        playerControls.Player.Move.ApplyBindingOverride(3, keyboardKey + save.GetBind(InputActionType.MoveBackward));
        playerControls.Player.Move.ApplyBindingOverride(5, keyboardKey + save.GetBind(InputActionType.MoveLeft));
        playerControls.Player.Move.ApplyBindingOverride(7, keyboardKey + save.GetBind(InputActionType.MoveRight));

        playerControls.Player.Sprint.ApplyBindingOverride(0, keyboardKey + save.GetBind(InputActionType.Sprint));
        playerControls.Player.Jump.ApplyBindingOverride(0, keyboardKey + save.GetBind(InputActionType.Jump));
        playerControls.Player.Crouch.ApplyBindingOverride(0, keyboardKey + save.GetBind(InputActionType.Crouch));
        playerControls.Player.Interact.ApplyBindingOverride(0, keyboardKey + save.GetBind(InputActionType.Interact));

    }
}