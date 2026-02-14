using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultInput : IInput
{
    public Vector2 CurrentMoveInput => GetInputMove();
    public Vector2 CurrentLookInput => GetInputLook();

    private float _lastInventoryInteraction = 0f;
    private const float INVENTORY_INTERACTION_COOLDOWN = 0.03f;
    #region INPUT_EVENTS
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action<bool> OnSprint;
    public event Action OnCrouchToggle;
    public event Action OnInteract;
    public event Action OnDrop;
    public event Action OnNextInventorySlot;
    public event Action OnPreviousInventorySlot;
    public event Action OnInteractWithItem;
    public event Action OnOpenChat;
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

        _playerControls.Player.Sprint.performed += HandleSprintInput;
        _playerControls.Player.Sprint.canceled += HandleSprintInput;

        _playerControls.Player.Crouch.performed += HandleCrouchInput;

        _playerControls.Player.Move.performed += HandleMoveInput;
        _playerControls.Player.Move.canceled += HandleMoveInput;

        _playerControls.Player.Look.performed += HandleLookInput;
        _playerControls.Player.Look.canceled += HandleLookInput;

        _playerControls.Player.InteractWithItem.performed += HandleInteractWithItemButtonClick;
        _playerControls.Player.OpenChat.performed += HandleOpenChatButtonClick;
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
    void HandleMoveInput(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        OnMove?.Invoke(moveInput);
    }
    void HandleLookInput(InputAction.CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();
        OnLook?.Invoke(lookInput);
    }
    void HandleSprintInput(InputAction.CallbackContext context)
    {
        OnSprint?.Invoke(_playerControls.Player.Sprint.IsPressed());
    }
    void HandleCrouchInput(InputAction.CallbackContext context)
    {
        OnCrouchToggle?.Invoke();
    }
    void HandleInteractButtonClick(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }
    void HandleDropButtonClick(InputAction.CallbackContext context)
    {
        OnDrop?.Invoke();
    }
    private void HandleNextInventorySlotTrigger(InputAction.CallbackContext context)
    {
        if (Time.time - _lastInventoryInteraction >= INVENTORY_INTERACTION_COOLDOWN)
        {
            _lastInventoryInteraction = Time.time;
            OnNextInventorySlot?.Invoke();
        }
    }
    private void HandlePreviousInventorySlotTrigger(InputAction.CallbackContext context)
    {
        if (Time.time - _lastInventoryInteraction >= INVENTORY_INTERACTION_COOLDOWN)
        {
            _lastInventoryInteraction = Time.time;
            OnPreviousInventorySlot?.Invoke();
        }
    }
    void HandleInteractWithItemButtonClick(InputAction.CallbackContext context)
    {
        OnInteractWithItem?.Invoke();
    }
    void HandleOpenChatButtonClick(InputAction.CallbackContext context)
    {
        OnOpenChat?.Invoke();
    }
    #endregion

    public void Dispose()
    {
        _playerControls.Player.Interact.performed -= HandleInteractButtonClick;
        _playerControls.Player.Drop.performed -= HandleDropButtonClick;

        _playerControls.Player.NextInventorySlot.performed -= HandleNextInventorySlotTrigger;
        _playerControls.Player.PreviousInventorySlot.performed -= HandlePreviousInventorySlotTrigger;

        _playerControls.Player.Sprint.started -= HandleSprintInput;
        _playerControls.Player.Sprint.canceled -= HandleSprintInput;

        _playerControls.Player.Move.performed -= HandleMoveInput;
        _playerControls.Player.Move.canceled -= HandleMoveInput;

        _playerControls.Player.Look.performed -= HandleLookInput;
        _playerControls.Player.Look.canceled -= HandleLookInput;

        _playerControls.Disable();
        _playerControls.Dispose();
    }
}