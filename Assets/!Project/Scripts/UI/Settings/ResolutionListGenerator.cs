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

        Dictionary<int, string> _selectorValues = new Dictionary<int, string>();
        int key = 0;

        foreach (Resolution resolution in resolutions)
        {
            string stringRes = $"{resolution.width}x{resolution.height}";

            _selectorValues.Add(key, stringRes);
            key++;
        }

        _valueSelectorUI.SetValues(_selectorValues);
    }
}