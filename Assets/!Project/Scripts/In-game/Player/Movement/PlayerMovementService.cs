using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

public class PlayerMovementService : NetworkBehaviour
{
    CharacterController _characterController;

    PlayerStatsConfig _playerStats;

    [SerializeField] float _crouchingSpeedMultiplier = 0.5f;
    [SerializeField] float _sprintSpeedMultiplier = 1.5f;
    [SerializeField] float _acceleration = 1f;
    float _verticalVelocity;
    const float Gravity = -9.81f;
    bool _isJumpedBuffer = false;
    bool _isLanded = false;

    bool _canMove = true;

    bool _isSprintingPressed = false;
    bool _isSprintingLocal = false;
    readonly SyncVar<bool> _isSprinting = new SyncVar<bool>();

    bool _isCrouchingLocal = false;
    readonly SyncVar<bool> _isCrouching = new SyncVar<bool>();

    Vector2 _localInput = Vector2.zero;
    Vector2 _targetInputs = Vector2.zero;
    readonly SyncVar<Vector2> _currentInputs = new SyncVar<Vector2>();
    public Vector2 CurrentInputs => _currentInputs.Value;

    float _currentRotationYLocal = 0f;
    float _targetRotationYLocal = 0f;
    float _rotationYLocalVelocity = 0f;

    public event Action<Vector2, bool> OnWalk;
    public event Action OnJump;
    public event Action OnLand;
    public event Action<bool> OnSprintChange;
    public event Action<bool> OnSprint;
    public event Action<bool> OnCrouchingChange;

