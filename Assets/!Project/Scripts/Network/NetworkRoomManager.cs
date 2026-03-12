using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using GameAudio;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;

public class NetworkRoomManager: NetworkBehaviour
{
    public static NetworkRoomManager Instance;

    NetworkManager _networkManager;
    readonly SyncDictionary<int, NetworkPlayerData> _connectedPlayers = new SyncDictionary<int, NetworkPlayerData>();
    public IReadOnlyDictionary<int, NetworkPlayerData> ConnectedPlayers => _connectedPlayers;
    public int ConnectedPlayersCount => ConnectedPlayers.Values.ToList().Count;

    public Dictionary<int, Sprite> PlayersAvatars { get; private set; } = new Dictionary<int, Sprite>();

    public event Action<NetworkPlayerData> OnConnectedPlayer;
    public event Action<NetworkPlayerData> OnUpdatedPlayer;
    public event Action<NetworkPlayerData> OnDisconnectedPlayer;

    private void Awake()
    {
        Instance = this; 

        _networkManager = InstanceFinder.NetworkManager;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _connectedPlayers.OnChange += HandleConnectedPlayersChange;
        Debug.Log($"[NetworkRoomManager] OnStartClient");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        CLIENT_Clear();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        SERVER_Clear();
    }

    [Server]
    public void SERVER_Init()
    {
        NetworkGameManager.Instance.OnClientDisconnected += SERVER_RemoveConnectedPlayer;
        NetworkGameManager.Instance.OnClientLoadedStartScene += HandleClientLoadedStartScene;
        _connectedPlayers.Clear();
    }

    [Client]
    public void CLIENT_Clear()
    {
        _connectedPlayers.OnChange -= HandleConnectedPlayersChange;
        OnConnectedPlayer = null;
        OnUpdatedPlayer = null;
        OnDisconnectedPlayer = null;
    }

    [Server]
    public void SERVER_Clear()
    {
        NetworkGameManager.Instance.OnClientDisconnected -= SERVER_RemoveConnectedPlayer;
        NetworkGameManager.Instance.OnClientLoadedStartScene -= HandleClientLoadedStartScene;
        _connectedPlayers.Clear();
    }

    void HandleClientLoadedStartScene(NetworkConnection conn, bool asServer)
    {
        if (asServer)
        {
            Debug.Log($"[RoomManager] Player[{conn.ClientId}] loaded start scenes.");

            SERVER_AddConnectedPlayer(conn.ClientId);
        }
        else
        {

        }
    }

    private void HandleConnectedPlayersChange(SyncDictionaryOperation op, int key, NetworkPlayerData value, bool asServer)
    {
        switch (op)
        {
            case SyncDictionaryOperation.Add:
                HandlePlayerAdded(value);
                OnConnectedPlayer?.Invoke(value);
                break;
            case SyncDictionaryOperation.Remove:
                HandlePlayerRemoved(value);
                OnDisconnectedPlayer?.Invoke(value);
                break;
            case SyncDictionaryOperation.Set:
                HandlePlayerAdded(value);
                OnUpdatedPlayer?.Invoke(value);
                break;
        }
    }

    async void HandlePlayerAdded(NetworkPlayerData playerData)
    {
        Sprite avatar = null;

        if (!string.IsNullOrEmpty(playerData.PlayerSteamId))
        {
            Texture2D texture = await NetworkGameManager.Instance.GetSteamAvatar(playerData.PlayerSteamId);
            avatar = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            Debug.Log($"[NetworkRoomManager] Loaded {playerData.PlayerName}'s avatar");
        }

        PlayersAvatars[playerData.ClientId] = avatar;
    }

    async void HandlePlayerRemoved(NetworkPlayerData playerData)
    {
        if (PlayersAvatars.ContainsKey(playerData.ClientId))
        {
            PlayersAvatars.Remove(playerData.ClientId);

        }
    }

    void SERVER_AddConnectedPlayer(int clientId)
    {
        if (_connectedPlayers.ContainsKey(clientId)) throw new System.Exception($"[RoomManager] Player already exists in connected players list.");

        NetworkPlayerData networkPlayerData = new NetworkPlayerData();
        networkPlayerData.ClientId = clientId;

        LobbySlot lobbySlot = LobbyManager.Instance.SERVER_GetFreeLobbySlot();
        networkPlayerData.PlayerIndex = lobbySlot.SlotKey;

        _connectedPlayers.Add(clientId, networkPlayerData);
        LobbyManager.Instance.SERVER_OccupyLobbySlot(networkPlayerData, lobbySlot);
        LobbyManager.Instance.SERVER_UpdateLobbyData();
    }


    void SERVER_RemoveConnectedPlayer(NetworkConnection conn)
    {
        int clientId = conn.ClientId;
        if (!_connectedPlayers.ContainsKey(clientId)) throw new System.Exception($"[RoomManager] Player doesn't exists in connected players list.");

        NetworkPlayerData networkPlayerData = _connectedPlayers[clientId];

        ChatManager.Instance.SERVER_SendMessage("has left the game.", ChatMessageType.Notification, networkPlayerData.ClientId);
        _connectedPlayers.Remove(clientId);
        LobbyManager.Instance.SERVER_UpdateLobbyData();
    }

    public void SERVER_SetPlayerData(PlayerSave data, NetworkConnection conn)
    {
        if (!_connectedPlayers.ContainsKey(conn.ClientId)) throw new System.Exception($"[RoomManager] Player doesn't exists in connected players list.");

        NetworkPlayerData networkPlayerData = _connectedPlayers[conn.ClientId];
        networkPlayerData.PlayerName = data.PlayerName;
        networkPlayerData.PlayerSteamId = data.SteamId;

        _connectedPlayers[conn.ClientId] = networkPlayerData;
        Debug.Log($"[RoomManager] Player {data.PlayerName} sent his data.");
        ChatManager.Instance.SERVER_SendMessage("has joined the game.", ChatMessageType.Notification, networkPlayerData.ClientId);
    }


    public NetworkPlayerData GetNetworkPlayerData(int clientId)
    {
        if (!ConnectedPlayers.ContainsKey(clientId)) return new NetworkPlayerData();

        return ConnectedPlayers[clientId];
    }

    [Server]
    public void SERVER_UpdateNetworkPlayerData(int clientId, NetworkPlayerData playerData)
    {
        _connectedPlayers[clientId] = playerData;
    }

    [Server]

    public async void SERVER_LoadGameScene(float delay = 0)
    {
        await UniTask.WaitForSeconds(delay);

        SceneLoadData sld = new SceneLoadData("GameScene")
        {
            ReplaceScenes = ReplaceOption.All,
        };
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    public Sprite GetPlayerAvatar(int clientId)
    {
        if (PlayersAvatars.ContainsKey(clientId))
        {
            return PlayersAvatars[clientId];
        }
        else
        {
            return null;
        }

    }
}