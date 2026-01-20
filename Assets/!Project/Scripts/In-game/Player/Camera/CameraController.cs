using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    IInput _input;
    [SerializeField] Camera _camera;
    [SerializeField] Transform _cameraPoint;

    [SerializeField] Vector2 _sensitivity = Vector2.one;
    [SerializeField] float _smoothTime = 0.3f;

    Vector2 _targetRotation = Vector2.zero;
    Vector2 _currentRotation = Vector2.zero;
    float _rotationVelocityX = 0;
    float _rotationVelocityY = 0;

    Vector3 _lookPosition = Vector3.zero;

    public event Action<Vector3> OnLookPositionUpdate;
    public event Action<Vector2> OnRotationUpdate;
    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        
    }

    public void Init()
    {
        _input = new DefaultInput();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
         
        IsInitialized = true;
    }

    void Update()
    {
        if (!IsInitialized) return;

        Rotate(_input.CurrentLookInput);
    }

    void Rotate(Vector2 moveDelta)
    {
        _targetRotation.x -= moveDelta.y * _sensitivity.y;
        _targetRotation.y += moveDelta.x * _sensitivity.x;
        _targetRotation.x = Mathf.Clamp(_targetRotation.x, -90f, 90f);

        _currentRotation.x = Mathf.SmoothDamp(_currentRotation.x, _targetRotation.x, ref _rotationVelocityX, _smoothTime);
        _currentRotation.y = Mathf.SmoothDamp(_currentRotation.y, _targetRotation.y, ref _rotationVelocityY, _smoothTime);

        _cameraPoint.localRotation = Quaternion.Euler(_currentRotation.x, 0f, 0f);

        _lookPosition = _cameraPoint.position + _cameraPoint.forward * 3f;

        OnLookPositionUpdate?.Invoke(_lookPosition);
        OnRotationUpdate?.Invoke(_currentRotation);
    }

}
