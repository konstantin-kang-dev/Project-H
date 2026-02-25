using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class CameraGraphicsApplier : MonoBehaviour
{
    private void OnEnable()
    {
        GraphicsManager.ApplyCameraSettings(GetComponent<HDAdditionalCameraData>());
    }
}