
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

    public AnimatorController AnimatorController;
    Vector2 _targetMovementInputs = Vector2.zero;
    Vector2 _currentMovementInputs = Vector2.zero;

    [SerializeField] VisualEffect _modelChangeVfx;

    readonly SyncVar<AnimatorState> _currentAnimatorState = new SyncVar<AnimatorState>();

    public event Action<PlayerModel, AnimatorController> OnPlayerModelChanged;
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

        AnimatorController = _playerModel.GetComponent<AnimatorController>();

        if (IsOwner)
        {
            AnimatorController.OnAnimatorStateChanged += RPC_RequestUpdateAnimatorState;
        }

        AnimatorController.Init();

        _modelChangeVfx.Play();
        OnPlayerModelChanged?.Invoke(_playerModel, AnimatorController);
        Debug.Log($"[PlayerVisuals | {Owner.ClientId}] Changed model to: {key}");
    }

    private void Update()
    {
        if(!IsInitialized) return;

        if (!IsOwner)
        {
            _currentMovementInputs = Vector2.Lerp(_currentMovementInputs, _targetMovementInputs, 10f * Time.deltaTime);
            HandleWalk(_currentMovementInputs);
        }
    }

    public int GetRandomModelKey()
    {
        return Random.Range(0, _playerModels.Count);
    }

    public int GetNextModelKey(int prevKey, bool goForward)
    {
        int modelKey = prevKey;
        if (goForward)
        {
            modelKey += 1;
            if (modelKey > _playerModels.Count - 1) modelKey = 0;
        }
        else
        {
            modelKey -= 1;
            if (modelKey < 0) modelKey = _playerModels.Count - 1;
        }

        return modelKey;
    }

    public void HandleWalk(Vector2 inputs)
    {
        if (AnimatorController == null) return;
        AnimatorController.HandleWalk(inputs);
        if(IsOwner)
        {
            RPC_RequestUpdateAnimatorMovement(inputs);
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
        if (IsOwner) return;
        AnimatorController.PlayAnimation(next);
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