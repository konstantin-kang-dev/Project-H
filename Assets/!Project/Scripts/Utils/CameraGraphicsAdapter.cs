using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CameraGraphicsApplier : MonoBehaviour
{
    private void Start()
    {
        HDAdditionalCameraData hdData = GetComponent<HDAdditionalCameraData>();
        if (hdData != null)
        {
            GraphicsManager.ApplyCameraSettings(hdData);
        }

    }

}