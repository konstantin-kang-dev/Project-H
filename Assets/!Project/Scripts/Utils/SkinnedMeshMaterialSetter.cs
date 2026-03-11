using UnityEngine;

public class SkinnedMeshMaterialSetter : MonoBehaviour
{
    [SerializeField] private Material _overrideMaterial;

    private SkinnedMeshRenderer[] _renderers;
    private Material[][] _originalMaterials;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        CacheOriginalMaterials();
    }

    private void CacheOriginalMaterials()
    {
        _originalMaterials = new Material[_renderers.Length][];
        for (int i = 0; i < _renderers.Length; i++)
            _originalMaterials[i] = _renderers[i].sharedMaterials;
    }

    public void SetMaterial(Material material)
    {
        foreach (var renderer in _renderers)
        {
            var mats = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = material;
            renderer.sharedMaterials = mats;
        }
    }

    public void ResetMaterials()
    {
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].sharedMaterials = _originalMaterials[i];
    }

    [ContextMenu("Apply Override Material")]
    public void ApplyOverrideMaterial() => SetMaterial(_overrideMaterial);

    [ContextMenu("Reset Materials")]
    public void ResetMaterialsContext() => ResetMaterials();
}