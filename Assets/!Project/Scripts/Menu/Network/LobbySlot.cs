using UnityEditor;
using UnityEngine;

public class LobbySlot
{
    public int SlotKey = 0;
    public int ClientId = -1;
    public bool IsOccupied = false;

    public void ResetData()
    {
        ClientId = -1;
        IsOccupied = false;
    }
}