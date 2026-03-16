using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ObjectiveType
{
    Notebooks = 0,
}

[CreateAssetMenu(fileName = "ObjectiveConfig", menuName = "GameData/ObjectiveConfig")]
public class ObjectiveConfig : SerializedScriptableObject
{
    public GameObject ObjectivePrefab;
    public ObjectiveType Type;
    public string Description;
    public Dictionary<DifficultyType, int> ObjectivesAmountPerDiff = new Dictionary<DifficultyType, int>();

    public int GetObjectivesAmountForDiff(DifficultyType type)
    {
        if (!ObjectivesAmountPerDiff.ContainsKey(type))
        {
            Debug.LogError($"[ObjectiveConfig | {Type}] ObjectivesAmountPerDiff doesn't contain amount for {type} diff.");
            return 0;
        }

        return ObjectivesAmountPerDiff[type];
    }

    public ObjectiveConfig Clone()
    {
        var clone = (ObjectiveConfig)MemberwiseClone();
        clone.ObjectivesAmountPerDiff = new Dictionary<DifficultyType, int>(ObjectivesAmountPerDiff);
        return clone;
    }
}