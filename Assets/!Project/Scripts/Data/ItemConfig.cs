using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemConfig", menuName = "GameData/ItemConfig")]
public class ItemConfig : SerializedScriptableObject
{
    public ItemType Type;

    public Vector3 HandHoldingPosition;
    public Vector3 HandHoldingRotation;

    public ItemConfig Clone()
    {
        return (ItemConfig)MemberwiseClone();
    }
}