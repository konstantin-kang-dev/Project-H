using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementService : NetworkBehaviour
{
    Rigidbody _rb;

    PlayerStatsConfig _playerStats;

    [SerializeField] float _sprintSpeedMultiplier = 1.5f;
    [SerializeField] float _acceleration = 1f;

    bool _isWalkingLocal = false;
    readonly SyncVar<bool> _isWalking = new SyncVar<bool>();
    bool _isSprintingLocal = false;

    readonly SyncVar<bool> _isSprinting = new SyncVar<bool>();

    Vector2 _localInput = Vector2.zero;
    Vector2 _targetInputs = Vector2.zero;
    readonly SyncVar<Vector2> _currentInputs = new SyncVar<Vector2>();
    public Vector2 CurrentInputs => _currentInputs.Value;

    float _currentRotationYLocal = 0f;
    float _targetRotationYLocal = 0f;
    readonly SyncVar<float> _targetRotationY = new SyncVar<float>();

    public event Action<Vector2> OnWalkStart;
    public event Action<Vector2> OnWalkUpdate;
    public event Action<Vector2> OnWalkStop;

    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            _isWalking.OnChange += HandleIsWalkingChange;
            _isSprinting.OnChange += HandleIsSprintingChange;
            _currentInputs.OnChange += HandleInputsChange;
        }
    }

    public void Init(PlayerStatsConfig playerStats, Rigidbody rb)
    {
        _playerStats = playerStats;

        _rb = rb;

        IsInitialized = true;
    }

    private void Update()
    {
        if (!IsInitialized) return;

        if (GameManager.Instance.GameState != GameState.Started) return;

        Rotate();
        if (IsOwner)
        {
            Move();
        }
    }

    void Move()
    {
        Vector2 targetInput = _targetInputs;

        if (_isSprintingLocal && _targetInputs.y > 0)
        {
            targetInput *= _sprintSpeedMultiplier;
        }

        _localInput = Vector2.MoveTowards(_localInput, targetInput, _acceleration * Time.deltaTime);

        Vector3 forceDirection = _localInput.x * transform.right + _localInput.y * transform.forward;

        Vector3 targetVel = forceDirection * _playerStats.MoveSpeed;
        Vector3 currentVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 velDiff = targetVel - currentVel;

        _rb.AddForce(velDiff * 500f, ForceMode.Force);

        RPC_RequestSetMoveInputs(_localInput);
        OnWalkUpdate?.Invoke(_localInput);
    }

    public void HandleMoveInput(Vector2 input)
    {
        _targetInputs = input;

        SetWalkingState(_targetInputs.x != 0 || _targetInputs.y != 0);
    }
    public void HandleSprintInput(bool value)
    {
        SetSprintState(value);
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

        OnWalkUpdate?.Invoke(next);
    }

    [ServerRpc]
    void RPC_RequestSetIsWalking(bool value)
    {
        _isWalking.Value = value;
    }

    [Client]
    void HandleIsWalkingChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;
        if (IsOwner) return;

        if(!prev && next)
        {
            OnWalkStart?.Invoke(CurrentInputs);
        }
    }

    [ServerRpc]
    void RPC_RequestSetIsSprinting(bool value)
    {
        _isSprinting.Value = value;
    }

    [Client]
    void HandleIsSprintingChange(bool prev, bool next, bool asServer)
    {
        if (asServer) return;
        if (IsOwner) return;

    }
    
    public void Rotate()
    {
        if (IsOwner)
        {
            _rb.transform.rotation = Quaternion.Euler(0, _targetRotationYLocal, 0);
        }
        else
        {
            float smoothSpeed = 5f;
            if(_isWalking.Value && !_isSprinting.Value)
            {
                smoothSpeed = 15f;
            }else if (_isSprinting.Value)
            {
                smoothSpeed = 20f;
            }
            _currentRotationYLocal = Mathf.Lerp(_currentRotationYLocal, _targetRotationY.Value, smoothSpeed * Time.deltaTime);

            _rb.transform.rotation = Quaternion.Euler(0, _currentRotationYLocal, 0);
        }

    }

    public void UpdateRotation(Vector2 rotations)
    {
        _targetRotationYLocal = rotations.y;
        RPC_RequestSetTargetYRotation(_targetRotationYLocal);
    }

    [ServerRpc]
    void RPC_RequestSetTargetYRotation(float rotationY)
    {
        _targetRotationY.Value = rotationY;
    }

    public void SetWalkingState(bool isWalking)
    {
        bool prevValue = _isWalkingLocal;
        _isWalkingLocal = isWalking;

        if (!prevValue && _isWalkingLocal)
        {
            OnWalkStart?.Invoke(_localInput);
        }

        RPC_RequestSetIsWalking(_isWalkingLocal);
    }
    public void SetSprintState(bool isSprinting)
    {
        _isSprintingLocal = isSprinting;

        RPC_RequestSetIsSprinting(_isSprintingLocal);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (!IsOwner)
        {
            _isWalking.OnChange -= HandleIsWalkingChange;
            _isSprinting.OnChange -= HandleIsSprintingChange;
            _currentInputs.OnChange -= HandleInputsChange;
        }
    }
}
