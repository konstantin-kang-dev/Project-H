using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public struct InventorySelection
    {
        public int ObjectId;
        public int InventoryIndex;
    }

    Player _player;
    [SerializeField] int _capacity = 5;
    [SerializeField] float _interactionRange = 3f;
    [SerializeField] LayerMask _interactionLayer;

    IInput _input;
    
    Dictionary<int, IPickable> _items = new Dictionary<int, IPickable>();
    public Dictionary<int, IPickable> Items => _items;

    int _selectedItemIndex = 0;
    IPickable _selectedItem = null;
    readonly SyncVar<InventorySelection> _inventorySelection = new SyncVar<InventorySelection>();
    public IPickable SelectedItem => _selectedItem;

    public event Action<IPickable, int> OnItemPickUp;
    public event Action<IPickable, int> OnItemDrop;
    public event Action<IPickable, int> OnSelectedItem;

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

        _inventorySelection.Value = new InventorySelection()
        {
            InventoryIndex = 0,
            ObjectId = -1,
        };
    }

    public void Init(Player player)
    {
        _player = player;

        _items.Clear();
        for (int i = 0; i < _capacity; i++)
        {
            _items[i] = null;
        }

        _inventorySelection.OnChange += HandleSelectItem;

        if (IsOwner)
        {
            _input = new DefaultInput();
            _input.OnInteract += HandleInteractInput;
            _input.OnDrop += HandleDropInput;
            _input.OnNextInventorySlot += HandleNextInventorySlotInput;
            _input.OnPreviousInventorySlot += HandlePreviousInventorySlotInput;

            SelectItem(0);
        }


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
        if (IsFullfilled()) return;

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

        Debug.Log($"[PlayerInventory] Picked up item: {item.ItemConfig.Type} in slot: {chosenIndex}");

        OnItemPickUp?.Invoke(item, chosenIndex);
        SelectItem(chosenIndex);
    }

    void Drop()
    {
        if(_selectedItem == null) return;

        Debug.Log($"[PlayerInventory] Droped item: {_selectedItem.ItemConfig.Type}");

        RPC_RequestSelectItem(new InventorySelection()
        {
            ObjectId = -1,
            InventoryIndex = _selectedItemIndex,
        });

        _selectedItem.Drop();

        OnItemDrop?.Invoke(_selectedItem, _selectedItemIndex);

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


        InventorySelection inventorySelection = new InventorySelection()
        {
            InventoryIndex = index,
        };

        IPickable selectedItem = _items[index];

        if(selectedItem == null)
        {
            RPC_RequestSelectItem(inventorySelection);
        }
        else
        {
            inventorySelection.ObjectId = selectedItem.ItemObjectId;
            RPC_RequestSelectItem(inventorySelection);
        }
    }

    [ServerRpc]
    void RPC_RequestSelectItem(InventorySelection inventorySelection)
    {
        _inventorySelection.Value = inventorySelection;
    }

    [Client]
    void HandleSelectItem(InventorySelection prev, InventorySelection next, bool asServer)
    {
        if (_selectedItem != null)
        {
            _selectedItem.SetVisibility(false);
            _player.PlayerController.PlayerVisuals.AnimatorController.SetItemInHand(_selectedItem, false);
            //Debug.Log($"[PlayerInventory] Deselected item: {_selectedItem}");
        }

        IPickable selectedItem = null;

        if (ClientManager.Objects.Spawned.TryGetValue(next.ObjectId, out NetworkObject networkObject))
        {
            selectedItem = networkObject.GetComponent<IPickable>();

            selectedItem.SetHighlight(false);
            selectedItem.SetVisibility(true);

            _player.PlayerController.PlayerVisuals.AnimatorController.SetItemInHand(selectedItem, true);

        }

        _selectedItemIndex = next.InventoryIndex;
        _selectedItem = selectedItem;

        //Debug.Log($"[PlayerInventory] Selected item: {_selectedItem} in slot: {_selectedItemIndex}");
        OnSelectedItem?.Invoke(_selectedItem, next.InventoryIndex);
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
            _hoveredInteractable.Interact(_player, _selectedItem);
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

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (IsOwner)
        {
            DropAll();
        }
    }
}