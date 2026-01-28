using UnityEditor;
using UnityEngine;

public interface IPickable
{
    ItemConfig ItemConfig { get; }
    Transform Transform { get; }
    void PickUp(int playerObjectId);
    void Drop();
    void SetHighlight(bool value);
}