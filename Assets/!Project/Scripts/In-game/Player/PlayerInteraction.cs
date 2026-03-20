using UnityEngine;
using System.Collections;
using System;

public class PlayerInteraction : MonoBehaviour
{
    Player _player;

    [field: SerializeField] public float InteractionRange { get; private set; } = 3f;
    [SerializeField] LayerMask _interactionLayer;
    [SerializeField] SphereCollider _sphereCollider;
    [SerializeField] float _longPressInteractDuration = 3f;
    bool _isInteractionPressed = false;
    float _interactionTimer = 0f;

    public IPickable _hoveredPickable { get; private set; } = null;
    public IInteractable _hoveredInteractable { get; private set; } = null;
    public Player _hoveredPlayer { get; private set; } = null;

    public event Action<IPickable> OnInteractPickable;
    public event Action<IInteractable> OnInteractInteractable;
    public event Action<Player> OnInteractPlayer;
    public event Action OnDrop;

    public bool IsInitialized { get; private set; } = false;

    public void Init(Player player)
    {
        _player = player;

        _sphereCollider.radius = InteractionRange;

        GlobalInputManager.Input.OnInteract += HandleInteractPressed;
        GlobalInputManager.Input.OnInteractReleased += HandleInteractReleased;
        GlobalInputManager.Input.OnDrop += HandleDropInput;

        IsInitialized = true;
    }

    private void OnDestroy()
    {
        Clear();

    }

    public void Clear()
    {
        GlobalInputManager.Input.OnInteract -= HandleInteractPressed;
        GlobalInputManager.Input.OnInteractReleased -= HandleInteractReleased;
        GlobalInputManager.Input.OnDrop -= HandleDropInput;
    }

    private void FixedUpdate()
    {
        if (_hoveredPlayer != null && _interactionTimer >= _longPressInteractDuration)
        {
            HandleInteractPlayer(_hoveredPlayer);
            OnInteractPlayer?.Invoke(_hoveredPlayer);
        }
    }

    public void HandleRaycast(Collider collider)
    {
        if (collider == null)
        {
            _hoveredPickable = null;
            _hoveredInteractable = null;
            _hoveredPlayer = null;

            HandleHintables();
            return;
        }

        _hoveredPickable = CheckForPickable(collider);
        _hoveredInteractable = CheckForInteractable(collider);
        _hoveredPlayer = CheckForPlayer(collider);

        HandleHintables();
    }

    void HandleHintables()
    {
        if (_hoveredPlayer != null && _hoveredPlayer is IHintable hintablePlayer)
        {
            if (_isInteractionPressed)
            {
                _interactionTimer += Time.deltaTime;
                _interactionTimer = Mathf.Clamp(_interactionTimer, 0, _longPressInteractDuration);
            }

            float interactionProgress = _interactionTimer / _longPressInteractDuration;
            HintsUI.Instance.SetHint(hintablePlayer, interactionProgress);
            return;
        }
        else if (_hoveredInteractable != null && _hoveredInteractable is IHintable hintableInteractable)
        {
            HintsUI.Instance.SetHint(hintableInteractable);
            return;
        }
        else if (_hoveredPickable != null && _hoveredPickable is IHintable hintablePickable)
        {
            HintsUI.Instance.SetHint(hintablePickable);
            return;
        }

        HintsUI.Instance.SetHint(null);
    }

    public void HandleInteractPressed()
    {
        _interactionTimer = 0f;
        _isInteractionPressed = true;

        if (_hoveredPickable != null)
        {
            OnInteractPickable?.Invoke(_hoveredPickable);
            return;
        }

        if (_hoveredInteractable != null)
        {
            OnInteractInteractable?.Invoke(_hoveredInteractable);
        }
    }

    public void HandleInteractReleased()
    {
        _interactionTimer = 0f;
        _isInteractionPressed = false;

    }

    public void HandleInteractPlayer(Player player)
    {
        if (player.IsKnockedDown)
        {
            player.RPC_RequestRevive();
        }
    }

    public void HandleDropInput()
    {
        OnDrop?.Invoke();
    }

    IPickable CheckForPickable(Collider collider)
    {
        IPickable pickable = null;

        if (collider.TryGetComponent<IPickable>(out IPickable foundPickable))
        {
            pickable = foundPickable;
        }
        return pickable;
    }
    IInteractable CheckForInteractable(Collider collider)
    {
        IInteractable interactable = null;

        if (collider.TryGetComponent<IInteractable>(out IInteractable foundInteractable))
        {
            interactable = foundInteractable;
        }
        return interactable;
    }
    Player CheckForPlayer(Collider collider)
    {
        Player player = null;

        if (collider.TryGetComponent<Player>(out Player foundPlayer))
        {
            if (foundPlayer.IsKnockedDown)
            {
                player = foundPlayer;
            }
        }
        return player;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (_interactionLayer.Contains(other.gameObject.layer))
        {
            IOutlinable outlinable = other.GetComponent<IOutlinable>();
            if (outlinable != null)
            {
                outlinable.SetOutlineVisibility(true);
            }

            Player player = other.GetComponent<Player>();
            if(player != null && !player.IsOwner)
            {
                player.PlayerController.SetPlayerUIVisibility(true);
            }

            //Debug.Log($"[PlayerInventory] OnTriggerEnter {other.gameObject.name}");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (_interactionLayer.Contains(other.gameObject.layer))
        {
            IOutlinable outlinable = other.GetComponent<IOutlinable>();
            if (outlinable != null)
            {
                outlinable.SetOutlineVisibility(false);
            }

            Player player = other.GetComponent<Player>();
            if (player != null && !player.IsOwner)
            {
                player.PlayerController.SetPlayerUIVisibility(false);
            }
            //Debug.Log($"[PlayerInventory] OnTriggerExit {other.gameObject.name}");
        }
    }
}