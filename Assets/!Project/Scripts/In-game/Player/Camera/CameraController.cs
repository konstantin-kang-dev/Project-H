using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    DefaultInput _input;
    Camera _camera;

    Vector2 _currentRotation = Vector2.zero;

    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        
    }

    public void Init(Camera camera)
    {
        _camera = camera;
        _input = new DefaultInput();
         
        IsInitialized = true;
    }

    void Update()
    {
        if (!IsInitialized) return;

        Rotate(_input.CurrentLookInput);
    }

    void Rotate(Vector2 moveDelta)
    {
        _currentRotation.x -= moveDelta.y;
        _currentRotation.y -= moveDelta.x;
        _currentRotation.x = Mathf.Clamp(_currentRotation.x, -90f, 90f);
        _camera.transform.localRotation = Quaternion.Euler(_currentRotation.x, _currentRotation.y, 0f);
    }
}
