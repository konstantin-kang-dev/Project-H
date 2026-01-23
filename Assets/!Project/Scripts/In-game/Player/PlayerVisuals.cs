
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerVisuals: MonoBehaviour
{
    [SerializeField] List<PlayerModel> _playerModelPrefabs = new List<PlayerModel>();
    PlayerModel _playerModel;

    public AnimatorController AnimatorController;

    [SerializeField] ParticleSystem _modelChangeVfx;

    public event Action<PlayerModel, AnimatorController> OnPlayerModelChanged;
    public bool IsInitialized { get; private set; } = false;
    public void Init(int modelKey)
    {
        ChangePlayerModel(modelKey);
        IsInitialized = true;
    }

    public void ChangePlayerModel(int key)
    {
        if(_playerModel != null)
        {
            Destroy(_playerModel.gameObject);
        }

        PlayerModel newPrefab = _playerModelPrefabs[key];
        _playerModel = Instantiate(newPrefab, transform);

        AnimatorController = _playerModel.GetComponent<AnimatorController>();
        AnimatorController.Init();

        _modelChangeVfx.Play();
        OnPlayerModelChanged?.Invoke(_playerModel, AnimatorController);
        Debug.Log($"[PlayerVisuals] Changed model to: {key}");
    }

    public int GetRandomModelKey()
    {
        return Random.Range(0, _playerModelPrefabs.Count);
    }

    public int GetNextModelKey(int prevKey, bool goForward)
    {
        int modelKey = prevKey;
        if (goForward)
        {
            modelKey += 1;
            if (modelKey > _playerModelPrefabs.Count - 1) modelKey = 0;
        }
        else
        {
            modelKey -= 1;
            if (modelKey < 0) modelKey = _playerModelPrefabs.Count - 1;
        }

        return modelKey;
    }

    public void HandleWalk(Vector2 inputs)
    {
        if (AnimatorController == null) return;
        AnimatorController.HandleWalk(inputs);
    }
}