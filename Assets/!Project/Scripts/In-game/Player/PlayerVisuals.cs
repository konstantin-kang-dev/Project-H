using System;
using UnityEditor;
using UnityEngine;

public class PlayerVisuals: MonoBehaviour
{
    [SerializeField] PlayerModel _playerModelPrefab;
    PlayerModel _playerModel;

    public AnimatorController AnimatorController;

    public event Action<PlayerModel> OnPlayerModelChanged;
    public bool IsInitialized { get; private set; } = false;
    public void Init()
    {
        ChangePlayerModel(_playerModelPrefab);

    }

    public void ChangePlayerModel(PlayerModel newPrefab)
    {
        Destroy(_playerModel);

        _playerModel = Instantiate(newPrefab, transform);

        AnimatorController = _playerModel.GetComponent<AnimatorController>();
        AnimatorController.Init();

        OnPlayerModelChanged?.Invoke(_playerModel);
    }

    public void HandleWalk(Vector2 inputs)
    {
        if (AnimatorController == null) return;
        AnimatorController.HandleWalk(inputs);
    }
}