using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementService : MonoBehaviour
{
    NavMeshAgent _navMeshAgent;

    Transform _target;
    bool _isFollowingPlayer = false;

    public bool IsInitialized { get; private set; } = false;
    public void Init()
    {
        _navMeshAgent = GetComponentInParent<NavMeshAgent>();

        IsInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        if(_target != null)
        {
            _navMeshAgent.SetDestination(_target.position);
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        if(target.TryGetComponent<Player>(out Player player))
        {
            _isFollowingPlayer = true;
        }
    }

    public void UpdateSpeed(float value)
    {
        _navMeshAgent.speed = value;
    }
}