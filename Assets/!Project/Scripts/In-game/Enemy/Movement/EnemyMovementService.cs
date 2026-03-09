using FishNet.Object;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementService : MonoBehaviour
{
    EnemyStatsConfig _enemyStats;
    NavMeshAgent _navMeshAgent;

    Transform _target;
    bool _isFollowingPlayer = false;

    public event Action<bool, Transform> OnMove;
    public event Action<Player> OnReachedPlayer;
    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        _navMeshAgent = GetComponentInParent<NavMeshAgent>();
        _navMeshAgent.enabled = false;
    }
    public void Init(EnemyStatsConfig enemyStats)
    {
        _enemyStats = enemyStats;

        UpdateSpeed(_enemyStats.MoveSpeed);
        _navMeshAgent.enabled = true;

        IsInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        UpdateDestination();
    }

    void UpdateDestination()
    {
        if (_navMeshAgent.isStopped) return;

        if (_target != null)
        {
            _navMeshAgent.SetDestination(_target.position);
        }

        if (_navMeshAgent.velocity != Vector3.zero)
        {
            OnMove?.Invoke(_isFollowingPlayer, _target);
        }

        if(_isFollowingPlayer && _navMeshAgent.remainingDistance <= 1f)
        {
            OnReachedPlayer?.Invoke(_target.GetComponent<Player>());
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        if(_target == null)
        {
            _isFollowingPlayer = false;
            UpdateSpeed(_enemyStats.MoveSpeed);
            return;
        }

        if(target.TryGetComponent<Player>(out Player player))
        {
            _isFollowingPlayer = true;
            UpdateSpeed(_enemyStats.SprintSpeed);
        }
        else
        {
            _isFollowingPlayer = false;
            UpdateSpeed(_enemyStats.MoveSpeed);
        }
    }

    public void UpdateSpeed(float value)
    {
        _navMeshAgent.speed = value;
    }

    public void SetMoveAbility(bool canMove)
    {
        _navMeshAgent.isStopped = !canMove;
        _navMeshAgent.velocity = Vector3.zero;
    }

    public void HandleKillPlayer(Player player)
    {

        SetMoveAbility(false);
    }

    public bool IsReachedTarget()
    {
        if(_target == null) return true;

        return Vector3.Distance(transform.position, _target.position) < 0.5f;
    }

    private void OnDrawGizmos()
    {
        if(_target == null) return;

        Gizmos.color = Color.purple;
        Gizmos.DrawLine(transform.position, _target.position);
        Gizmos.DrawSphere(_target.position, 0.5f);
    }
}