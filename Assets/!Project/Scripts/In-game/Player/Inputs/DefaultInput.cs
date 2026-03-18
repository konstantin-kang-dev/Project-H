using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultInput : IInput
{
    public bool IsLocked { get; private set; }
    public Vector2 CurrentMoveInput => GetInputMove();
    public Vector2 CurrentLookInput => GetInputLook();

    private float _lastInventoryInteraction = 0f;
    private const float INVENTORY_INTERACTION_COOLDOWN = 0.02f;
    #region INPUT_EVENTS
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action OnJump;
    public event Action<bool> OnSprint;
    public event Action OnCrouchToggle;
    public event Action OnInteract;
    public event Action OnInteractReleased;
    public event Action OnDrop;
    public event Action<int> OnInventorySlotKey;
    public event Action OnNextInventorySlot;
    public event Action OnPreviousInventorySlot;
    public event Action OnInteractWithItem;
    public event Action OnOpenChat;
    public event Action OnEscPressed;
    #endregion

    public PlayerControls PlayerControls { get; private set; }

    public DefaultInput()
    {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.Player.Interact.performed += HandleInteractButtonPress;
        PlayerControls.Player.Interact.canceled += HandleInteractButtonRelease;
        PlayerControls.Player.Drop.performed += HandleDropButtonClick;

        PlayerControls.Player.NextInventorySlot.performed += HandleNextInventorySlotTrigger;
        PlayerControls.Player.PreviousInventorySlot.performed += HandlePreviousInventorySlotTrigger;

        PlayerControls.Player.Jump.performed += HandleJumpInput;

        PlayerControls.Player.Sprint.performed += HandleSprintInput;
        PlayerControls.Player.Sprint.canceled += HandleSprintInput;

        PlayerControls.Player.Crouch.performed += HandleCrouchInput;

        PlayerControls.Player.Move.performed += HandleMoveInput;
        PlayerControls.Player.Move.canceled += HandleMoveInput;

        PlayerControls.Player.Look.performed += HandleLookInput;
        PlayerControls.Player.Look.canceled += HandleLookInput;

        PlayerControls.Player.InteractWithItem.performed += HandleInteractWithItemButtonClick;
        PlayerControls.Player.OpenChat.performed += HandleOpenChatButtonClick;

        PlayerControls.Player.InventorySlot1.performed += HandleInventorySlotKey1;
        PlayerControls.Player.InventorySlot2.performed += HandleInventorySlotKey2;
        PlayerControls.Player.InventorySlot3.performed += HandleInventorySlotKey3;
        PlayerControls.Player.InventorySlot4.performed += HandleInventorySlotKey4;
        PlayerControls.Player.InventorySlot5.performed += HandleInventorySlotKey5;

        PlayerControls.Player.Esc.performed += HandleEscPress;
    
    
    }

    public void SetLock(bool value)
    {
        IsLocked = value;
    }

    #region INPUT_HANDLERS
    Vector2 GetInputMove()
    {
        return PlayerControls.Player.Move.ReadValue<Vector2>();
    }
    Vector2 GetInputLook()
    {
        return PlayerControls.Player.Look.ReadValue<Vector2>();
    }
    void HandleMoveInput(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        Vector2 moveInput = context.ReadValue<Vector2>();
        OnMove?.Invoke(moveInput);
    }
    void HandleLookInput(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        Vector2 lookInput = context.ReadValue<Vector2>();
        OnLook?.Invoke(lookInput);
    }
    void HandleSprintInput(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnSprint?.Invoke(PlayerControls.Player.Sprint.IsPressed());
    }
    void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnJump?.Invoke();
    }
    void HandleCrouchInput(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnCrouchToggle?.Invoke();
    }
    void HandleInteractButtonPress(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnInteract?.Invoke();
    }
    void HandleInteractButtonRelease(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnInteractReleased?.Invoke();
    }
    void HandleDropButtonClick(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnDrop?.Invoke();
    }
    private void HandleNextInventorySlotTrigger(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        if (Time.time - _lastInventoryInteraction >= INVENTORY_INTERACTION_COOLDOWN)
        {
            _lastInventoryInteraction = Time.time;
            OnNextInventorySlot?.Invoke();
        }
    }
    private void HandlePreviousInventorySlotTrigger(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        if (Time.time - _lastInventoryInteraction >= INVENTORY_INTERACTION_COOLDOWN)
        {
            _lastInventoryInteraction = Time.time;
            OnPreviousInventorySlot?.Invoke();
        }
    }
    void HandleInteractWithItemButtonClick(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnInteractWithItem?.Invoke();
    }
    void HandleOpenChatButtonClick(InputAction.CallbackContext context)
    {
        OnOpenChat?.Invoke();
    }

    void HandleInventorySlotKey1(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnInventorySlotKey?.Invoke(0);
    }

    void HandleInventorySlotKey2(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnInventorySlotKey?.Invoke(1);
    }

    void HandleInventorySlotKey3(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnInventorySlotKey?.Invoke(2);
    }

    void HandleInventorySlotKey4(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnInventorySlotKey?.Invoke(3);
    }

    void HandleInventorySlotKey5(InputAction.CallbackContext context)
    {
        if (IsLocked) return;
        OnInventorySlotKey?.Invoke(4);
    }

    void HandleEscPress(InputAction.CallbackContext context)
    {
        OnEscPressed?.Invoke();
    }

    #endregion

    public void Dispose()
    {
        PlayerControls.Player.Interact.performed -= HandleInteractButtonPress;
        PlayerControls.Player.Drop.performed -= HandleDropButtonClick;

        PlayerControls.Player.NextInventorySlot.performed -= HandleNextInventorySlotTrigger;
        PlayerControls.Player.PreviousInventorySlot.performed -= HandlePreviousInventorySlotTrigger;

        PlayerControls.Player.Sprint.started -= HandleSprintInput;
        PlayerControls.Player.Sprint.canceled -= HandleSprintInput;

        PlayerControls.Player.Move.performed -= HandleMoveInput;
        PlayerControls.Player.Move.canceled -= HandleMoveInput;

        PlayerControls.Player.Look.performed -= HandleLookInput;
        PlayerControls.Player.Look.canceled -= HandleLookInput;

        PlayerControls.Disable();
        PlayerControls.Dispose();
    }
}