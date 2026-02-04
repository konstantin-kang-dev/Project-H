using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] Transform _cameraPoint;

    [SerializeField] Vector2 _sensitivity = Vector2.one;
    [SerializeField] float _smoothTime = 0.3f;

    Vector2 _lookInput = Vector2.zero;

    Vector2 _targetRotation = Vector2.zero;
    Vector2 _currentRotation = Vector2.zero;
    Vector2 _rotationVelocity = Vector2.zero;

    Vector3 _lookPosition = Vector3.zero;

    public event Action<Vector3> OnLookPositionUpdate;
    public event Action<Vector2> OnRotationUpdate;
    public event Action<Collider> OnRaycast;
    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        
    }

    public void Init()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
         
        IsInitialized = true;
    }

    void Update()
    {
        if (!IsInitialized) return;
        if (GameManager.Instance.GameState != GameState.Started) return;

        Rotate();
    }

    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        CheckForCollider();
    }

    void Rotate()
    {
        _targetRotation.x -= _lookInput.y * _sensitivity.y;
        _targetRotation.y += _lookInput.x * _sensitivity.x;
        _targetRotation.x = Mathf.Clamp(_targetRotation.x, -90f, 90f);

        _currentRotation = Vector2.SmoothDamp(_currentRotation, _targetRotation, ref _rotationVelocity, _smoothTime);

        _cameraPoint.localRotation = Quaternion.Euler(_currentRotation.x, 0f, 0f);

        _lookPosition = _cameraPoint.position + _cameraPoint.forward.normalized * 3f;

        //Debug.Log($"[CameraController] Updated rotation: {_currentRotation}/{_targetRotation} and look position: {_lookPosition} inputs: {moveDelta}", this);
        //Debug.Log($"[CameraController] Updated look position: {_lookPosition} cameraPointPos: {_cameraPoint.position} cameraForward: {_cameraPoint.forward.normalized} inputs: {moveDelta}", this);

        OnLookPositionUpdate?.Invoke(_lookPosition);
        OnRotationUpdate?.Invoke(_currentRotation);
    }

    public void HandleLookInput(Vector2 lookInput)
    {
        _lookInput = lookInput;
    }

    Collider CheckForCollider()
    {
        Collider collider = null;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = _camera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            collider = hitInfo.collider;
        }

        OnRaycast?.Invoke(collider);
        return collider;
    }
}
