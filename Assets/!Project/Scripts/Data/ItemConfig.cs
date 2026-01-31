using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum ItemType
{
    DoorKey = 0,
}

[CreateAssetMenu(fileName = "ItemConfig", menuName = "GameData/ItemConfig")]
public class ItemConfig : SerializedScriptableObject
{
    public ItemType Type;

    public Vector3 HandHoldingPosition;
    public Vector3 HandHoldingRotation;

    public Sprite InventoryIcon;

    public ItemConfig Clone()
    {
        return (ItemConfig)MemberwiseClone();
    }
}
