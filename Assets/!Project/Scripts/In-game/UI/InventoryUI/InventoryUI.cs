using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] List<InventorySlotUI> _inventorySlots = new List<InventorySlotUI>();
    InventorySlotUI _lastSelectedSlot;

    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        
    }

    public void Init()
    {
        if (IsInitialized) return;

        Player localPlayer = GameManager.Instance.LocalPlayer;
        localPlayer.PlayerController.PlayerInventory.OnSelectedItem += HandleSelectItem;

        IsInitialized = true;

        Debug.Log($"[InventoryUI] Initialized");
    }

    void HandleSelectItem(int index)
    {
        if(index > _inventorySlots.Count - 1)
        {
            throw new System.Exception($"[InventoryUI] Index of selected item is out of bounds.");
        }

        if (_lastSelectedSlot == _inventorySlots[index]) return;

        if (_lastSelectedSlot != null)
        {
            _lastSelectedSlot.Deselect();
        }

        _lastSelectedSlot = _inventorySlots[index];
        _lastSelectedSlot.Select();
    }

    private void OnDestroy()
    {

    }

}
