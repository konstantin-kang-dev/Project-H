using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementService : MonoBehaviour
{
    IInput _input;

    Rigidbody _rb;
    CapsuleCollider _capsuleCollider;

    PlayerStats _playerStats;

    [SerializeField] float _sprintSpeedMultiplier = 1.5f;
    [SerializeField] float _acceleration = 1f;
    bool _isWalking = false;
    Vector2 _currentInputs = Vector2.zero;

    bool _isSprinting => !IsInitialized ? false : _input.IsSprinting();

    public event Action<Vector2> OnWalkStart;
    public event Action<Vector2> OnWalkUpdate;
    public event Action<Vector2> OnWalkStop;

    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        
    }

    public void Init(PlayerStats playerStats, Rigidbody rb, CapsuleCollider capsuleCollider)
    {
        _playerStats = playerStats;

        _input = new DefaultInput();

        _rb = rb;
        _capsuleCollider = capsuleCollider;

        IsInitialized = true;
    }

    private void Update()
    {
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

        _rb.AddForce(velDiff * 10f, ForceMode.Force);

        bool prevWalkState = _isWalking;
        _isWalking = _currentInputs.x != 0 && _currentInputs.y != 0;

        if (!prevWalkState && _isWalking)
        {
            OnWalkStart?.Invoke(_currentInputs);
        }

        OnWalkUpdate?.Invoke(_currentInputs);
    }
    
    public void Rotate(Vector2 rotations)
    {
        _rb.transform.rotation = Quaternion.Euler(0, rotations.y, 0);
    }

}
