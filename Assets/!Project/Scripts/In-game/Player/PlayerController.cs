using FishNet.Component.Animating;
using System;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Player _player;

    CharacterController _characterController;
    CapsuleCollider _capsuleCollider;

    PlayerStatsConfig _playerStatsConfig;

    [field: SerializeField] public PlayerVisuals PlayerVisuals { get; private set; }
    [field: SerializeField] public CharacterAudioService CharacterAudioService { get; private set; }

    public PlayerStaminaService PlayerStaminaService { get; private set; }
    [SerializeField] PlayerMovementService _playerMovementService;

    [SerializeField] CameraController _cameraControllerPrefab;
    public CameraController CameraController { get; private set; }

    [field: SerializeField] public PlayerInventory PlayerInventory { get; private set; }
    [field: SerializeField] public PlayerInteraction PlayerInteraction { get; private set; }
    [field: SerializeField] public PlayerUI PlayerUI { get; private set; }

    public static event Action OnInitialized;
    public bool IsInitialized { get; private set; } = false;
    private void Start()
    {

    }

    public void Init(Player player)
    {
        _player = player;

        _characterController = GetComponent<CharacterController>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        GameDifficultyConfig difficultyConfig = GameDifficultyManager.Instance.SelectedConfig;
        _playerStatsConfig = difficultyConfig.PlayersStats.Clone();

        if (_player.IsOwner)
        {
            CameraController = Instantiate(_cameraControllerPrefab, transform);
            CameraController.Init();

            CameraController.OnRaycast += PlayerInteraction.HandleRaycast;
        }

        PlayerVisuals.Init(_player.ModelKey);

        _playerMovementService.Init(_playerStatsConfig, _characterController);

        if (_player.IsOwner)
        {
            PlayerStaminaService = new PlayerStaminaService(1f);
            PlayerStaminaService.OnStaminaUpdate += _playerMovementService.HandleUpdateStamina;
            PlayerStaminaService.OnStaminaEmpty += CharacterAudioService.HandleStaminaEmpty;

            _playerMovementService.OnWalk += PlayerStaminaService.HandleWalk;
            _playerMovementService.OnJump += PlayerStaminaService.HandleJump;

            PlayerVisuals.AnimatorController.SetHeadVisibility(false);
            PlayerVisuals.OnAnimatorStateChanged += CameraController.HandleAnimationChange;

            _playerMovementService.OnWalk += PlayerVisuals.HandleWalk;
            _playerMovementService.OnJump += PlayerVisuals.HandleJump;
            _playerMovementService.OnLand += PlayerVisuals.HandleLand;
            _playerMovementService.OnCrouchingChange += PlayerVisuals.HandleCrouch;

            CameraController.OnLookPositionUpdate += PlayerVisuals.AnimatorController.SetLookPosition;
            CameraController.OnLookPositionUpdate += _player.RPC_RequestSetLookPosition;
            CameraController.OnRotationUpdate += _playerMovementService.UpdateRotation;
            _playerMovementService.OnSprint += CameraController.AdjustFov;

        }

        PlayerInventory.Init(_player);
        PlayerInventory.OnSelectedItem += PlayerVisuals.HandleItemInHandSelect;
        PlayerInventory.OnDeselectedItem += PlayerVisuals.HandleItemInHandDeselect;
        
        PlayerInteraction.Init(_player);
        if(_player.IsOwner)
        {
            PlayerInteraction.OnInteractPickable += PlayerInventory.HandleInteractPickable;
            PlayerInteraction.OnInteractInteractable += PlayerInventory.HandleInteractInteractable;
            PlayerInteraction.OnDrop += PlayerInventory.HandleDropInput;
            PlayerInventory.OnItemPickUp += CharacterAudioService.HandlePickUp;
        }


        IsInitialized = true;

        OnInitialized?.Invoke();
        if (_player.IsOwner)
        {
            Debug.Log($"[PlayerController] Initialized.");
        }
    }

    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        if(_player.IsOwner)
        {
            PlayerStaminaService.Tick(Time.fixedDeltaTime);
        }
    }

    public void SetPlayerData(NetworkPlayerData playerData)
    {
        Sprite playerAvatar = NetworkRoomManager.Instance.GetPlayerAvatar(playerData.ClientId);

        PlayerUI.SetData(playerData.PlayerName, playerAvatar);
    }

    public void SetPlayerUIVisibility(bool visible)
    {
        PlayerUI.SetVisibility(visible);
    }

    public void SetLookPosition(Vector3 lookPosition)
    {
        if (!IsInitialized) return;

        PlayerVisuals.AnimatorController.SetLookPosition(lookPosition);
    }

    public void HandleKnockDown(bool value)
    {
        if (value)
        {
            PlayerVisuals.AnimatorController.SetState(AnimatorState.KnockDown);
            _playerMovementService.SetMoveAbility(false);
        }
        else
        {
            PlayerVisuals.AnimatorController.SetState(AnimatorState.Idle);
            _playerMovementService.SetMoveAbility(true);
        }
    }

    public void HandleEndGame()
    {
        if(_player.IsOwner)
        {
            PlayerInteraction.Clear();
            PlayerInventory.Clear();
            _playerMovementService.Clear();
            CameraController.Clear();

            PlayerStaminaService.OnStaminaUpdate -= _playerMovementService.HandleUpdateStamina;
            PlayerStaminaService.OnStaminaEmpty -= CharacterAudioService.HandleStaminaEmpty;

            _playerMovementService.OnWalk -= PlayerStaminaService.HandleWalk;
            _playerMovementService.OnJump -= PlayerStaminaService.HandleJump;

            PlayerVisuals.AnimatorController.SetHeadVisibility(false);
            PlayerVisuals.OnAnimatorStateChanged -= CameraController.HandleAnimationChange;

            _playerMovementService.OnWalk -= PlayerVisuals.HandleWalk;
            _playerMovementService.OnJump -= PlayerVisuals.HandleJump;
            _playerMovementService.OnLand -= PlayerVisuals.HandleLand;
            _playerMovementService.OnCrouchingChange -= PlayerVisuals.HandleCrouch;

            CameraController.OnLookPositionUpdate -= PlayerVisuals.AnimatorController.SetLookPosition;
            CameraController.OnLookPositionUpdate -= _player.RPC_RequestSetLookPosition;
            CameraController.OnRotationUpdate -= _playerMovementService.UpdateRotation;
            _playerMovementService.OnSprint -= CameraController.AdjustFov;

        }

        PlayerVisuals.AnimatorController.SetState(AnimatorState.Idle);
    }

}