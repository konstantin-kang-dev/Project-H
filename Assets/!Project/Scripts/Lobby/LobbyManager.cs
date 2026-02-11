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
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Networking.PlayerConnection;
using UnityEditor.PackageManager;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;
    public static event Action OnReady;

    [SerializeField] List<Transform> _lobbySlotsPoints = new List<Transform>();
    readonly SyncList<LobbySlot> _lobbySlots = new SyncList<LobbySlot>();
    [SerializeField] LobbyPlayer _lobbyPlayerPrefab;

    readonly SyncVar<LobbyData> _lobbyData = new SyncVar<LobbyData>();

    List<LobbyPlayer> _lobbyPlayers = new List<LobbyPlayer>();
    LobbyPlayer _localLobbyPlayer;
    public bool IsLocalPlayerSet => _localLobbyPlayer != null;
    public bool LocalPlayerReadyState => IsLocalPlayerSet ? _localLobbyPlayer.IsReady : false;

    public event Action OnLocalPlayerRegister;
    public event Action OnLocalPlayerUnregister;
    public event Action<bool> OnPlayersReady;

    public event Action<LobbyData> OnLobbyDataUpdated;
    public event Action OnGameStarted;
    private void Awake()
    {
        Instance = this;

    }

    private void OnDestroy()
    {

    }

    [Server]
    public void SERVER_InitLobby(LobbyData lobbyData)
    {
        Debug.Log($"[LobbyManager] Initialized lobby: {lobbyData} | {_lobbyData.Value}");
        _lobbyData.Value = lobbyData;

    }

    [Server]
    public void SERVER_CloseLobby()
    {
        FirebaseManager.Instance.RemoveLobby(_lobbyData.Value.LobbyId);
    }


    public override void OnStartServer()
    {
        NetworkGameManager.Instance.OnClientDisconnected += SERVER_HandleClientDisconnected;

        base.OnStartServer();

        _lobbySlots.Clear();
        _lobbySlots.AddRange(new List<LobbySlot>()
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
        });
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        NetworkGameManager.Instance.OnClientDisconnected -= SERVER_HandleClientDisconnected;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        NetworkGameManager.Instance.OnLocalClientConnected += HandleLocalClientConnected;
        NetworkGameManager.Instance.OnLocalClientDisconnected += HandleLocalClientDisconnected;

        RPC_RequestSendPlayerData(SaveManager.GameData, ClientManager.Connection);
        OnReady?.Invoke();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        NetworkGameManager.Instance.OnLocalClientConnected -= HandleLocalClientConnected;
        NetworkGameManager.Instance.OnLocalClientDisconnected -= HandleLocalClientDisconnected;
    }

    void HandleLocalClientConnected()
    {
        _lobbyData.OnChange += HandleLobbyDataChanged;
    }

    void HandleLocalClientDisconnected()
    {
        _lobbyData.OnChange -= HandleLobbyDataChanged;
    }
    [Server]
    void SERVER_HandleClientDisconnected(NetworkConnection conn)
    {
        LobbySlot lobbySlot = _lobbySlots.FirstOrDefault((x) => x.ClientId == conn.ClientId);
        if (lobbySlot != null)
        {
            lobbySlot.ResetData();
        }
    }

    [Server]
    public void SERVER_UpdateLobbyData()
    {
        _lobbyData.Value = new LobbyData()
        {
            LobbyId = _lobbyData.Value.LobbyId,
            MaxPlayers = _lobbyData.Value.MaxPlayers,
            CurrentPlayers = ServerRoomManager.Instance.ConnectedPlayersCount,
            ChosenDifficulty = _lobbyData.Value.ChosenDifficulty,
            HostName = _lobbyData.Value.HostName,
            HostSteamId = _lobbyData.Value.HostSteamId,
        };

        FirebaseManager.Instance.UpdateLobbyData(_lobbyData.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RPC_RequestStartGame()
    {
        Debug.Log($"[LobbyManager] Started game.");
        CLIENTS_HandleStartGame();
        ServerRoomManager.Instance.SERVER_LoadGameScene(1);
    }

    [ObserversRpc]
    public void CLIENTS_HandleStartGame()
    {
        OnGameStarted?.Invoke();
    }

    [Server]
    public void SERVER_UpdateDifficulty(DifficultyType type)
    {
        _lobbyData.Value = new LobbyData()
        {
            LobbyId = _lobbyData.Value.LobbyId,
            MaxPlayers = _lobbyData.Value.MaxPlayers,
            CurrentPlayers = _lobbyData.Value.CurrentPlayers,
            ChosenDifficulty = type,
            HostName = _lobbyData.Value.HostName,
            HostSteamId = _lobbyData.Value.HostSteamId,
        };

        FirebaseManager.Instance.UpdateLobbyData(_lobbyData.Value);
    }

    [Client]
    void HandleLobbyDataChanged(LobbyData prev, LobbyData next, bool asServer)
    {
        if (asServer) return;

        OnLobbyDataUpdated?.Invoke(next);
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
        OnLocalPlayerUnregister?.Invoke();
        _localLobbyPlayer = null;
    }

    [Server]
    public void SERVER_RegisterLobbyPlayer(LobbyPlayer player)
    {
        _lobbyPlayers.Add(player);
    }

    [Server]
    public void SERVER_UnregisterLobbyPlayer(LobbyPlayer player)
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
    public LobbySlot SERVER_GetPlayersLobbySlot(int clientId)
    {
        return _lobbySlots.FirstOrDefault((x)=> x.ClientId == clientId);
    }

    [Server]
    public LobbySlot SERVER_GetFreeLobbySlot()
    {
        return _lobbySlots.FirstOrDefault((x) => !x.IsOccupied);
    }

    [Server]
    public void SERVER_OccupyLobbySlot(NetworkPlayerData networkPlayerData, LobbySlot lobbySlot = null)
    {
        if(lobbySlot == null)
        {
            lobbySlot = SERVER_GetFreeLobbySlot();
            if (lobbySlot == null) throw new Exception($"[LobbyManager] No available lobby slots.");
        }

        NetworkConnection conn = ServerManager.Clients[networkPlayerData.ClientId];
        if (conn == null) throw new Exception($"[LobbyManager] Player({networkPlayerData.PlayerName}, {networkPlayerData.ClientId}) doesn't exist in network.");
        
        lobbySlot.IsOccupied = true;
        lobbySlot.ClientId = networkPlayerData.ClientId;

        Transform slotPoint = _lobbySlotsPoints[lobbySlot.SlotKey];
        LobbyPlayer lobbyPlayer = Instantiate(_lobbyPlayerPrefab, slotPoint.position, Quaternion.identity);
        Spawn(lobbyPlayer, conn);

        Debug.Log($"[LobbyManager] Added player({conn.ClientId}) to slot: {lobbySlot.SlotKey}.");
    }


    [ServerRpc(RequireOwnership = false)]
    public void RPC_RequestSendPlayerData(GameData data, NetworkConnection conn)
    {
        ServerRoomManager.Instance.SERVER_SetPlayerData(data, conn);
    }
}