using System;
using UnityEngine;

public struct ChatMessageData
{
    public string OwnerPlayerName;
    public string Message;
    public int OwnerColorPreset;
    public float Timestamp;
    public ChatMessageType MessageType;
}

[Serializable]
public enum ChatMessageType
{
    Notification = 0,
    PlayerMessage = 1,
}