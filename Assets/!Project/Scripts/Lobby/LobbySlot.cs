using UnityEditor;
using UnityEngine;

public struct LobbySlot
{
    public int SlotKey;
    public int ClientId;
    public bool IsOccupied;

    public LobbySlot(int slotKey, int clientId, bool isOccupied)
    {
        SlotKey = slotKey;
        ClientId = clientId;
        IsOccupied = isOccupied;
    }
}