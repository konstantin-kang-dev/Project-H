using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    readonly SyncList<ChatMessageData> _chatMessages = new SyncList<ChatMessageData>();

    public event Action<ChatMessageData> OnChatMessageAdded;
    public event Action OnChatClear;
    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _chatMessages.OnChange += HandleMessageUpdate;
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    [Server]
    public void ClearChat()
    {
        _chatMessages.Clear();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RPC_RequestSendMessage(string message, ChatMessageType chatMessageType, NetworkConnection conn)
    {
        NetworkPlayerData networkPlayerData = ServerRoomManager.Instance.GetNetworkPlayerData(conn.ClientId);

        ChatMessageData data = new ChatMessageData()
        {
            Message = message,
            OwnerPlayerName = networkPlayerData.PlayerName,
            OwnerColorPreset = networkPlayerData.PlayerIndex,
            Timestamp = (float)base.TimeManager.Tick,
            MessageType = chatMessageType,
        };

        _chatMessages.Add(data);
    }

    [Server]
    public void SERVER_SendMessage(string message, ChatMessageType chatMessageType, int clientId)
    {
        NetworkPlayerData networkPlayerData = ServerRoomManager.Instance.GetNetworkPlayerData(clientId);

        ChatMessageData data = new ChatMessageData()
        {
            Message = message,
            OwnerPlayerName = networkPlayerData.PlayerName,
            OwnerColorPreset = networkPlayerData.PlayerIndex,
            Timestamp = (float)base.TimeManager.Tick,
            MessageType = chatMessageType,
        };

        _chatMessages.Add(data);
    }

    private void HandleMessageUpdate(SyncListOperation op, int index, ChatMessageData oldItem, ChatMessageData newItem, bool asServer)
    {
        if (asServer) return;

        switch (op)
        {
            case SyncListOperation.Add:
                OnChatMessageAdded?.Invoke(newItem);
                break;
            case SyncListOperation.Clear:
                OnChatClear?.Invoke();
                break;
        }
    }
}
