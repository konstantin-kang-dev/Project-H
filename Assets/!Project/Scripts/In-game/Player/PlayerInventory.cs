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

    
    Dictionary<int, IPickable> _items = new Dictionary<int, IPickable>();
    public Dictionary<int, IPickable> Items => _items;

    int _selectedItemIndex = 0;
    IPickable _selectedItem = null;
    readonly SyncVar<InventorySelection> _inventorySelection = new SyncVar<InventorySelection>();
    public IPickable SelectedItem => _selectedItem;

    public event Action<IPickable, int> OnItemPickUp;
    public event Action<IPickable, int> OnItemDrop;
    public event Action<IPickable, int> OnSelectedItem;
    public event Action<IPickable, int> OnDeselectedItem;
         
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

        if (IsOwner)
        {

            GlobalInputManager.Input.OnInventorySlotKey += SelectItem;
            GlobalInputManager.Input.OnNextInventorySlot += HandleNextInventorySlotInput;
            GlobalInputManager.Input.OnPreviousInventorySlot += HandlePreviousInventorySlotInput;
            GlobalInputManager.Input.OnInteractWithItem += InteractWithItemInHands;

        }

        _items.Clear();
        for (int i = 0; i < _capacity; i++)
        {
            _items[i] = null;
        }

        _inventorySelection.OnChange += HandleSelectItem;

        if (IsOwner)
        {
            SelectItem(0);
        }


        IsInitialized = true;
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            GlobalInputManager.Input.OnInventorySlotKey -= SelectItem;
            GlobalInputManager.Input.OnNextInventorySlot -= HandleNextInventorySlotInput;
            GlobalInputManager.Input.OnPreviousInventorySlot -= HandlePreviousInventorySlotInput;
            GlobalInputManager.Input.OnInteractWithItem -= InteractWithItemInHands;
        }

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

        RPC_RequestPickUp(item.ItemObjectId);
    }

    [ServerRpc]
    void RPC_RequestPickUp(int itemObjectId)
    {
        if (!ServerManager.Objects.Spawned.TryGetValue(itemObjectId, out var obj)) return;

        var item = obj.GetComponent<IPickable>();
        if (item == null || item.IsPickedUp) return;

        item.SERVER_PickUp(_player.ObjectId);
        RPC_ConfirmPickUp(itemObjectId, _player.ObjectId);
    }

    [ObserversRpc]
    void RPC_ConfirmPickUp(int itemObjectId, int pickerObjectId)
    {
        var item = ClientManager.Objects.Spawned[itemObjectId].GetComponent<IPickable>();

        item.SetHighlight(false);
        item.SetColliders(false);

        if(IsOwner && pickerObjectId == _player.ObjectId)
        {
            HandlePickUp(item);
        }
    }

    public void HandlePickUp(IPickable item)
    {
        int chosenIndex = 0;
        if (_selectedItem == null)
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
        RPC_RequestDropItem(_selectedItem.ItemObjectId);
    }

    [ServerRpc]
    void RPC_RequestDropItem(int itemObjectId)
    {
        if (!ServerManager.Objects.Spawned.TryGetValue(itemObjectId, out var obj)) return;

        IPickable item = obj.GetComponent<IPickable>();
        if(item == null || !item.IsPickedUp) return;

        item.SERVER_Drop();

        _inventorySelection.Value = new InventorySelection()
        {
            ObjectId = -1,
            InventoryIndex = _inventorySelection.Value.InventoryIndex,
        };

        RPC_ConfirmDrop(itemObjectId);
    }
    [ObserversRpc]
    void RPC_ConfirmDrop(int itemObjectId)
    {
        if (!ClientManager.Objects.Spawned.TryGetValue(itemObjectId, out var obj)) return;
                
        _selectedItem = null;
        IPickable item = obj.GetComponent<IPickable>();

        item.SetVisibility(true);
        item.SetColliders(true);

        _player.PlayerController.PlayerVisuals.AnimatorController.SetItemInHand(item, false);
        Debug.Log($"[PlayerInventory] Dropped item: {item.ItemConfig.Type}");

        _items[_selectedItemIndex] = null;
        HandleDropItem(item);
    }

    public void InteractWithItemInHands()
    {
        if(_selectedItem == null) return;

        RPC_RequestInteractWithItem(_selectedItem.ItemObjectId);
    }

    [ServerRpc]
    void RPC_RequestInteractWithItem(int itemObjectId)
    {
        if (!ServerManager.Objects.Spawned.TryGetValue(itemObjectId, out var obj)) return;

        IPickable item = obj.GetComponent<IPickable>();
        if(item == null || !item.IsPickedUp) return;

        item.SERVER_Interact();
    }

    void HandleDropItem(IPickable item)
    {
        OnItemDrop?.Invoke(item, _selectedItemIndex);
    }

    void DropAll()
    {
        foreach (var item in _items)
        {
            if(item.Value != null)
            {
                item.Value.SERVER_Drop();
            }
        }
    }

    public void SelectItem(bool goForward)
    {
        int index = ProjectUtils.GetNextIndex(_selectedItemIndex, _items.Count - 1, goForward);

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
        if (asServer) return;

        if (_selectedItem != null)
        {
            _selectedItem.SetVisibility(false);
            OnDeselectedItem?.Invoke(_selectedItem, prev.InventoryIndex);
            //Debug.Log($"[PlayerInventory] Deselected item: {_selectedItem}");
        }

        IPickable selectedItem = null;

        if (ClientManager.Objects.Spawned.TryGetValue(next.ObjectId, out NetworkObject networkObject))
        {
            selectedItem = networkObject.GetComponent<IPickable>();

            if(selectedItem != null)
            {
                selectedItem.SetHighlight(false);
                selectedItem.SetVisibility(true);
            }
        }

        _selectedItemIndex = next.InventoryIndex;
        _selectedItem = selectedItem;

        //Debug.Log($"[PlayerInventory] Selected item: {_selectedItem} in slot: {_selectedItemIndex}");
        OnSelectedItem?.Invoke(_selectedItem, next.InventoryIndex);
    }

    public void HandleInteractPickable(IPickable pickable)
    {
        PickUp(pickable);
    }

    public void HandleInteractInteractable(IInteractable interactable)
    {
        interactable.Interact(_selectedItem);
    }

    public void HandleDropInput()
    {
        Drop();
    }
    public void HandleNextInventorySlotInput()
    {
        SelectItem(true);
    }
    public void HandlePreviousInventorySlotInput()
    {
        SelectItem(false);
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

    public override void OnStopClient()
    {
        base.OnStopClient();

        _inventorySelection.OnChange -= HandleSelectItem;

        if (IsOwner)
        {
            DropAll();
        }
    }
}