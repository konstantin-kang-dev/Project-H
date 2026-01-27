using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    Player _player;
    [SerializeField] int _capacity = 3;
    [SerializeField] float _interactionRange = 3f;
    [SerializeField] LayerMask _interactionLayer;

    IInput _input;
    
    List<IPickable> _items = new List<IPickable>();
    IPickable _selectedItem = null;
    public IPickable SelectedItem => _selectedItem;
    IPickable _hoveredPickable = null;
    IInteractable _hoveredInteractable = null;

    public bool IsInitialized { get; private set; } = false;
    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public void Init(Player player)
    {
        _player = player;
        _input = new DefaultInput();
        _input.OnInteract += HandleInteractInput;

        IsInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        if (IsOwner)
        {

        }
    }

    void PickUp(IPickable item)
    {
        if (_items.Count >= _capacity) return;

        Debug.Log($"[PlayerInventory] Picked up item: {item.Name}");

        item.PickUp(_player.ObjectId);

        _items.Add(item);
    }

    void Drop()
    {
        if(_selectedItem == null) return;
        _selectedItem.Drop();
    }
    
    void DropAll()
    {
        foreach (var item in _items)
        {
            item.Drop();
        }
    }

    public void SelectItem(bool goForward)
    {
        int index = _items.IndexOf(_selectedItem);
        if (goForward)
        {
            if (index >= _items.Count - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
        }
        else
        {
            if(index == 0)
            {
                index = _items.Count - 1;
            }
            else
            {
                index--;
            }
        }

        SelectItem(index);
    }

    public void SelectItem(int index)
    {
        if(index < 0 || index >= _items.Count)
        {
            throw new System.Exception($"[PlayerInventory] Index of item is out of range.");
        }

        _selectedItem = _items[index];
    }

    void HandleInteractInput()
    {
        Debug.Log($"[PlayerInventory] Interact");

        if (_hoveredPickable != null)
        {
            PickUp(_hoveredPickable);
            return;
        }

        if(_hoveredInteractable != null)
        {
            _hoveredInteractable.Interact();
        }
    }
    public void HandleRaycast(Collider collider)
    {
        if (collider == null)
        {
            _hoveredPickable = null;
            _hoveredInteractable = null;
            return;
        }

        _hoveredPickable = CheckForPickable(collider);
        _hoveredInteractable = CheckForInteractable(collider);
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


    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (_interactionLayer.Contains(other.gameObject.layer))
        {
            IPickable pickable = other.GetComponent<IPickable>();
            if (pickable == null) return;
            
            pickable.SetHighlight(true);
            Debug.Log($"[PlayerInventory] OnTriggerEnter {other.gameObject.name}");
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
            Debug.Log($"[PlayerInventory] OnTriggerExit {other.gameObject.name}");
        }
    }
}