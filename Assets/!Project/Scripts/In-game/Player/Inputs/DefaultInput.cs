using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultInput : IInput
{
    public Vector2 CurrentMoveInput => GetInputMove();
    public Vector2 CurrentLookInput => GetInputLook();

    #region INPUT_EVENTS
    public event Action OnInteract;
    public event Action OnDrop;
    public event Action OnNextInventorySlot;
    public event Action OnPreviousInventorySlot;
    #endregion

    PlayerControls _playerControls;

    public DefaultInput()
    {
        _playerControls = new PlayerControls();
        _playerControls.Enable();

        _playerControls.Player.Interact.performed += HandleInteractButtonClick;
        _playerControls.Player.Drop.performed += HandleDropButtonClick;

        _playerControls.Player.NextInventorySlot.performed += HandleNextInventorySlotTrigger;
        _playerControls.Player.PreviousInventorySlot.performed += HandlePreviousInventorySlotTrigger;
    }

    #region INPUT_HANDLERS
    Vector2 GetInputMove()
    {
        return _playerControls.Player.Move.ReadValue<Vector2>();
    }
    Vector2 GetInputLook()
    {
        return _playerControls.Player.Look.ReadValue<Vector2>();
    }
    void HandleInteractButtonClick(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }
    void HandleDropButtonClick(InputAction.CallbackContext context)
    {
        OnDrop?.Invoke();
    }
    void HandleNextInventorySlotTrigger(InputAction.CallbackContext context)
    {
        OnNextInventorySlot?.Invoke();
    }
    void HandlePreviousInventorySlotTrigger(InputAction.CallbackContext context)
    {
        OnPreviousInventorySlot?.Invoke();
    }
    #endregion

    public bool IsSprinting()
    {
        return _playerControls.Player.Sprint.IsPressed();
    }
    public void Dispose()
    {
        _playerControls.Player.Interact.performed -= HandleInteractButtonClick;
        _playerControls.Disable();
        _playerControls.Dispose();
    }
}