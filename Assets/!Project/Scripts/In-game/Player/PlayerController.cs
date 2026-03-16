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
    [SerializeField] public PlayerMovementService PlayerMovementService;

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

        PlayerMovementService.Init(_playerStatsConfig, _characterController);

        if (_player.IsOwner)
        {
            PlayerStaminaService = new PlayerStaminaService(1f);
            PlayerStaminaService.OnStaminaUpdate += PlayerMovementService.HandleUpdateStamina;
            PlayerStaminaService.OnStaminaEmpty += CharacterAudioService.HandleStaminaEmpty;

            PlayerMovementService.OnWalk += PlayerStaminaService.HandleWalk;
            PlayerMovementService.OnJump += PlayerStaminaService.HandleJump;

            PlayerVisuals.AnimatorController.SetHeadVisibility(false);
            PlayerVisuals.OnAnimatorStateChanged += CameraController.HandleAnimationChange;

            PlayerMovementService.OnWalk += PlayerVisuals.HandleWalk;
            PlayerMovementService.OnJump += PlayerVisuals.HandleJump;
            PlayerMovementService.OnLand += PlayerVisuals.HandleLand;
            PlayerMovementService.OnCrouchingChange += PlayerVisuals.HandleCrouch;

            CameraController.OnLookPositionUpdate += PlayerVisuals.AnimatorController.SetLookPosition;
            CameraController.OnLookPositionUpdate += _player.RPC_RequestSetLookPosition;
            CameraController.OnRotationUpdate += PlayerMovementService.UpdateRotation;
            PlayerMovementService.OnSprint += CameraController.AdjustFov;

        }

        PlayerInventory.Init(_player);
        PlayerInventory.OnSelectedItem += PlayerVisuals.HandleItemInHandSelect;
        PlayerInventory.OnDeselectedItem += PlayerVisuals.HandleItemInHandDeselect;
        
        PlayerInteraction.Init(_player);
        if(_player.IsOwner)
        {
            CameraController.SetRaycastDistance(PlayerInteraction.InteractionRange);
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
        if (_player.IsOwner)
        {
            if (value)
            {
                PlayerVisuals.AnimatorController.SetState(AnimatorState.KnockDown);
                PlayerMovementService.SetMoveAbility(false);
                CharacterAudioService.Play(CharacterAudioType.HeavyBreath);
            }
            else
            {
                PlayerVisuals.AnimatorController.SetState(AnimatorState.Idle, true);
                PlayerMovementService.SetMoveAbility(true);
            }
        }
        else
        {
            
        }

        PlayerVisuals.HandleKnockedDownChange(value);
    }

    public void HandleEndGame()
    {
        if(_player.IsOwner)
        {
            PlayerInteraction.Clear();
            PlayerInventory.Clear();
            PlayerMovementService.Clear();
            CameraController.Clear();

            PlayerStaminaService.OnStaminaUpdate -= PlayerMovementService.HandleUpdateStamina;
            PlayerStaminaService.OnStaminaEmpty -= CharacterAudioService.HandleStaminaEmpty;

            PlayerMovementService.OnWalk -= PlayerStaminaService.HandleWalk;
            PlayerMovementService.OnJump -= PlayerStaminaService.HandleJump;

            PlayerVisuals.AnimatorController.SetHeadVisibility(false);
            PlayerVisuals.OnAnimatorStateChanged -= CameraController.HandleAnimationChange;

            PlayerMovementService.OnWalk -= PlayerVisuals.HandleWalk;
            PlayerMovementService.OnJump -= PlayerVisuals.HandleJump;
            PlayerMovementService.OnLand -= PlayerVisuals.HandleLand;
            PlayerMovementService.OnCrouchingChange -= PlayerVisuals.HandleCrouch;

            CameraController.OnLookPositionUpdate -= PlayerVisuals.AnimatorController.SetLookPosition;
            CameraController.OnLookPositionUpdate -= _player.RPC_RequestSetLookPosition;
            CameraController.OnRotationUpdate -= PlayerMovementService.UpdateRotation;
            PlayerMovementService.OnSprint -= CameraController.AdjustFov;

        }

        PlayerVisuals.AnimatorController.SetState(AnimatorState.Idle);
    }

}