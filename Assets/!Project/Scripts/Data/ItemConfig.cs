using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum ItemType
{
    Flashlight = -1,
    DoorKey = 0,
}

[CreateAssetMenu(fileName = "ItemConfig", menuName = "GameData/ItemConfig")]
public class ItemConfig : SerializedScriptableObject
{
    public ItemType Type;

    public Vector3 HandPosition;
    public Vector3 HandRotation;

    public Vector3 HandItemPointPosition;
    public Vector3 HandItemPointRotation;

    public Sprite InventoryIcon;

    public ItemConfig Clone()
    {
        return (ItemConfig)MemberwiseClone();
    }
}
