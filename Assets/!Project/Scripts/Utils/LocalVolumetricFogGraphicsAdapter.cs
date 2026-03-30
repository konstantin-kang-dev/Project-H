using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LocalVolumetricFogGraphicsAdapter : MonoBehaviour
{
    private void Start()
    {
        LocalVolumetricFog localVolumetricFog = GetComponent<LocalVolumetricFog>();
        if(localVolumetricFog != null)
        {
            GraphicsManager.ApplyLocalVolumetricFogSettings(localVolumetricFog);
        }
    }
}
