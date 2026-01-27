using UnityEditor;
using UnityEngine;

public interface IPickable
{
    string Name { get; }
    Transform Transform { get; }
    void PickUp(int playerObjectId);
    void Drop();
    void SetHighlight(bool value);
}