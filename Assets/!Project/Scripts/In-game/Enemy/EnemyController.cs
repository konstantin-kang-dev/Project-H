using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] EnemyStats _enemyStats;

    [SerializeField] EnemyMovementService _enemyMovementServicePrefab;
    EnemyMovementService _enemyMovementService;

    [SerializeField] EnemyVisuals _enemyVisualsPrefab;
    EnemyVisuals _enemyVisuals;

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
        _enemyMovementService = Instantiate(_enemyMovementServicePrefab, transform);
        _enemyMovementService.Init(_enemyStats);

        _enemyVisuals = Instantiate(_enemyVisualsPrefab, transform);
        _enemyVisuals.Init();
        _enemyVisuals.AnimatorController.OnLookPositionUpdate += HandleLookPositionUpdate;

        OnStateUpdate += _enemyVisuals.HandleStateUpdate;

        _enemyMovementService.OnMove += _enemyVisuals.HandleEnemyMove;

        _aggroController = Instantiate(_aggroControllerPrefab, transform);
        _aggroController.OnAggroProceed += HandleAggroOnPlayer;
        _aggroController.OnAggroRelease += HandleAggroOnPlayerRelease;
        _aggroController.Init(_enemyStats);

        IsInitialized = true;
        SetState(EnemyState.Idle);
    }


    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        ProcessStateMachine();
    }

    public void SetState(EnemyState state)
    {
        if(state == _currentState) return;

        _currentState = state;
    }

    void ProcessStateMachine()
    {
        switch (_currentState)
        {
            case EnemyState.Idle:
                Transform randomKeyPoint = LocationManager.Instance.GetRandomClosestPoint(transform.position);
                _enemyMovementService.SetTarget(randomKeyPoint);
                SetState(EnemyState.Stranding);
                break;
            case EnemyState.Stranding:
                if(_enemyMovementService.IsReachedTarget())
                {
                    SetState(EnemyState.Idle);
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
        SetState(EnemyState.Following);
        _enemyMovementService.SetTarget(player.transform);
    }
    void HandleAggroOnPlayerRelease()
    {
        SetState(EnemyState.Idle);
    }

    void HandleLookPositionUpdate(Vector3 lookPosition)
    {
        _aggroController.UpdateSightDirection(lookPosition);
    }
}
