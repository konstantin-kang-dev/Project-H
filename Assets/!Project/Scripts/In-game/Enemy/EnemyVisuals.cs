using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEditor;
using UnityEngine;

public class EnemyVisuals : NetworkBehaviour
{
    [SerializeField] EnemyModel _enemyModelPrefab;
    EnemyModel _enemyModel;

    public CharacterAnimatorController AnimatorController;

    float _headRotationTimer = 0f;
    float _headRotationInterval = 2f;

    readonly SyncVar<AnimatorState> _currentAnimatorState = new SyncVar<AnimatorState>();

    readonly SyncVar<Vector3> _lookPosition = new SyncVar<Vector3>();
    public void Init()
    {
        _enemyModel = Instantiate(_enemyModelPrefab, transform);

        _currentAnimatorState.OnChange += HandleAnimatorStateChange;
        _lookPosition.OnChange += HandleLookPositionChange;

        AnimatorController = _enemyModel.GetComponent<CharacterAnimatorController>();
        AnimatorController.Init();
    }

    public void HandleEnemyMove(bool isFollowingPlayer, Transform target)
    {
        if (IsServerStarted)
        {
            if (isFollowingPlayer)
            {
                Vector3 lookPos = target.position;
                lookPos.y += 1f;
                SERVER_SetLookPosition(lookPos);
            }
        }

    }

    public void HandleStateChange(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                SERVER_SetAnimatorState(AnimatorState.Idle);
                break;
            case EnemyState.Stranding:
                SERVER_SetAnimatorState(AnimatorState.Walk);
                break;
            case EnemyState.Following:
                SERVER_SetAnimatorState(AnimatorState.Sprint);
                break;
            case EnemyState.Killing:
                SERVER_SetAnimatorState(AnimatorState.KnockDown);
                break;
            default:
                break;
        }
    }
    public void HandleStateMachineUpdate(EnemyState state)
    {
        if (IsServerStarted)
        {
            switch (state)
            {
                case EnemyState.Following:
                    _headRotationTimer = 0f;
                    break;
                default:
                    _headRotationTimer += Time.fixedDeltaTime;
                    break;
            }

            if (_headRotationTimer >= _headRotationInterval)
            {
                SERVER_RotateHeadRandomly();
            }
        }
    }

    public void HandleKillPlayer(Player player)
    {
        transform.rotation = ProjectUtils.GetFlatYLookRotation(transform.position, player.transform.position);
    }

    [Server]
    void SERVER_RotateHeadRandomly()
    {
        _headRotationTimer = 0f;

        Vector3 randomPosInCone = ProjectUtils.RandomPositionInRectangle(transform.position, transform.forward, 7f, 13f, 0.5f);
        SERVER_SetLookPosition(randomPosInCone);
    }

    [Server]
    public void SERVER_SetLookPosition(Vector3 lookPosition)
    {
        _lookPosition.Value = lookPosition;

    }

    [Client]
    void HandleLookPositionChange(Vector3 prev, Vector3 next, bool asServer)
    {
        if (asServer) return;

        AnimatorController.SetLookPosition(next);
    }

    [Server]
    void SERVER_SetAnimatorState(AnimatorState state)
    {
        _currentAnimatorState.Value = state;
    }

    [Client]
    void HandleAnimatorStateChange(AnimatorState prev, AnimatorState next, bool asServer)
    {
        if (asServer) return;

        AnimatorController.SetState(next, true);
        Debug.Log($"[EnemyVisuals] HandleAnimatorStateChange {next}");
    }
}