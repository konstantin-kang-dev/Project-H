using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum InteractableType
{
    Door = 0,
    Notebook = 1,
    Shelf = 2,
}

[CreateAssetMenu(fileName = "InteractableConfig", menuName = "GameData/InteractableConfig")]
public class InteractableConfig : SerializedScriptableObject
{
    public InteractableType Type;

    public ItemConfig Clone()
    {
        return (ItemConfig)MemberwiseClone();
    }
}