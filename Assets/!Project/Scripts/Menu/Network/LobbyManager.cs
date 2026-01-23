using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using FishNet.Transporting;
using UnityEngine;
using FishNet.Managing;
using System;
using System.Linq;
using FishNet;
using FishNet.Object.Synchronizing;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] List<Transform> _lobbySlotsPoints = new List<Transform>();

    readonly SyncVar<List<LobbySlot>> _lobbySlots = new SyncVar<List<LobbySlot>>();

    int _maxPlayers = 4;

    [SerializeField] LobbyPlayer _lobbyPlayerPrefab;

    List<LobbyPlayer> _lobbyPlayers = new List<LobbyPlayer>();
    public List<LobbyPlayer> LobbyPlayers => _lobbyPlayers;

    LobbyPlayer _localLobbyPlayer;
    public bool IsLocalPlayerSet => _localLobbyPlayer != null;
    public bool LocalPlayerReadyState => IsLocalPlayerSet ? _localLobbyPlayer.IsReady : false;

    readonly SyncVar<Dictionary<int, NetworkPlayerData>> _connectedPlayers = new SyncVar<Dictionary<int, NetworkPlayerData>>();
    public Dictionary<int, NetworkPlayerData> ConnectedPlayers => _connectedPlayers.Value;

    public event Action OnLocalPlayerRegister;
    public event Action OnLocalPlayerUnregister;
    public event Action<bool> OnPlayersReady;
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

        SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
        ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChange;

        _connectedPlayers.Value = new Dictionary<int, NetworkPlayerData>();
        _lobbySlots.Value = new List<LobbySlot>()
        {
            new LobbySlot()
            {
                SlotKey = 0,
            },
            new LobbySlot()
            {
                SlotKey = 1,
            },
            new LobbySlot()
            {
                SlotKey = 2,
            },
            new LobbySlot()
            {
                SlotKey = 3,
            },
        };
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChange;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionStateChange;
    }

    private void OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (asServer)
        {
            SceneManager.AddConnectionToScene(conn, gameObject.scene);

            LobbySlot availableSlot = _lobbySlots.Value.First((x) => !x.IsOccupied);
            availableSlot.IsOccupied = true;
            availableSlot.ClientId = conn.ClientId;

            Transform slotPoint = _lobbySlotsPoints[availableSlot.SlotKey];

            LobbyPlayer lobbyPlayer = Instantiate(_lobbyPlayerPrefab, transform);
            Spawn(lobbyPlayer, conn);

            lobbyPlayer.transform.position = slotPoint.position;

            Debug.Log($"[LobbyManager] Added player[{conn.ClientId}] to slot: {availableSlot.SlotKey} and spawned lobby player object.");
        }
        else
        {

        }

        Debug.Log($"Client {conn.ClientId} loaded start scenes");
    }

    void OnClientConnectionStateChange(ClientConnectionStateArgs args)
    {
        if(args.ConnectionState == LocalConnectionState.Started)
        {

        }
        Debug.Log($"[LobbyManager] Updated connection state: {args.ConnectionState}");
    }
    void OnRemoteConnectionStateChange(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        Debug.Log($"[LobbyManager] Updated {conn.ClientId} connection state: {args.ConnectionState}");

        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            _connectedPlayers.Value.Add(conn.ClientId, new NetworkPlayerData()
            {
                ClientId = conn.ClientId,
            });
        }else if(args.ConnectionState == RemoteConnectionState.Stopped)
        {
            _connectedPlayers.Value.Remove(conn.ClientId);
        }
    }

    [Server]
    public void StartGame()
    {
        SceneLoadData sld = new SceneLoadData("GameScene")
        {
            ReplaceScenes = ReplaceOption.All,
        };
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        Debug.Log($"[LobbyManager] Started game.");
    }

    [Client]
    public void RegisterLocalLobbyPlayer(LobbyPlayer player)
    {
        _localLobbyPlayer = player;
        OnLocalPlayerRegister?.Invoke();
    }
    [Client]
    public void UnregisterLocalLobbyPlayer()
    {
        _localLobbyPlayer = null;
        OnLocalPlayerUnregister?.Invoke();
    }

    [Server]
    public void RegisterLobbyPlayer(LobbyPlayer player)
    {
        _lobbyPlayers.Add(player);
    }

    [Server]
    public void UnregisterLobbyPlayer(LobbyPlayer player)
    {
        _lobbyPlayers.Remove(player);
    }

    [Client]
    public void ChangeReadyState(bool ready)
    {
        _localLobbyPlayer.RPC_RequestSetReady(ready);
    }

    [Server]
    public void SERVER_HandlePlayerReadyStateChange()
    {
        bool allPlayersReady = _lobbyPlayers.All((x) => x.IsReady);

        OnPlayersReady?.Invoke(allPlayersReady);
    }

    [Server]
    public void SERVER_UpdateNetworkPlayerData(int clientId, NetworkPlayerData playerData)
    {
        _connectedPlayers.Value[clientId] = playerData;
    }
}