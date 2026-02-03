using UnityEditor;
using UnityEngine;

public interface IPickable
{
    ItemConfig ItemConfig { get; }
    Transform Transform { get; }
    int ItemObjectId { get; }
    bool IsPickedUp { get; }
    void SERVER_PickUp(int playerObjectId);
    void SERVER_Drop();
    void SetHighlight(bool value);
    void SetVisibility(bool value);
    void SetColliders(bool value);
}