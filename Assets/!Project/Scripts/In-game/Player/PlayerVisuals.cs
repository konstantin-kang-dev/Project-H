
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class PlayerVisuals: NetworkBehaviour
{
    [SerializeField] List<PlayerModel> _playerModels = new List<PlayerModel>();
    PlayerModel _playerModel = null;

    public CharacterAnimatorController AnimatorController;
    Vector2 _targetMovementInputs = Vector2.zero;
    Vector2 _currentMovementInputs = Vector2.zero;

    [SerializeField] VisualEffect _modelChangeVfx;

    readonly SyncVar<AnimatorState> _currentAnimatorState = new SyncVar<AnimatorState>();

    public event Action<AnimatorState> OnAnimatorStateChanged;
    public event Action<PlayerModel, CharacterAnimatorController> OnPlayerModelChanged;
    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        foreach (var playerModel in _playerModels)
        {
            playerModel.gameObject.SetActive(false);
        }
    }

    public void Init(int modelKey)
    {
        _currentAnimatorState.OnChange += HandleAnimatorStateChange;

        ChangePlayerModel(modelKey);
        IsInitialized = true;
    }

    public void ChangePlayerModel(int key)
    {
        if (key < 0) return;

        if(_playerModel != null)
        {
            AnimatorController.ResetController();
            _playerModel.gameObject.SetActive(false);
        }

        _playerModel = _playerModels[key];
        _playerModel.gameObject.SetActive(true);

        AnimatorController = _playerModel.GetComponent<CharacterAnimatorController>();

        if (IsOwner)
        {
            AnimatorController.OnAnimatorStateChanged += RPC_RequestUpdateAnimatorState;
        }

        AnimatorController.Init();

        _modelChangeVfx.Play();
        OnPlayerModelChanged?.Invoke(_playerModel, AnimatorController);
        //Debug.Log($"[PlayerVisuals | {Owner.ClientId}] Changed model to: {key}");
    }

    private void Update()
    {
        if(!IsInitialized) return;

        if (!IsOwner)
        {
            _currentMovementInputs = Vector2.Lerp(_currentMovementInputs, _targetMovementInputs, 10f * Time.deltaTime);
            HandleWalk(_currentMovementInputs, false);
        }
    }

    public int GetRandomModelKey()
    {
        return Random.Range(0, _playerModels.Count);
    }

    public int GetNextModelKey(int prevKey, bool goForward)
    {
        int modelKey = ProjectUtils.GetNextIndex(prevKey, _playerModels.Count - 1, goForward);

        return modelKey;
    }

    public void HandleItemInHandSelect(IPickable item, int inventoryIndex)
    {
        if(item == null) return;
        AnimatorController.SetItemInHand(item, true);
    }

    public void HandleItemInHandDeselect(IPickable item, int inventoryIndex)
    {
        if (item == null) return;
        AnimatorController.SetItemInHand(item, false);
    }

    public void HandleWalk(Vector2 inputs, bool isSprinting)
    {
        if (AnimatorController == null) return;

        AnimatorController.SetMoveInputs(inputs);
        if(IsOwner)
        {
            RPC_RequestUpdateAnimatorMovement(inputs);
            AnimatorController.HandleWalking(inputs, isSprinting);
        }
    }

    public void HandleJump()
    {
        if (AnimatorController == null) return;

        AnimatorController.HandleJump();
    }

    public void HandleLand()
    {
        if (AnimatorController == null) return;

        AnimatorController.HandleLanding();
    }

    public void HandleCrouch(bool value)
    {
        if (AnimatorController == null) return;

        if (value)
        {
            AnimatorController.SetState(AnimatorState.Crouch);
        }
        else
        {
            AnimatorController.SetState(AnimatorState.Idle);
        }
    }

    [ServerRpc]
    public void RPC_RequestUpdateAnimatorState(AnimatorState state)
    {
        _currentAnimatorState.Value = state;
        //Debug.Log($"[PlayerVisuals | SERVER | {Owner.ClientId}] Updated animator state: {state}");
    }

    void HandleAnimatorStateChange(AnimatorState prev, AnimatorState next, bool asServer)
    {
        if (asServer) return;

        OnAnimatorStateChanged?.Invoke(next);
        if (IsOwner) return;

        AnimatorController.SetState(next);
    }

    [ServerRpc]
    public void RPC_RequestUpdateAnimatorMovement(Vector2 inputs)
    {
        RPC_HandleObserversUpdateAnimatorMovement(inputs);
    }

    [ObserversRpc]
    void RPC_HandleObserversUpdateAnimatorMovement(Vector2 inputs)
    {
        if (IsOwner) return;
        _targetMovementInputs = inputs;
    }
}