using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerRoomManager: MonoBehaviour
{
    public static ServerRoomManager Instance;
    public static event Action OnManagerReady;

    NetworkManager _networkManager;
    Dictionary<int, NetworkPlayerData> _connectedPlayers = new Dictionary<int, NetworkPlayerData>();
    public IReadOnlyDictionary<int, NetworkPlayerData> ConnectedPlayers => _connectedPlayers;
    public int ConnectedPlayersCount => ConnectedPlayers.Values.ToList().Count;

    private void Awake()
    {
        Instance = this; 

        _networkManager = InstanceFinder.NetworkManager;
    }

    public void Init()
    {
        NetworkGameManager.Instance.OnClientDisconnected += SERVER_RemoveConnectedPlayer;
        NetworkGameManager.Instance.OnClientLoadedStartScene += HandleClientLoadedStartScene;
        _connectedPlayers.Clear();
    }
    public void Clear()
    {
        NetworkGameManager.Instance.OnClientLoadedStartScene -= HandleClientLoadedStartScene;
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

    public void SERVER_SetPlayerData(GameData data, NetworkConnection conn)
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
}