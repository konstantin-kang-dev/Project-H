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
    public void RPC_RequestSendMessage(string message, NetworkConnection conn = null)
    {
        NetworkPlayerData networkPlayerData = RoomManager.Instance.GetNetworkPlayerData(conn.ClientId);
        if (networkPlayerData == null) throw new Exception($"[ChatManager] Could'nt get network data for player: {conn.ClientId}");

        ChatMessageData data = new ChatMessageData()
        {
            Message = message,
            OwnerPlayerName = networkPlayerData.PlayerName,
            Timestamp = (float)base.TimeManager.Tick
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
