using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Player _player;

    Rigidbody _rb;
    CapsuleCollider _capsuleCollider;

    [SerializeField] PlayerStatsData _playerStatsData;
    PlayerStats _playerStats;

    [SerializeField] PlayerVisuals _playerVisualsPrefab;
    PlayerVisuals _playerVisuals;

    [SerializeField] PlayerMovementService _playerMovementServicePrefab;
    PlayerMovementService _playerMovementService;

    [SerializeField] CameraController _cameraControllerPrefab;
    CameraController _cameraController;

    public bool IsInitialized { get; private set; } = false;
    private void Start()
    {

    }

    public void Init(Player player)
    {
        _player = player;

        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _playerStats = new PlayerStats(_playerStatsData);

        if (_player.IsOwner)
        {
            _cameraController = Instantiate(_cameraControllerPrefab, transform);
            _cameraController.Init();
        }

        _playerVisuals = Instantiate(_playerVisualsPrefab, transform);
        _playerVisuals.Init(player.ModelKey);

        _playerMovementService = Instantiate(_playerMovementServicePrefab, transform);
        _playerMovementService.Init(_player, _playerStats, _rb, _capsuleCollider);

        if (_player.IsOwner)
        {
            _cameraController.OnLookPositionUpdate += _playerVisuals.AnimatorController.SetLookPosition;
            _cameraController.OnLookPositionUpdate += _player.RPC_RequestSetLookPosition;

            _playerMovementService.OnWalkStart += _playerVisuals.HandleWalk;
            _playerMovementService.OnWalkUpdate += _playerVisuals.HandleWalk;

            _cameraController.OnRotationUpdate += _playerMovementService.UpdateRotation;
            _cameraController.OnRotationUpdate += _player.RPC_RequestSetCharacterRotation;
        }

        IsInitialized = true;
    }

    public void ChangePlayerModel(int modelKey)
    {
        if (!IsInitialized) return;

        _playerVisuals.ChangePlayerModel(modelKey);
    }

    public void SetLookPosition(Vector3 lookPosition)
    {
        if (!IsInitialized) return;

        _playerVisuals.AnimatorController.SetLookPosition(lookPosition);
    }
    public void SetCharacterRotation(float rotationY)
    {
        if (!IsInitialized) return;

        _playerMovementService.UpdateRotation(new Vector2(0, rotationY));
    }

    public void SetWalkingState(bool isWalking)
    {
        if (!IsInitialized) return;

        _playerMovementService.SetWalkingState(isWalking);
    }

    public void SetSprintingState(bool isSprinting)
    {
        if (!IsInitialized) return;

        _playerMovementService.SetSprintState(isSprinting);
    }

}