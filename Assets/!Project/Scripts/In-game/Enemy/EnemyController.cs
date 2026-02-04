using FishNet.Object;
using System;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] EnemyStatsConfig _enemyStats;

    [SerializeField] EnemyMovementService _enemyMovementService;


    [SerializeField] EnemyVisuals _enemyVisuals;

    [SerializeField] AggroController _aggroControllerPrefab;
    AggroController _aggroController;

    EnemyState _currentState = EnemyState.None;

    public event Action<EnemyState> OnStateUpdate;
    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        Init();
    }

    public void Init()
    {
        _enemyMovementService.Init(_enemyStats);

        _enemyVisuals.Init();

        OnStateUpdate += _enemyVisuals.HandleStateUpdate;

        _enemyMovementService.OnMove += _enemyVisuals.HandleEnemyMove;

        if (IsServerStarted)
        {
            _aggroController = Instantiate(_aggroControllerPrefab, transform);
            _aggroController.OnAggroProceed += HandleAggroOnPlayer;
            _aggroController.OnAggroRelease += HandleAggroOnPlayerRelease;
            _aggroController.Init(_enemyStats);

            _enemyVisuals.AnimatorController.OnLookPositionUpdate += HandleLookPositionUpdate;
        }

        IsInitialized = true;

        if (IsServerStarted)
        {
            SERVER_SetState(EnemyState.Idle);
        }
    }


    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        if (IsServerStarted)
        {
            SERVER_ProcessStateMachine();
        }
    }

    [Server]
    public void SERVER_SetState(EnemyState state)
    {
        if(state == _currentState) return;

        _currentState = state;
    }

    [Server]
    void SERVER_ProcessStateMachine()
    {
        switch (_currentState)
        {
            case EnemyState.Idle:
                Transform randomKeyPoint = LocationManager.Instance.GetRandomClosestPoint(transform.position);
                _enemyMovementService.SetTarget(randomKeyPoint);
                SERVER_SetState(EnemyState.Stranding);
                break;
            case EnemyState.Stranding:
                if(_enemyMovementService.IsReachedTarget())
                {
                    SERVER_SetState(EnemyState.Idle);
                }
                break;
            case EnemyState.Following:
                break;
            default:
                break;
        }

        OnStateUpdate?.Invoke(_currentState);
    }

    void HandleAggroOnPlayer(Player player)
    {
        SERVER_SetState(EnemyState.Following);
        _enemyMovementService.SetTarget(player.transform);
    }
    void HandleAggroOnPlayerRelease()
    {
        SERVER_SetState(EnemyState.Idle);
    }

    void HandleLookPositionUpdate(Vector3 lookPosition)
    {
        _aggroController.UpdateSightDirection(lookPosition);
    }
}
