using FishNet.Component.Animating;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Player _player;

    Rigidbody _rb;
    CapsuleCollider _capsuleCollider;

    PlayerStatsConfig _playerStatsConfig;

    public IInput Input { get; private set; }
    [field: SerializeField] public PlayerVisuals PlayerVisuals { get; private set; }

    [SerializeField] PlayerMovementService _playerMovementService;

    [SerializeField] CameraController _cameraControllerPrefab;
    public CameraController CameraController { get; private set; }

    [field: SerializeField] public PlayerInventory PlayerInventory { get; private set; }
    [field: SerializeField] public PlayerInteraction PlayerInteraction { get; private set; }
    public bool IsInitialized { get; private set; } = false;
    private void Start()
    {

    }

    public void Init(Player player)
    {
        _player = player;
        Input = new DefaultInput();

        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        GameDifficultyConfig difficultyConfig = GameDifficultyManager.Instance.SelectedConfig;
        _playerStatsConfig = difficultyConfig.PlayersStats.Clone();

        if (_player.IsOwner)
        {
            _rb.isKinematic = false;
            CameraController = Instantiate(_cameraControllerPrefab, transform);
            CameraController.Init();

            CameraController.OnRaycast += PlayerInteraction.HandleRaycast;
        }
        else
        {


        }

        PlayerVisuals.Init(_player.ModelKey);

        _playerMovementService.Init(_playerStatsConfig, _rb);

        if (_player.IsOwner)
        {
            PlayerVisuals.AnimatorController.SetHeadVisibility(false);

            Input.OnLook += CameraController.HandleLookInput;
            CameraController.OnLookPositionUpdate += PlayerVisuals.AnimatorController.SetLookPosition;
            CameraController.OnLookPositionUpdate += _player.RPC_RequestSetLookPosition;

            Input.OnMove += _playerMovementService.HandleMoveInput;
            Input.OnSprint += _playerMovementService.HandleSprintInput;
            _playerMovementService.OnWalkStart += PlayerVisuals.HandleWalk;
            _playerMovementService.OnWalkUpdate += PlayerVisuals.HandleWalk;

            CameraController.OnRotationUpdate += _playerMovementService.UpdateRotation;
        }

        PlayerInventory.Init(_player);
        PlayerInventory.OnSelectedItem += PlayerVisuals.HandleItemInHandSelect;
        PlayerInventory.OnDeselectedItem += PlayerVisuals.HandleItemInHandDeselect;
        if (_player.IsOwner)
        {
            Input.OnNextInventorySlot += PlayerInventory.HandleNextInventorySlotInput;
            Input.OnPreviousInventorySlot += PlayerInventory.HandlePreviousInventorySlotInput;
        }
        
        
        PlayerInteraction.Init(_player);
        if(_player.IsOwner)
        {
            Input.OnInteract += PlayerInteraction.HandleInteractInput;
            Input.OnDrop += PlayerInteraction.HandleDropInput;
            PlayerInteraction.OnInteractPickable += PlayerInventory.HandleInteractPickable;
            PlayerInteraction.OnInteractInteractable += PlayerInventory.HandleInteractInteractable;
            PlayerInteraction.OnDrop += PlayerInventory.HandleDropInput;

        }

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

}