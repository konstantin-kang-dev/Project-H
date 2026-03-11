using Cysharp.Threading.Tasks;
using DG.Tweening;
using FishNet.Object;
using GameAudio;
using System;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    EnemyStatsConfig _enemyStats;

    [SerializeField] EnemyMovementService _enemyMovementService;

    [SerializeField] EnemyVisuals _enemyVisuals;

    [SerializeField] AggroController _aggroControllerPrefab;
    AggroController _aggroController;

    [SerializeField] EnemyInteractionService _enemyInteractionServicePrefab;
    EnemyInteractionService _enemyInteractionService;

    [SerializeField] CharacterAudioService _characterAudioService;
    [SerializeField] VisibilityChecker _visibilityChecker;

    [SerializeField] EnemyState _currentState = EnemyState.None;

    public event Action<EnemyState> OnStateChange;
    public event Action<EnemyState> OnStateMachineUpdate;
    public bool IsInitialized { get; private set; } = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        Init(GameManager.Instance.GameDifficultyConfig.EnemyStats);
    }

    public void Init(EnemyStatsConfig enemyStats)
    {
        _enemyStats = enemyStats;

        if (IsServerStarted)
        {
            _enemyMovementService.Init(_enemyStats);
        }

        _enemyVisuals.Init();
        _visibilityChecker.OnVisibilityChanged += HandleModelVisibilityInCameraChange;

        OnStateMachineUpdate += _enemyVisuals.HandleStateMachineUpdate;

        _enemyMovementService.OnMove += _enemyVisuals.HandleEnemyMove;

        if (IsServerStarted)
        {
            OnStateChange += _enemyVisuals.HandleStateChange;

            _aggroController = Instantiate(_aggroControllerPrefab, transform);
            _aggroController.OnAggroProceed += HandleAggro;
            _aggroController.OnAggroRelease += HandleAggroRelease;
            _aggroController.Init(_enemyStats);

            _enemyVisuals.AnimatorController.OnLookPositionUpdate += HandleLookPositionUpdate;

            _enemyInteractionService = Instantiate(_enemyInteractionServicePrefab, transform);
            _enemyInteractionService.Init();

            _enemyMovementService.OnReachedPlayer += SERVER_KillPlayer;
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

        OnStateChange?.Invoke(_currentState);
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

        OnStateMachineUpdate?.Invoke(_currentState);
    }

    [Server]
    public async void SERVER_KillPlayer(Player player)
    {
        RPC_NotifyPlayerKill(player.PlayerData.ClientId);

        _enemyMovementService.HandleKillPlayer(player); 
        _enemyVisuals.HandleKillPlayer(player);
        SERVER_SetState(EnemyState.Killing);

        Vector3 playerPos = player.transform.position;
        playerPos.y = 0;

        Vector3 enemyPos = transform.position;
        enemyPos.y = 0;

        Vector3 playerTargetPos = ProjectUtils.GetPositionAtDistance(playerPos, enemyPos, 1.5f);

        player.Teleport(playerTargetPos);
        player.SERVER_SetKnockedDown(true);

        await UniTask.WaitForSeconds(1.3f);

        SERVER_SetState(EnemyState.Idle);
        _enemyMovementService.SetMoveAbility(true);
    }

    [ObserversRpc]
    void RPC_NotifyPlayerKill(int playerClientId)
    {
        _characterAudioService.Play(CharacterAudioType.Jumpscare);

    }

    void HandleAggro(Player player)
    {
        SERVER_SetState(EnemyState.Following);
        _enemyMovementService.SetTarget(player.transform);
        RPC_NotifyPlayerAggro(player.PlayerData.ClientId);
    }
    [ObserversRpc]
    void RPC_NotifyPlayerAggro(int playerClientId)
    {
        _characterAudioService.Play(CharacterAudioType.ChasingScream);
        _characterAudioService.Play(CharacterAudioType.Detection);
    }
    void HandleAggroRelease()
    {
        SERVER_SetState(EnemyState.Idle);
        RPC_NotifyPlayerAggroRelease();
    }
    [ObserversRpc]
    void RPC_NotifyPlayerAggroRelease()
    {
        _characterAudioService.Stop(CharacterAudioType.ChasingScream);
    }

    void HandleLookPositionUpdate(Vector3 lookPosition)
    {
        _aggroController.UpdateSightDirection(lookPosition);
    }

    void HandleModelVisibilityInCameraChange(bool visible)
    {
        if(visible)
        {
            GlobalAudioManager.Instance.Play(SoundType.EnemySpotted);
        }
    }
}
