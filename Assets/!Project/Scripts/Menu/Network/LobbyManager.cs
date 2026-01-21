using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using FishNet.Transporting;
using UnityEngine;
using FishNet.Managing;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }
    [SerializeField] private NetworkManager _networkManager;

    [SerializeField] private GameObject _lobbyPlayerPrefab;
    private List<LobbyPlayer> _players = new();

    [SerializeField] private List<Transform> _lobbySpawnPoints = new();

    private void Awake()
    {
        Instance = this;
    }
    public void StartHost()
    {
        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();
        Debug.Log("Host started");
    }

    public void StartClient(string ip = "127.0.0.1")
    {
        _networkManager.ClientManager.StartConnection(ip);
        Debug.Log($"Client connecting to {ip}");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    private void OnRemoteConnectionState(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == FishNet.Transporting.RemoteConnectionState.Started)
        {
            SpawnLobbyPlayer(conn);
        }
    }

    [Server]
    private void SpawnLobbyPlayer(NetworkConnection conn)
    {
        NetworkObject nob = Instantiate(_lobbyPlayerPrefab).GetComponent<NetworkObject>();
        ServerManager.Spawn(nob, conn);

        _players.Add(nob.GetComponent<LobbyPlayer>());

        Transform spawnPoint = _lobbySpawnPoints[_players.Count - 1];
        nob.transform.position = spawnPoint.position;

        Debug.Log($"Spawned lobby player for connection {conn.ClientId}");
    }
}