    public bool IsInitialized { get; private set; } = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            _isSprinting.OnChange += HandleIsSprintingChange;
            _isCrouching.OnChange += HandleIsCrouchingChange;
            _currentInputs.OnChange += HandleInputsChange;
        }
    }
    public override void OnStopClient()
    {
        base.OnStopClient();

        if (!IsOwner)
        {
            _isSprinting.OnChange -= HandleIsSprintingChange;
            _isCrouching.OnChange -= HandleIsCrouchingChange;
            _currentInputs.OnChange -= HandleInputsChange;
        }
    }

    public void Init(PlayerStatsConfig playerStats, CharacterController characterController)
    {
        _playerStats = playerStats;

        if (IsOwner)
        {
            GlobalInputManager.Input.OnMove += HandleMoveInput;
            GlobalInputManager.Input.OnJump += HandleJumpInput;
            GlobalInputManager.Input.OnSprint += HandleSprintInput;
            GlobalInputManager.Input.OnCrouchToggle += HandleCrouchToggle;
        }

        _characterController = characterController;

        IsInitialized = true;
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            GlobalInputManager.Input.OnMove -= HandleMoveInput;
            GlobalInputManager.Input.OnJump -= HandleJumpInput;
            GlobalInputManager.Input.OnSprint -= HandleSprintInput;
            GlobalInputManager.Input.OnCrouchToggle -= HandleCrouchToggle;
        }
    }


    private void Update()
    {
        if (!IsInitialized) return;

        if (GameManager.Instance.GameState != GameState.Started) return;

        UpdateInputs();

        if (IsOwner)
        {
            if (_canMove)
            {
                HandleSprint();
                Move();
                HandleLanding();
            }

            Rotate();
        }
    }


    public void SetMoveAbility(bool value)
    {
        _canMove = value;
    }

    void UpdateInputs()
    {
        _localInput = Vector2.MoveTowards(_localInput, _targetInputs, _acceleration * Time.deltaTime);
    }

    void Move()
    {
        Vector3 forceDirection = _localInput.x * transform.right + _localInput.y * transform.forward;
        Vector3 targetVel = forceDirection * _playerStats.MoveSpeed;

        if (_isSprintingLocal && _targetInputs.y > 0)
            targetVel *= _sprintSpeedMultiplier;
        else if (_isCrouchingLocal)
            targetVel *= _crouchingSpeedMultiplier;

        ApplyGravity();
        targetVel.y = _verticalVelocity;
        //_rb.linearVelocity = targetVel;
        _characterController.Move(targetVel * Time.deltaTime);

        OnWalk?.Invoke(_localInput, _isSprintingLocal);
    }
    private void ApplyGravity()
    {
        if (_characterController.isGrounded)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }

        if (_isJumpedBuffer)
        {
            _isJumpedBuffer = false;
            _verticalVelocity += _playerStats.JumpPower;
        }
    }

    void Jump()
    {
        if (!_characterController.isGrounded) return;

        _isJumpedBuffer = true;

        OnJump?.Invoke();
    }

    void HandleLanding()
    {
        bool oldValue = _isLanded;
        _isLanded = _characterController.isGrounded;

        if(!oldValue && _isLanded)
        {
            OnLand?.Invoke();
        }
    }

    void HandleSprint()
    {
        bool oldValue = _isSprintingLocal;
        _isSprintingLocal = _isSprintingPressed && _targetInputs.y > 0;

        if (!oldValue && _isSprintingLocal)
        {
            SetCrouchingState(false);
            OnSprint?.Invoke(true);

            RPC_RequestSetIsSprinting(_isSprintingLocal);
        }
        else if (oldValue && !_isSprintingLocal)
        {
            _isSprintingLocal = false;
            OnSprint?.Invoke(false);
            
            RPC_RequestSetIsSprinting(_isSprintingLocal);
        }
    }

    public void HandleMoveInput(Vector2 input)
    {
        if (!_canMove) return;

        _targetInputs = input;
        RPC_RequestSetMoveInputs(_targetInputs);
    }

    public void HandleSprintInput(bool value)
    {
        if (!_canMove) return;

        SetSprintState(value);
    }
    public void HandleJumpInput()
    {
        if (!_canMove) return;

        if (_isCrouchingLocal)
        {
            SetCrouchingState(false);
        }

        Jump();
    }

    public void HandleCrouchToggle()
    {
        if (!_canMove) return;
        if (!_isLanded) return;

        bool value = !_isCrouchingLocal;

        if (value)
        {
            SetSprintState(false);
        }
        SetCrouchingState(value);
    }

    [ServerRpc]
    void RPC_RequestSetMoveInputs(Vector2 inputs)
    {
        _currentInputs.Value = inputs;
    }

    [Client]
    void HandleInputsChange(Vector2 prev, Vector2 next, bool asServer)
    {
        if (asServer) return;
        if (IsOwner) return;

        OnWalk?.Invoke(next, _isSprinting.Value);
    }

    [ServerRpc]
    void RPC_RequestSetIsSprinting(bool value)
    {
        _isSprinting.Value = value;
    }

    [ServerRpc]
    void RPC_RequestSetIsCrouching(bool value)
    {
        _isCrouching.Value = value;
    }

    [Client]
    void HandleIsSprintingChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;
        if (IsOwner) return;

        OnSprintChange?.Invoke(next);
    }

    [Client]
    void HandleIsCrouchingChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;
        if (IsOwner) return;

        OnCrouchingChange?.Invoke(next);
    }
    
    public void Rotate()
    {
        _currentRotationYLocal = Mathf.SmoothDamp(_currentRotationYLocal, _targetRotationYLocal, ref _rotationYLocalVelocity, 0.05f);
        _characterController.transform.rotation = Quaternion.Euler(0, _currentRotationYLocal, 0);
        //_rb.MoveRotation();
    }

    public void UpdateRotation(Vector2 rotations)
    {
        _targetRotationYLocal = rotations.y;
    }

    public void SetSprintState(bool isSprinting)
    {
        _isSprintingPressed = isSprinting;

        OnSprintChange?.Invoke(_isSprintingPressed);
    }

    public void SetCrouchingState(bool value)
    {
        _isCrouchingLocal = value;

        RPC_RequestSetIsCrouching(_isCrouchingLocal);
        OnCrouchingChange?.Invoke(_isCrouchingLocal);

    }

}
