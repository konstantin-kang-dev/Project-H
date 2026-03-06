using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.STP;
using Random = UnityEngine.Random;

public class ObjectivesManager : NetworkBehaviour
{
    public static ObjectivesManager Instance;
    [SerializeField] List<ObjectiveConfig> _configs = new List<ObjectiveConfig>();
    [SerializeField] int _itemsPercentInLockedRooms = 30;

    Dictionary<ObjectiveType, ObjectiveConfig> _cachedConfigs = new Dictionary<ObjectiveType, ObjectiveConfig>();

    //ObjectiveConfig _chosenConfig;

    readonly SyncVar<bool> _isInitialized = new SyncVar<bool>();

    private readonly SyncDictionary<ObjectiveType, int> _objectives = new();
    public IReadOnlyDictionary<ObjectiveType, int> Objectives => _objectives;

    private readonly SyncDictionary<ObjectiveType, int> _completedObjectives = new();
    public IReadOnlyDictionary<ObjectiveType, int> CompletedObjectives => _completedObjectives;

    public event Action OnInitialize;
    public event Action<ObjectiveType, int> OnObjectiveInitialized;
    public event Action<ObjectiveType, string> OnObjectiveCollected;
    public event Action<ObjectiveType> OnObjectiveCompleted;
    public event Action OnAllObjectivesCollected;

    private void Awake()
    {
        _cachedConfigs.Clear();
        foreach (var config in _configs)
        {
            _cachedConfigs[config.Type] = config;
        }
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _objectives.OnChange += OnObjectivesChanged;
        _completedObjectives.OnChange += OnCompletedObjectivesChanged;
        _isInitialized.OnChange += HandleInitialization;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

    }

    [Server]
    public void Init(DifficultyType chosenDiff, int playersCount)
    {
        _completedObjectives.Clear();
        _objectives.Clear();

        List<ObjectiveConfig> configsCopy = new List<ObjectiveConfig>(_configs);

        playersCount = GameManager.Instance.Players.Count;

        int randomConfigKey = Random.Range(0, configsCopy.Count);
        ObjectiveConfig chosenConfig = configsCopy[randomConfigKey];
        configsCopy.RemoveAt(randomConfigKey);

        SpawnObjectives(chosenConfig, chosenDiff);

        Debug.Log($"[ObjectivesManager] Initialized.");

        _isInitialized.Value = true;
    }

    void HandleInitialization(bool prev, bool next, bool asServer)
    {
        if (asServer) return;

        if (next)
        {
            OnInitialize?.Invoke();
        }
    }
    void SpawnObjectives(ObjectiveConfig objectiveConfig, DifficultyType diff)
    {
        int totalObjectivesAmount = objectiveConfig.GetObjectivesAmountForDiff(diff);

        int objectivesAmountInRequiredPlaces = ProjectUtils.GetPercentOfValue(totalObjectivesAmount, _itemsPercentInLockedRooms);
        int objectivesAmountInCommonPlaces = totalObjectivesAmount - objectivesAmountInRequiredPlaces;

        _objectives[objectiveConfig.Type] = totalObjectivesAmount;
        _completedObjectives[objectiveConfig.Type] = 0;

        Debug.Log($"[ObjectivesManager] Started spawning {totalObjectivesAmount} (r: {objectivesAmountInRequiredPlaces} + c: {objectivesAmountInCommonPlaces}) items for objective {objectiveConfig.Type}.");

        for (int i = 0; i < totalObjectivesAmount; i++)
        {
            Transform freePoint = null;
            if (objectivesAmountInRequiredPlaces > 0)
            {
                freePoint = ObjectivesPointsManager.Instance.GetFreeCommonPoint();
                objectivesAmountInRequiredPlaces--; 
            }
            else if(objectivesAmountInCommonPlaces > 0)
            {
                freePoint = ObjectivesPointsManager.Instance.GetFreeRequiredPoint();
                objectivesAmountInCommonPlaces--;
            }

            if (freePoint == null)
            {
                Debug.LogError($"[ObjectivesManager] Not enough free points. Breaking spawn loop");
                break;
            }

            BasicObjectiveItem objectiveItem = Instantiate(objectiveConfig.ObjectivePrefab, freePoint.position, freePoint.rotation).GetComponent<BasicObjectiveItem>();
            NetworkObject networkObject = objectiveItem;
            networkObject.transform.SetParent(freePoint);

            ServerManager.Spawn(networkObject);

            objectiveItem.ResetAll();
            objectiveItem.SetObjectiveType(objectiveConfig.Type);
            objectiveItem.OnObjectiveCollected += HandleObjectivesInteraction;
        }

        Debug.Log($"[ObjectivesManager] Spawned ({totalObjectivesAmount}) items for objective {objectiveConfig.Type}.");
    }

    void HandleObjectivesInteraction(BasicObjectiveItem basicObjective)
    {
        ObjectiveType objectiveType = basicObjective.ObjectiveType;
        if (IsCompletedObjective(objectiveType)) return;

        _completedObjectives[objectiveType] += 1;
    }

    void OnCompletedObjectivesChanged(SyncDictionaryOperation op, ObjectiveType key, int value, bool asServer)
    {
        if (asServer) return;

        switch (op)
        {
            case SyncDictionaryOperation.Add:
            case SyncDictionaryOperation.Set:
                HandleObjectiveProgress(key, value);
                break;
        }
    }

    void HandleObjectiveProgress(ObjectiveType type, int collected)
    {
        if (!_objectives.ContainsKey(type)) return;

        int total = _objectives[type];
        Debug.Log($"[ObjectivesManager] Collected objective: {type} collected: {collected} total: {total}");

        ObjectiveConfig config = _cachedConfigs[type];

        string description = $"{config.Description} ({collected}/{total})";
        OnObjectiveCollected?.Invoke(type, description);

        if (collected >= total)
        {
            OnObjectiveCompleted?.Invoke(type);

            if (IsCompletedAllObjectives())
            {
                OnAllObjectivesCollected?.Invoke();
            }
        }
    }
    void OnObjectivesChanged(SyncDictionaryOperation op, ObjectiveType key, int value, bool asServer)
    {
        if (asServer) return;

        switch (op)
        {
            case SyncDictionaryOperation.Add:
                HandleObjectiveAdded(key, value);
                break;
            case SyncDictionaryOperation.Set:
                break;
        }
    }

    void HandleObjectiveAdded(ObjectiveType type, int amount)
    {
        if (!_objectives.ContainsKey(type)) return;

        OnObjectiveInitialized?.Invoke(type, amount);
    }

    bool IsCompletedObjective(ObjectiveType objectiveType)
    {
        if(!CompletedObjectives.ContainsKey(objectiveType)) return false;
        if(!Objectives.ContainsKey(objectiveType)) return false;

        return CompletedObjectives[objectiveType] >= Objectives[objectiveType];
    }

    bool IsCompletedAllObjectives()
    {
        bool result = true;

        foreach (var objectiveItems in Objectives)
        {
            if (CompletedObjectives[objectiveItems.Key] < objectiveItems.Value)
            {
                result = false;
                break;
            }
        }

        return result;
    }

    public ObjectiveConfig GetConfigByType(ObjectiveType type)
    {
        if (!_cachedConfigs.ContainsKey(type)) return null;
        return _cachedConfigs[type];
    }

}
