using UnityEngine;

public class ChildLayerSetter : MonoBehaviour
{
    private GameObject[] _children;
    private int[] _originalLayers;

    private void Awake()
    {
        CacheChildren();
    }

    private void CacheChildren()
    {
        var transforms = GetComponentsInChildren<Transform>(true);
        _children = new GameObject[transforms.Length];
        _originalLayers = new int[transforms.Length];

        for (int i = 0; i < transforms.Length; i++)
        {
            _children[i] = transforms[i].gameObject;
            _originalLayers[i] = transforms[i].gameObject.layer;
        }
    }

    public void SetLayer(int layer)
    {
        foreach (var go in _children)
            go.layer = layer;
    }

    public void SetLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{layerName}' not found.");
            return;
        }
        SetLayer(layer);
    }

    public void ResetLayers()
    {
        for (int i = 0; i < _children.Length; i++)
            _children[i].layer = _originalLayers[i];
    }
}