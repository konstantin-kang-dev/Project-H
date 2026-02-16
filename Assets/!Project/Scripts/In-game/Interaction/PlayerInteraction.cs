using UnityEngine;
using System.Collections;
using System;

public class PlayerInteraction : MonoBehaviour
{
    Player _player;

    [SerializeField] float _interactionRange = 3f;
    [SerializeField] LayerMask _interactionLayer;

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

        IsInitialized = true;
    }

    public void HandleRaycast(Collider collider)
    {
        if (collider == null)
        {
            _hoveredPickable = null;
            _hoveredInteractable = null;
            _hoveredPlayer = null;
            return;
        }

        _hoveredPickable = CheckForPickable(collider);
        _hoveredInteractable = CheckForInteractable(collider);
        _hoveredPlayer = CheckForPlayer(collider);
    }

    public void HandleInteractInput()
    {
        if (_hoveredPickable != null)
        {
            OnInteractPickable?.Invoke(_hoveredPickable);
            return;
        }

        if (_hoveredInteractable != null)
        {
            OnInteractInteractable?.Invoke(_hoveredInteractable);
        }

        if(_hoveredPlayer != null)
        {
            HandleInteractPlayer(_hoveredPlayer);
            OnInteractPlayer?.Invoke(_hoveredPlayer);
        }
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
            if (player.IsKnockedDown)
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
            IPickable pickable = other.GetComponent<IPickable>();
            if (pickable == null) return;

            pickable.SetHighlight(true);
            //Debug.Log($"[PlayerInventory] OnTriggerEnter {other.gameObject.name}");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (_interactionLayer.Contains(other.gameObject.layer))
        {
            IPickable pickable = other.GetComponent<IPickable>();
            if (pickable == null) return;

            pickable.SetHighlight(false);
            //Debug.Log($"[PlayerInventory] OnTriggerExit {other.gameObject.name}");
        }
    }
}