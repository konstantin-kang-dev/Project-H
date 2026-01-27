using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameDifficultyManager : SerializedMonoBehaviour
{
    public static GameDifficultyManager Instance { get; private set; }
    [SerializeField] string _configsPath;

    Dictionary<string, GameDifficultyConfig> _difficultyConfigs = new Dictionary<string, GameDifficultyConfig>();

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
            _difficultyConfigs[config.DifficultyName] = config.Clone();
        }

        Debug.Log($"[GameDifficultyManager] Loaded {configs.Length} configs.");
    }

    public void SelectConfig(string difficultyName)
    {
        if (!_difficultyConfigs.ContainsKey(difficultyName)) throw new System.Exception($"[GameDifficultyManager] Config with name {difficultyName} not found.");

        _selectedConfig = _difficultyConfigs[difficultyName];
    }

    public GameDifficultyConfig GetConfigByIndex(int index)
    {
        if (_difficultyConfigs.Count - 1 < index) throw new System.Exception($"[GameDifficultyManager] Index is out of length.");

        return _difficultyConfigs.Values.ToList()[index];
    }
}