using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResolutionListGenerator : MonoBehaviour
{
    [SerializeField] ValueSelectorUI _valueSelectorUI;

    private void Awake()
    {
        SetupResolutionsList();
    }

    void SetupResolutionsList()
    {
        Resolution[] resolutions = GraphicsManager.AvailableResolutions;

        List<string> _selectorValues = new List<string>();

        foreach (Resolution resolution in resolutions)
        {
            string stringRes = $"{resolution.width}x{resolution.height}";

            _selectorValues.Add(stringRes);
        }

        _valueSelectorUI.SetValues(_selectorValues);
    }
}