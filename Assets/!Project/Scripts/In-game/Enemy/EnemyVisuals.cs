using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEditor;
using UnityEngine;

public class EnemyVisuals : NetworkBehaviour
{
    [SerializeField] EnemyModel _enemyModel;
    [SerializeField] float _referenceMoveSpeed = 1.7f;
    [SerializeField] float _referenceSprintSpeed = 3f;

    public CharacterAnimatorController AnimatorController;

    float _headRotationTimer = 0f;
    float _headRotationInterval = 2f;

    readonly SyncVar<AnimatorState> _currentAnimatorState = new SyncVar<AnimatorState>();

    readonly SyncVar<Vector3> _lookPosition = new SyncVar<Vector3>();
    public void Init(EnemyStatsConfig enemyStats)
    {
        AnimatorController = _enemyModel.GetComponent<CharacterAnimatorController>();
        AnimatorController.Init();

        float moveSpeedRatio = enemyStats.MoveSpeed / _referenceMoveSpeed;
        float sprintSpeedRatio = enemyStats.SprintSpeed / _referenceSprintSpeed;
        AnimatorController.Animator.SetFloat("MoveSpeedRatio", moveSpeedRatio);
        AnimatorController.Animator.SetFloat("SprintSpeedRatio", sprintSpeedRatio);
    }

    private void OnDestroy()
    {

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _currentAnimatorState.OnChange += HandleAnimatorStateChange;
        _lookPosition.OnChange += HandleLookPositionChange;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        _currentAnimatorState.OnChange -= HandleAnimatorStateChange;
        _lookPosition.OnChange -= HandleLookPositionChange;
    }

    public void HandleEnemyMove(Transform target)
    {


    }

    public void HandleFollowingPlayer(Player player)
    {
        if (IsServerStarted)
        {
            SERVER_SetLookPosition(player.CameraPosition);
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
    }

}