using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameDifficultyManager : SerializedMonoBehaviour
{
    public static GameDifficultyManager Instance { get; private set; }
    [SerializeField] string _configsPath;

    Dictionary<DifficultyType, GameDifficultyConfig> _difficultyConfigs = new Dictionary<DifficultyType, GameDifficultyConfig>();

    GameDifficultyConfig _selectedConfig = null;
    public GameDifficultyConfig SelectedConfig => _selectedConfig;

    public bool IsInitialized { get; private set; } = false;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void Init()
    {
        if (IsInitialized) return;
        Load();
        IsInitialized = true;
    }

    void Load()
    {
        _difficultyConfigs.Clear();
        GameDifficultyConfig[] configs = Resources.LoadAll<GameDifficultyConfig>(_configsPath);

        foreach (GameDifficultyConfig config in configs)
        {
            _difficultyConfigs[config.DifficultyType] = config.Clone();
        }
        _selectedConfig = _difficultyConfigs.Values.ToList()[0];

        Debug.Log($"[GameDifficultyManager] Loaded {configs.Length} configs.");
    }

    public void SelectConfig(DifficultyType difficultyType)
    {
        if (!_difficultyConfigs.ContainsKey(difficultyType)) throw new System.Exception($"[GameDifficultyManager] Config with type {difficultyType} not found.");

        _selectedConfig = _difficultyConfigs[difficultyType];
    }

    public GameDifficultyConfig GetConfigByIndex(int index)
    {
        if (_difficultyConfigs.Count - 1 < index) throw new System.Exception($"[GameDifficultyManager] Index is out of length.");

        return _difficultyConfigs.Values.ToList()[index];
    }
}