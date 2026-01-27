using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultInput : IInput
{
    public Vector2 CurrentMoveInput => GetInputMove();
    public Vector2 CurrentLookInput => GetInputLook();

    public event Action OnInteract;

    PlayerControls _playerControls;

    public DefaultInput()
    {
        _playerControls = new PlayerControls();
        _playerControls.Enable();

        _playerControls.Player.Interact.performed += HandleInteract;
    }
    
    Vector2 GetInputMove()
    {
        return _playerControls.Player.Move.ReadValue<Vector2>();
    }
    Vector2 GetInputLook()
    {
        return _playerControls.Player.Look.ReadValue<Vector2>();
    }
    void HandleInteract(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }

    public bool IsSprinting()
    {
        return _playerControls.Player.Sprint.IsPressed();
    }
}