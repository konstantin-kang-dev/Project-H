
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameAudio;
using Saves;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;
    public static event Action OnReady;
    public static event Action OnClear;

    [SerializeField] List<Transform> _lobbySlotsPoints = new List<Transform>();
    readonly SyncDictionary<int, LobbySlot> _lobbySlots = new SyncDictionary<int, LobbySlot>();
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
        NetworkGameManager.Instance.OnClientConnected += SERVER_HandleClientConnected;
        NetworkGameManager.Instance.OnClientDisconnected += SERVER_HandleClientDisconnected;

        base.OnStartServer();

        _lobbySlots.Clear();

        for (int i = 0; i < 4; i++)
        {
            _lobbySlots.Add(i, new LobbySlot()
            {
                SlotKey = i,
                ClientId = -1,
                IsOccupied = false,
            });
        }

    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        NetworkGameManager.Instance.OnClientConnected -= SERVER_HandleClientConnected;
        NetworkGameManager.Instance.OnClientDisconnected -= SERVER_HandleClientDisconnected;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        foreach (var lobbySlotPoint in _lobbySlotsPoints)
        {
            lobbySlotPoint.gameObject.SetActive(true);
        }

        _lobbySlots.OnChange += HandleLobbySlotsChange;

        NetworkGameManager.Instance.OnLocalClientConnected += HandleLocalClientConnected;
        NetworkGameManager.Instance.OnLocalClientDisconnected += HandleLocalClientDisconnected;

        RPC_RequestSendPlayerData(SaveManager.GameSave.PlayerSave, ClientManager.Connection);

        OnReady?.Invoke();
    }

    private void HandleLobbySlotsChange(SyncDictionaryOperation op, int key, LobbySlot value, bool asServer)
    {
        switch (op)
        {
            case SyncDictionaryOperation.Set:
                SetEmptySlotUI(value.SlotKey, value.ClientId == -1);

                break;
            case SyncDictionaryOperation.Complete:
                SetEmptySlotUI(value.SlotKey, value.ClientId == -1);

                break;
        }

    }

    void SetEmptySlotUI(int slotKey, bool value)
    {
        _lobbySlotsPoints[slotKey].gameObject.SetActive(value);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        _lobbySlots.OnChange -= HandleLobbySlotsChange;
        NetworkGameManager.Instance.OnLocalClientConnected -= HandleLocalClientConnected;
        NetworkGameManager.Instance.OnLocalClientDisconnected -= HandleLocalClientDisconnected;

        OnClear?.Invoke();
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
    void SERVER_HandleClientConnected(NetworkConnection conn)
    {
        RPC_NotifyPlayersConnection();
    }

    [Server]
    void SERVER_HandleClientDisconnected(NetworkConnection conn)
    {
        LobbySlot lobbySlot = _lobbySlots.FirstOrDefault((x) => x.Value.ClientId == conn.ClientId).Value;
        if (lobbySlot.IsOccupied)
        {
            LobbySlot lobbySlotTemp = lobbySlot;
            lobbySlotTemp.ClientId = -1;
            lobbySlotTemp.IsOccupied = false;

            _lobbySlots[lobbySlot.SlotKey] = lobbySlotTemp;
        }

        RPC_NotifyPlayersDisconnection();
    }

    [ObserversRpc]
    void RPC_NotifyPlayersConnection()
    {
        GlobalAudioManager.Instance.Play(SoundType.UILobbyJoin);
    }

    [ObserversRpc]
    void RPC_NotifyPlayersDisconnection()
    {
        GlobalAudioManager.Instance.Play(SoundType.UILobbyLeave);
    }

    [Server]
    public void SERVER_UpdateLobbyData()
    {
        _lobbyData.Value = new LobbyData()
        {
            LobbyId = _lobbyData.Value.LobbyId,
            MaxPlayers = _lobbyData.Value.MaxPlayers,
            CurrentPlayers = NetworkRoomManager.Instance.ConnectedPlayersCount,
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
        NetworkRoomManager.Instance.SERVER_LoadGameScene(1);
    }

    [ObserversRpc]
    public void CLIENTS_HandleStartGame()
    {
        GlobalAudioManager.Instance.Stop(SoundType.MenuAmbient);

        foreach (var lobbySlotPoint in _lobbySlotsPoints)
        {
            lobbySlotPoint.gameObject.SetActive(false);
        }

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
        return _lobbySlots.FirstOrDefault((x)=> x.Value.ClientId == clientId).Value;
    }

    [Server]
    public LobbySlot SERVER_GetFreeLobbySlot()
    {
        return _lobbySlots.FirstOrDefault((x) => !x.Value.IsOccupied).Value;
    }

    [Server]
    public void SERVER_OccupyLobbySlot(NetworkPlayerData networkPlayerData, LobbySlot lobbySlot)
    {
        NetworkConnection conn = ServerManager.Clients[networkPlayerData.ClientId];
        if (conn == null) throw new Exception($"[LobbyManager] Player({networkPlayerData.PlayerName}, {networkPlayerData.ClientId}) doesn't exist in network.");
        
        LobbySlot lobbySlotTemp = lobbySlot;

        lobbySlotTemp.IsOccupied = true;
        lobbySlotTemp.ClientId = networkPlayerData.ClientId;
        _lobbySlots[lobbySlot.SlotKey] = lobbySlotTemp;

        Transform slotPoint = _lobbySlotsPoints[lobbySlot.SlotKey];
        LobbyPlayer lobbyPlayer = Instantiate(_lobbyPlayerPrefab, slotPoint.position, Quaternion.identity);
        Spawn(lobbyPlayer, conn);

        Debug.Log($"[LobbyManager] Added player({conn.ClientId}) to slot: {lobbySlot.SlotKey}.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RPC_RequestSendPlayerData(PlayerSave data, NetworkConnection conn)
    {
        NetworkRoomManager.Instance.SERVER_SetPlayerData(data, conn);
    }

}