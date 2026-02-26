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

        playerControls.Player.Move.ApplyBindingOverride(1, keyboardKey + save.MoveForwardBind);
        playerControls.Player.Move.ApplyBindingOverride(3, keyboardKey + save.MoveBackwardBind);
        playerControls.Player.Move.ApplyBindingOverride(5, keyboardKey + save.MoveLeftBind);
        playerControls.Player.Move.ApplyBindingOverride(7, keyboardKey + save.MoveRightBind);

        playerControls.Player.Sprint.ApplyBindingOverride(0, keyboardKey + save.SprintBind);
        playerControls.Player.Jump.ApplyBindingOverride(0, keyboardKey + save.JumpBind);
        playerControls.Player.Crouch.ApplyBindingOverride(0, keyboardKey + save.CrouchBind);
        playerControls.Player.Interact.ApplyBindingOverride(0, keyboardKey + save.InteractBind);

    }
}