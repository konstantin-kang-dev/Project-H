using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] EnemyMovementService _enemyMovementServicePrefab;
    EnemyMovementService _enemyMovementService;

    [SerializeField] EnemyVisuals _enemyVisualsPrefab;
    EnemyVisuals _enemyVisuals;

    EnemyState _currentState = EnemyState.None;
    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        Init();
    }

    public void Init()
    {
        _enemyMovementService = Instantiate(_enemyMovementServicePrefab, transform);
        _enemyMovementService.Init();

        _enemyVisuals = Instantiate(_enemyVisualsPrefab, transform);
        _enemyVisuals.Init();

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
                
                break;
            case EnemyState.Stranding:
                break;
            case EnemyState.Following:
                break;
            default:
                break;
        }
    }
}
