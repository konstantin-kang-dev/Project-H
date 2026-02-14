using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEditor;
using UnityEngine;

public class EnemyVisuals : NetworkBehaviour
{
    [SerializeField] EnemyModel _enemyModelPrefab;
    EnemyModel _enemyModel;

    public AnimatorController AnimatorController;

    float _headRotationTimer = 0f;
    float _headRotationInterval = 2f;

    readonly SyncVar<AnimatorState> _currentAnimatorState = new SyncVar<AnimatorState>();

    readonly SyncVar<Vector3> _lookPosition = new SyncVar<Vector3>();
    public void Init()
    {
        _enemyModel = Instantiate(_enemyModelPrefab, transform);

        _currentAnimatorState.OnChange += HandleAnimatorStateChange;
        _lookPosition.OnChange += HandleLookPositionChange;

        AnimatorController = _enemyModel.GetComponent<AnimatorController>();
        AnimatorController.Init();
    }

    public void HandleEnemyMove(bool isFollowingPlayer, Transform target)
    {
        if (IsServerStarted)
        {
            if (isFollowingPlayer)
            {
                SERVER_SetAnimatorState(AnimatorState.Sprint);

                Vector3 lookPos = target.position;
                lookPos.y += 1f;
                SERVER_SetLookPosition(lookPos);
            }
            else
            {
                SERVER_SetAnimatorState(AnimatorState.Walk);
            }
        }

    }

    public void HandleStateUpdate(EnemyState state)
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

    [Server]
    void SERVER_RotateHeadRandomly()
    {
        _headRotationTimer = 0f;

        Vector3 randomPosInCone = ProjectUtils.RandomPositionInRectangle(transform.position, transform.forward, 7f, 13f, 0.5f);
        SERVER_SetLookPosition(randomPosInCone);
    }

    [Server]
    void SERVER_SetLookPosition(Vector3 lookPosition)
    {
        _lookPosition.Value = lookPosition;
    }

    [Client]
    void HandleLookPositionChange(Vector3 prev, Vector3 next, bool asServer)
    {
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
        AnimatorController.PlayAnimation(next);
    }
}