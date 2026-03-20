#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MaterialCounter : EditorWindow
{
    [MenuItem("Tools/Count Scene Materials")]
    static void Count()
    {
        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var materials = new HashSet<Material>();

        foreach (var r in renderers)
            foreach (var m in r.sharedMaterials)
                if (m != null) materials.Add(m);

        Debug.Log($"Renderers: {renderers.Length} | Unique Materials: {materials.Count}");
    }
}
#endif