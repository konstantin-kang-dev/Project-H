using FishNet.Component.Animating;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Player _player;

    Rigidbody _rb;
    CapsuleCollider _capsuleCollider;

    PlayerStatsConfig _playerStatsConfig;

    [field: SerializeField] public PlayerVisuals PlayerVisuals { get; private set; }

    [SerializeField] PlayerMovementService _playerMovementServicePrefab;
    PlayerMovementService _playerMovementService;

    [SerializeField] CameraController _cameraControllerPrefab;
    public CameraController CameraController { get; private set; }

    [field: SerializeField] public PlayerInventory PlayerInventory { get; private set; }
    public bool IsInitialized { get; private set; } = false;
    private void Start()
    {

    }

    public void Init(Player player)
    {
        _player = player;

        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        GameDifficultyConfig difficultyConfig = GameDifficultyManager.Instance.SelectedConfig;
        _playerStatsConfig = difficultyConfig.PlayersStats.Clone();

        if (_player.IsOwner)
        {
            _rb.isKinematic = false;
            CameraController = Instantiate(_cameraControllerPrefab, transform);
            CameraController.Init();

            CameraController.OnRaycast += PlayerInventory.HandleRaycast;
        }
        else
        {

        }

        PlayerVisuals.Init(_player.ModelKey);

        _playerMovementService = Instantiate(_playerMovementServicePrefab, transform);
        _playerMovementService.Init(_player, _playerStatsConfig, _rb, _capsuleCollider);

        if (_player.IsOwner)
        {
            PlayerVisuals.AnimatorController.SetHeadVisibility(false);
            CameraController.OnLookPositionUpdate += PlayerVisuals.AnimatorController.SetLookPosition;
            CameraController.OnLookPositionUpdate += _player.RPC_RequestSetLookPosition;

            _playerMovementService.OnWalkStart += PlayerVisuals.HandleWalk;
            _playerMovementService.OnWalkUpdate += PlayerVisuals.HandleWalk;

            CameraController.OnRotationUpdate += _playerMovementService.UpdateRotation;
            CameraController.OnRotationUpdate += _player.RPC_RequestSetCharacterRotation;
        }

        PlayerInventory.Init(_player);

        IsInitialized = true;

        if (_player.IsOwner)
        {
            Debug.Log($"[PlayerController] Initialized.");
        }
    }

    public void SetLookPosition(Vector3 lookPosition)
    {
        if (!IsInitialized) return;

        PlayerVisuals.AnimatorController.SetLookPosition(lookPosition);
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