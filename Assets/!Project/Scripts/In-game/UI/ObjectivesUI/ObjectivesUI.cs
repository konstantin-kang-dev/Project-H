using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ObjectivesUI : MonoBehaviour
{
    [SerializeField] ObjectiveUI _objectivePrefab;
    [SerializeField] RectTransform _container;

    Dictionary<ObjectiveType, ObjectiveUI> _objectives = new Dictionary<ObjectiveType, ObjectiveUI>();

    public bool IsInitialized { get; private set; } = false;
    private void Awake()
    {
        for (int i = 0; i < _container.childCount; i++)
        {
            Transform child = _container.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    public void Init()
    {
        ObjectivesManager.Instance.OnObjectiveCollected += HandleObjectiveCollected;
        ObjectivesManager.Instance.OnObjectiveCompleted += HandleObjectiveCompleted;
        ObjectivesManager.Instance.OnObjectiveInitialized += AddObjective;

        IsInitialized = true;
    }

    public void AddObjective(ObjectiveType objectiveType, int amountToComplete)
    {
        ObjectiveConfig config = ObjectivesManager.Instance.GetConfigByType(objectiveType);

        if (_objectives.ContainsKey(config.Type))
        {
            Debug.LogError($"[ObjectivesUI] Objective with type {config.Type} already exists.");
            return;
        }

        ObjectiveUI objectiveUI = Instantiate(_objectivePrefab, _container);
        string startDescription = $"{config.Description} (0/{amountToComplete})";

        _objectives[config.Type] = objectiveUI;
        objectiveUI.OnDestroy += HandleObjectiveUIDestroy;

        objectiveUI.Init(config.Type, amountToComplete, startDescription);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
    }

    void HandleObjectiveCollected(ObjectiveType type, int collectedAmount, string description)
    {
        if (!_objectives.ContainsKey(type)) return;

        _objectives[type].HandleUpdateObjective(collectedAmount, description);
    }
    void HandleObjectiveCompleted(ObjectiveType type)
    {
        if (!_objectives.ContainsKey(type)) return;

        _objectives[type].HandleCompleteObjective();
    }

    void HandleObjectiveUIDestroy(ObjectiveUI objectiveUI)
    {
        if (!_objectives.ContainsKey(objectiveUI.ObjectiveType)) return;

        _objectives.Remove(objectiveUI.ObjectiveType);
    }

    private void OnDestroy()
    {
        ObjectivesManager.Instance.OnObjectiveCollected -= HandleObjectiveCollected;
        ObjectivesManager.Instance.OnObjectiveCompleted -= HandleObjectiveCompleted;
    }
}