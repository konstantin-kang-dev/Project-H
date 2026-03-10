using System;
using UnityEngine;

public class VisibilityChecker : MonoBehaviour
{
    [SerializeField] private LayerMask _obstacleMask;

    private bool _isVisible;

    public event Action<bool> OnVisibilityChanged;

    private void FixedUpdate()
    {
        Camera camera = Camera.main;
        if (camera == null) return;

        Vector3 dirToCamera = camera.transform.position - transform.position;
        float distance = dirToCamera.magnitude;

        bool inFrustum = GeometryUtility.TestPlanesAABB(
            GeometryUtility.CalculateFrustumPlanes(camera),
            new Bounds(transform.position, Vector3.one * 0.1f)
        );

        if (!inFrustum)
        {
            SetVisible(false);
            return;
        }

        bool blocked = Physics.Raycast(transform.position, dirToCamera.normalized, distance, _obstacleMask);
        SetVisible(!blocked);
    }

    private void SetVisible(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;
        OnVisibilityChanged?.Invoke(visible);
    }
}