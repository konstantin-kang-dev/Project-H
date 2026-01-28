using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    Player _player;
    [SerializeField] int _capacity = 5;
    [SerializeField] float _interactionRange = 3f;
    [SerializeField] LayerMask _interactionLayer;

    IInput _input;
    
    Dictionary<int, IPickable> _items = new Dictionary<int, IPickable>();
    public Dictionary<int, IPickable> Items => _items;

    int _selectedItemIndex = 0;
    IPickable _selectedItem = null;
    public IPickable SelectedItem => _selectedItem;
    public event Action<int> OnSelectedItem;

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

        _items.Clear();
        for (int i = 0; i < _capacity; i++)
        {
            _items[i] = null;
        }

        _input = new DefaultInput();
        _input.OnInteract += HandleInteractInput;
        _input.OnDrop += HandleDropInput;
        _input.OnNextInventorySlot += HandleNextInventorySlotInput;
        _input.OnPreviousInventorySlot += HandlePreviousInventorySlotInput;

        SelectItem(0);

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

        Debug.Log($"[PlayerInventory] Picked up item: {item.ItemConfig.Type}");

        item.PickUp(_player.ObjectId);

        int chosenIndex = 0;
        if(_selectedItem == null)
        {
            chosenIndex = _selectedItemIndex;
            _items[chosenIndex] = item;
        }
        else
        {
            chosenIndex = GetFreeItemIndex();
            _items[chosenIndex] = item;
        }

        SelectItem(chosenIndex);
    }

    void Drop()
    {
        if(_selectedItem == null) return;
        _selectedItem.Drop();

        Debug.Log($"[PlayerInventory] Droped up item: {_selectedItem.ItemConfig.Type}");

        _items[_selectedItemIndex] = null;
    }
    
    void DropAll()
    {
        foreach (var item in _items)
        {
            if(item.Value != null)
            {
                item.Value.Drop();
            }
            
        }
    }

    public void SelectItem(bool goForward)
    {
        int index = _selectedItemIndex;
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
            throw new System.Exception($"[PlayerInventory] Index ({index}) of item is out of range.");
        }

        _selectedItem = _items[index];
        _selectedItemIndex = index;
        OnSelectedItem?.Invoke(index);
    }

    void HandleInteractInput()
    {
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
    void HandleDropInput()
    {

        if (_selectedItem == null) return;

        Drop();
    }
    void HandleNextInventorySlotInput()
    {
        SelectItem(true);
    }
    void HandlePreviousInventorySlotInput()
    {
        SelectItem(false);
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

    bool IsFullfilled()
    {
        bool result = true;

        foreach(var item in _items)
        {
            if(item.Value == null)
            {
                result = false;
                break;
            }
        }
        return result;
    }
    int GetFreeItemIndex()
    {
        int freeIndex = 0;

        foreach (var item in _items)
        {
            if(item.Value == null)
            {
                break;
            }

            freeIndex++;
        }

        return freeIndex;
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