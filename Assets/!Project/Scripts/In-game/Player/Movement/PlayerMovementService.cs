using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementService : MonoBehaviour
{
    DefaultInput _input;

    Rigidbody _rb;
    CapsuleCollider _capsuleCollider;

    PlayerStats _playerStats;

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
        Vector3 forceDirection = input.x * transform.right + input.y * transform.forward;

        Vector3 targetVel = forceDirection * _playerStats.MoveSpeed;
        Vector3 currentVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 velDiff = targetVel - currentVel;

        _rb.AddForce(velDiff * 2f, ForceMode.Force);
    }
    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        if (flatVel.magnitude > _playerStats.MoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * _playerStats.MoveSpeed;
            _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
        }
    }
}
