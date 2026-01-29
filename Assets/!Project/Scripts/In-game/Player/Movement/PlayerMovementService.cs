using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementService : MonoBehaviour
{
    Player _player;

    IInput _input;

    Rigidbody _rb;
    CapsuleCollider _capsuleCollider;

    PlayerStatsConfig _playerStats;

    [SerializeField] float _sprintSpeedMultiplier = 1.5f;
    [SerializeField] float _acceleration = 1f;
    bool _isWalking = false;
    bool _isSprinting = false;
    Vector2 _currentInputs = Vector2.zero;

    float _currentRotationY = 0f;
    float _targetRotationY = 0f;
    float _rotationYVelocity = 0f;


    public event Action<Vector2> OnWalkStart;
    public event Action<Vector2> OnWalkUpdate;
    public event Action<Vector2> OnWalkStop;

    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        
    }

    public void Init(Player player, PlayerStatsConfig playerStats, Rigidbody rb, CapsuleCollider capsuleCollider)
    {
        _player = player;

        _playerStats = playerStats;

        _rb = rb;
        _capsuleCollider = capsuleCollider;

        if(_player.IsOwner)
        {
            _input = new DefaultInput();
            IsInitialized = true;
        }
    }

    private void Update()
    {
        Rotate(_targetRotationY);
        if (!IsInitialized) return;

        Move(_input.CurrentMoveInput);
        //SpeedControl();
    }

    void Move(Vector2 input)
    {
        if (_isSprinting)
        {
            input.y *= _sprintSpeedMultiplier;
        }

        _currentInputs = Vector2.MoveTowards(_currentInputs, input, _acceleration);
        
        Vector3 forceDirection = _currentInputs.x * transform.right + _currentInputs.y * transform.forward;

        Vector3 targetVel = forceDirection * _playerStats.MoveSpeed;
        Vector3 currentVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 velDiff = targetVel - currentVel;

        _rb.AddForce(velDiff * 500f, ForceMode.Force);

        bool prevWalkState = _isWalking;
        SetWalkingState(_currentInputs.x != 0 && _currentInputs.y != 0);
        SetSprintState(_input.IsSprinting());

        if (!prevWalkState && _isWalking)
        {
            OnWalkStart?.Invoke(_currentInputs);
        }

        OnWalkUpdate?.Invoke(_currentInputs);
    }
    
    public void Rotate(float rotationY)
    {
        if (_player.IsOwner)
        {
            _rb.transform.rotation = Quaternion.Euler(0, rotationY, 0);
        }
        else
        {
            float smoothSpeed = 3.5f;
            if(_isWalking && !_isSprinting)
            {
                smoothSpeed = 10f;
            }else if (_isSprinting)
            {
                smoothSpeed = 15f;
            }
            _currentRotationY = Mathf.Lerp(_currentRotationY, rotationY, smoothSpeed * Time.deltaTime);

            _rb.transform.rotation = Quaternion.Euler(0, _currentRotationY, 0);
        }

    }

    public void UpdateRotation(Vector2 rotations)
    {
        _targetRotationY = rotations.y;
    }

    public void SetWalkingState(bool isWalking)
    {
        _isWalking = isWalking;

        if (_player.IsOwner)
        {
            _player.RPC_RequestSetWalkingState(_isWalking);
        }
    }
    public void SetSprintState(bool isSprinting)
    {
        _isSprinting = isSprinting;

        if (_player.IsOwner)
        {
            _player.RPC_RequestSetSprintingState(_isSprinting);
        }
    }
}
