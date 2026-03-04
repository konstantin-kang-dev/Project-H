using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CameraGraphicsApplier : MonoBehaviour
{
    private void Start()
    {
        HDAdditionalCameraData hdData = GetComponent<HDAdditionalCameraData>();
        GraphicsManager.ApplyCameraSettings(hdData);

    }
    void Update()
    {
        Debug.Log($"Scale: {DynamicResolutionHandler.instance.GetCurrentScale()}");
    }
}