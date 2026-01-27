using UnityEngine;

public static class LayerMaskUtils
{
    public static bool Contains(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask) != 0;
    }
}