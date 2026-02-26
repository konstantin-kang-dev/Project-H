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
    public event Action OnJump;
    public event Action<bool> OnSprint;
    public event Action OnCrouchToggle;
    public event Action OnInteract;
    public event Action OnDrop;
    public event Action<int> OnInventorySlotKey;
    public event Action OnNextInventorySlot;
    public event Action OnPreviousInventorySlot;
    public event Action OnInteractWithItem;
    public event Action OnOpenChat;
    #endregion

    public PlayerControls PlayerControls { get; private set; }

    public DefaultInput()
    {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.Player.Interact.performed += HandleInteractButtonClick;
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
        OnSprint?.Invoke(PlayerControls.Player.Sprint.IsPressed());
    }
    void HandleJumpInput(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
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

    void HandleInventorySlotKey1(InputAction.CallbackContext context)
    {
        OnInventorySlotKey?.Invoke(0);
    }

    void HandleInventorySlotKey2(InputAction.CallbackContext context)
    {
        OnInventorySlotKey?.Invoke(1);
    }

    void HandleInventorySlotKey3(InputAction.CallbackContext context)
    {
        OnInventorySlotKey?.Invoke(2);
    }

    void HandleInventorySlotKey4(InputAction.CallbackContext context)
    {
        OnInventorySlotKey?.Invoke(3);
    }

    void HandleInventorySlotKey5(InputAction.CallbackContext context)
    {
        OnInventorySlotKey?.Invoke(4);
    }

    #endregion

    public void Dispose()
    {
        PlayerControls.Player.Interact.performed -= HandleInteractButtonClick;
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