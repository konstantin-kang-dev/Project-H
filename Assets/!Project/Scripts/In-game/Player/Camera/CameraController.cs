using DG.Tweening;
using Saves;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] CinemachineCamera _cinemachineCamera;
    [SerializeField] Transform _cameraBlock;
    [SerializeField] Transform _cameraPoint;
    [SerializeField] Animator _cameraAnimator;

    bool _isActive = false;

    [SerializeField] Transform _standPoint;
    [SerializeField] Transform _crouchPoint;
    [SerializeField] Transform _liftedPoint;
    Tween _moveAnimation;

    [SerializeField] float _mouseSensitivity = 1f;
    [SerializeField] float _smoothTime = 0.3f;

    [SerializeField] float _defaultFov = 80f;
    [SerializeField] float _sprintFov = 100f;
    Tween _fovAnimation;

    Vector2 _lookInput = Vector2.zero;

    Vector2 _targetRotation = Vector2.zero;

    Vector3 _lookPosition = Vector3.zero;

    float _raycastDistance = 0f;

    public event Action<Vector3> OnLookPositionUpdate;
    public event Action<Vector2> OnRotationUpdate;
    public event Action<Collider> OnRaycast;
    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        
    }

    public void Init()
    {
        _cameraBlock.parent = null;

        GlobalInputManager.Input.OnLook += HandleLookInput;
        GameUI.Instance.OnGameplayUIFocusChange += HandleGameplayUIFocusChange;
        GameUI.Instance.SetCanvasCamera(_camera);

        SetupSensitivity(SaveManager.GameSave);
        SaveManager.OnSaveUpdated += SetupSensitivity;

        _isActive = true;
        IsInitialized = true;
    }
    public void SetupSensitivity(GameSave gameSave)
    {
        float mouseSensitivity = gameSave.SettingsSave.ControlsSave.MouseSensitivity;

        _mouseSensitivity = mouseSensitivity / 100f;
    }
    public void SetRaycastDistance(float raycastDistance)
    {
        _raycastDistance = raycastDistance;
    }

    void HandleGameplayUIFocusChange(bool visible)
    {
        _isActive = visible;
    }

    private void OnDestroy()
    {
        Clear();
    }

    public void Clear()
    {
        GlobalInputManager.Input.OnLook -= HandleLookInput;

        GameUI.Instance.OnGameplayUIFocusChange -= HandleGameplayUIFocusChange;
        _cameraAnimator.enabled = false;


        SaveManager.OnSaveUpdated -= SetupSensitivity;
    }

    void Update()
    {
        if (!IsInitialized) return;
        if (GameManager.Instance.GameState != GameState.Started) return;
        if (!_isActive) return;

        Rotate();
        CheckForCollider();
    }

    private void FixedUpdate()
    {
        if (!IsInitialized) return;
        if (GameManager.Instance.GameState != GameState.Started) return;

    }

    void Rotate()
    {
        _targetRotation.x -= _lookInput.y * _mouseSensitivity;
        _targetRotation.y += _lookInput.x * _mouseSensitivity;
        _targetRotation.x = Mathf.Clamp(_targetRotation.x, -90f, 90f);

        _cameraPoint.localRotation = Quaternion.Euler(_targetRotation.x, 0f, 0f);

        OnRotationUpdate?.Invoke(_targetRotation);

        _lookPosition = _cameraPoint.position + _cameraPoint.forward.normalized * 3f;

        //Debug.Log($"[CameraController] Updated rotation: {_currentRotation}/{_targetRotation} and look position: {_lookPosition} inputs: {moveDelta}", this);
        //Debug.Log($"[CameraController] Updated look position: {_lookPosition} cameraPointPos: {_cameraPoint.position} cameraForward: {_cameraPoint.forward.normalized} inputs: {moveDelta}", this);

        OnLookPositionUpdate?.Invoke(_lookPosition);
    }

    public void HandleAnimationChange(AnimatorState state)
    {
        switch (state)
        {
            case AnimatorState.Idle:
                ChangeCameraOffset(_standPoint.localPosition);
                _cameraAnimator.SetBool("IsWalking", false);
                _cameraAnimator.SetBool("IsRunning", false);
                break;
            case AnimatorState.Walk:
                ChangeCameraOffset(_standPoint.localPosition);
                _cameraAnimator.SetBool("IsWalking", true);
                _cameraAnimator.SetBool("IsRunning", false);
                break;
            case AnimatorState.Sprint:
                ChangeCameraOffset(_standPoint.localPosition);
                _cameraAnimator.SetBool("IsRunning", true);
                break;
            case AnimatorState.Crouch:
                ChangeCameraOffset(_crouchPoint.localPosition);
                _cameraAnimator.SetBool("IsWalking", false);
                _cameraAnimator.SetBool("IsRunning", false);
                break;
            case AnimatorState.KnockDown:
                ChangeCameraOffset(_liftedPoint.localPosition);
                _cameraAnimator.SetBool("IsWalking", false);
                _cameraAnimator.SetBool("IsRunning", false);
                break;
        }
    }

    void ChangeCameraOffset(Vector3 targetPos)
    {
        if(_moveAnimation != null)
        {
            _moveAnimation.Kill();
        }

        _moveAnimation = _cameraPoint.DOLocalMove(targetPos, 0.35f);
    }

    public void HandleLookInput(Vector2 lookInput)
    {
        _lookInput = lookInput;
    }

    public void AdjustFov(bool isSprinting)
    {
        if (!IsInitialized) return;

        float startFov = _cinemachineCamera.Lens.FieldOfView;
        float targetFov = isSprinting ? _sprintFov : _defaultFov;

        if(_fovAnimation != null)
        {
            _fovAnimation.Kill();
            _fovAnimation = null;
        }

        _fovAnimation = DOVirtual.Float(startFov, targetFov, 1.2f, (value) => _cinemachineCamera.Lens.FieldOfView = value);
    }

    Collider CheckForCollider()
    {
        Collider collider = null;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = _camera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _raycastDistance))
        {
            collider = hitInfo.collider;
        }

        OnRaycast?.Invoke(collider);
        return collider;
    }
}
