using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResolutionListGenerator : MonoBehaviour
{
    [SerializeField] ValueSelectorUI _valueSelectorUI;

    private void Awake()
    {

    }

    public void SetupResolutionsList()
    {
        Debug.Log($"[ResolutionListGenerator] Setup resolutions list started.");
        Resolution[] resolutions = GraphicsManager.AvailableResolutions;

        List<string> _selectorValues = new List<string>();

        foreach (Resolution resolution in resolutions)
        {
            string stringRes = $"{resolution.width}x{resolution.height}";

            //Debug.Log($"[ResolutionListGenerator] Added resolution: {stringRes}.");
            _selectorValues.Add(stringRes);
        }

        _valueSelectorUI.SetValues(_selectorValues);
        //Debug.Log($"[ResolutionListGenerator] Setup resolutions list finished. Total: {_selectorValues.Count}");
    }
}