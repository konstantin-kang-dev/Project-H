using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum ItemType
{
    None = -2,
    Flashlight = -1,
    KitchenKey = 0,
    BedroomKey = 1,
    StorageKey = 2,
    ExitKey = 3,
    OilLamp = 4,
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
