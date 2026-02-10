using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager: NetworkBehaviour
{
    public static RoomManager Instance;

    readonly SyncDictionary<int, NetworkPlayerData> _connectedPlayers = new SyncDictionary<int, NetworkPlayerData>();
    public IReadOnlyDictionary<int, NetworkPlayerData> ConnectedPlayers => _connectedPlayers;
    public int ConnectedPlayersCount => ConnectedPlayers.Values.ToList().Count;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChange;
        _connectedPlayers.Clear();
    }

    void OnRemoteConnectionStateChange(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case RemoteConnectionState.Started:
                _connectedPlayers.Add(conn.ClientId, new NetworkPlayerData()
                {
                    ClientId = conn.ClientId,
                });
                break;
            case RemoteConnectionState.Stopped:
                _connectedPlayers.Remove(conn.ClientId);
                break;
        }

        if(LobbyManager.Instance != null)
        {
            LobbyManager.Instance.UpdateLobbyData();
        }
    }

    public NetworkPlayerData GetNetworkPlayerData(int clientId)
    {
        if (!ConnectedPlayers.ContainsKey(clientId)) return null;

        return ConnectedPlayers[clientId];
    }

    [Server]
    public void SERVER_UpdateNetworkPlayerData(int clientId, NetworkPlayerData playerData)
    {
        _connectedPlayers[clientId] = playerData;
    }
}