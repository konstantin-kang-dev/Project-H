using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultInput : IInput
{
    public Vector2 CurrentMoveInput => GetInputMove();
    public Vector2 CurrentLookInput => GetInputLook();

    PlayerControls _playerControls;

    public DefaultInput()
    {
        _playerControls = new PlayerControls();
        _playerControls.Enable();
    }
    
    Vector2 GetInputMove()
    {
        return _playerControls.Player.Move.ReadValue<Vector2>();
    }
    Vector2 GetInputLook()
    {
        return _playerControls.Player.Look.ReadValue<Vector2>();
    }
}