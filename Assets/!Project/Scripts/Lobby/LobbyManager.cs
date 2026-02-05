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
using Cysharp.Threading.Tasks;

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

    readonly SyncDictionary<int, NetworkPlayerData> _connectedPlayers = new SyncDictionary<int, NetworkPlayerData>();
    public IReadOnlyDictionary<int, NetworkPlayerData> ConnectedPlayers => _connectedPlayers;

    readonly SyncVar<bool> _isGameStarted = new SyncVar<bool>();

    public event Action OnLocalPlayerRegister;
    public event Action OnLocalPlayerUnregister;
    public event Action<bool> OnPlayersReady;

    public event Action OnJoinedLobby;
    public event Action OnGameStarted;
    public event Action OnClientConnected;
    public event Action OnClientConnectionLost;
    private void Awake()
    {
        Instance = this;

        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChange;
    }

    private void OnDestroy()
    {

        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionStateChange;
    }

    public void StopConnection()
    {
        bool isServerActive = _networkManager.ServerManager.Started;

        bool isClientActive = _networkManager.ClientManager.Started;

        bool isHost = _networkManager.IsServerStarted && _networkManager.IsClientStarted;

        if (isHost)
        {
            StopHost();
        }
        else if(isClientActive)
        {
            StopClient();
        }
    }
    public void StopHost()
    {
        _networkManager.ClientManager.StopConnection();

        _networkManager.ServerManager.StopConnection(true);
        Debug.Log("Host stoped");
    }

    public void StopClient()
    {
        _networkManager.ClientManager.StopConnection();
        Debug.Log($"Client stoped");
    }

    public override void OnStartServer()
    {
        ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChange;
        SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;

        base.OnStartServer();

        _isGameStarted.Value = false;
        _connectedPlayers.Clear();

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
        ServerManager.OnRemoteConnectionState -= OnRemoteConnectionStateChange;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    private void OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (asServer)
        {
            Debug.Log($"[LobbyManager] Player[{conn.ClientId}] loaded start scenes.");

            SceneManager.AddConnectionToScene(conn, gameObject.scene);

            LobbySlot availableSlot = _lobbySlots.Value.First((x) => !x.IsOccupied);
            availableSlot.IsOccupied = true;
            availableSlot.ClientId = conn.ClientId;

            Transform slotPoint = _lobbySlotsPoints[availableSlot.SlotKey];

            LobbyPlayer lobbyPlayer = Instantiate(_lobbyPlayerPrefab, slotPoint.position, Quaternion.identity);
            Spawn(lobbyPlayer, conn);

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
            _isGameStarted.OnChange += HandleGameStarted;
            OnClientConnected?.Invoke();
        }
        else if(args.ConnectionState == LocalConnectionState.Stopped)
        {
            OnClientConnectionLost?.Invoke();
        }
        Debug.Log($"[LobbyManager] Updated client connection state: {args.ConnectionState}");
    }
    void OnRemoteConnectionStateChange(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        Debug.Log($"[LobbyManager] Updated {conn.ClientId} remote connection state: {args.ConnectionState}");

        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            _connectedPlayers.Add(conn.ClientId, new NetworkPlayerData()
            {
                ClientId = conn.ClientId,
            });
        }
        else if(args.ConnectionState == RemoteConnectionState.Stopped)
        {
            _connectedPlayers.Remove(conn.ClientId);
            LobbySlot lobbySlot = _lobbySlots.Value.FirstOrDefault((x)=> x.ClientId == conn.ClientId);
            if(lobbySlot != null)
            {
                lobbySlot.ResetData();
            }
        }
    }

    [Server]
    public async void StartGame()
    {
        _isGameStarted.Value = true;

        await UniTask.WaitForSeconds(0.5f);

        SceneLoadData sld = new SceneLoadData("GameScene")
        {
            ReplaceScenes = ReplaceOption.All,
        };
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        Debug.Log($"[LobbyManager] Started game.");
    }
    [Client]
    void HandleGameStarted(bool prev, bool next, bool asServer)
    {
        if (asServer) return;

        if (next)
        {
            OnGameStarted?.Invoke();
        }
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
        _connectedPlayers[clientId] = playerData;
    }
}