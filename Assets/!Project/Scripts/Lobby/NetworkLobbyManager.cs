using FishNet.Managing;
using FishNet.Transporting;
using FishySteamworks;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

public class NetworkLobbyManager : MonoBehaviour
{
    public static NetworkLobbyManager Instance;

    [SerializeField] private FishNet.Managing.NetworkManager _nm;
    [SerializeField] private FirebaseManager _firebaseLobby;

    private FishySteamworks.FishySteamworks _transport;
    private int _currentLobbyId;
    private bool _waitingForHost = false;
    private LobbyData _pendingLobby;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _transport = _nm.TransportManager.GetTransport<FishySteamworks.FishySteamworks>();
        _nm.ServerManager.OnServerConnectionState += OnServerConnectionState;

        if (!SteamAPI.Init())
        {
            Debug.LogError("[NetworkLobbyManager] SteamAPI.Init() failed");
            return;
        }

        Debug.Log("[NetworkLobbyManager] SteamAPI initialized");
        Debug.Log($"[NetworkLobbyManager] Steam Name: {SteamFriends.GetPersonaName()}");
        Debug.Log($"[NetworkLobbyManager] Steam ID: {SteamUser.GetSteamID()}");
    }

    void OnDestroy()
    {
        if (_nm != null && _nm.ServerManager != null)
        {
            _nm.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        }

        if (_nm.IsServerStarted && _currentLobbyId != 0)
        {
            _firebaseLobby.RemoveLobby(_currentLobbyId);
        }
    }

    void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        Debug.Log($"[NetworkLobbyManager] Server state changed: {args.ConnectionState}");

        if (args.ConnectionState == LocalConnectionState.Started && _waitingForHost)
        {
            _waitingForHost = false;
            RegisterLobbyInFirebase();
        }
    }

    public void CreateLobby()
    {
        _currentLobbyId = UnityEngine.Random.Range(1000, 9999);

        _pendingLobby = new LobbyData
        {
            LobbyId = _currentLobbyId,
            MaxPlayers = ProjectConstants.LOBBY_MAX_PLAYERS,
            CurrentPlayers = 1,
            HostName = SaveManager.GameData.PlayerName,
        };

        _waitingForHost = true;

        //_nm.ServerManager.StartConnection();
        //_nm.ClientManager.StartConnection();

        Debug.Log($"[NetworkLobbyManager] Create lobby");
    }

    void RegisterLobbyInFirebase()
    {
        CSteamID steamId = SteamUser.GetSteamID();
        ulong hostSteamId = steamId.m_SteamID;

        _pendingLobby.HostSteamId = hostSteamId;

        _firebaseLobby.CreateLobby(_pendingLobby, hostSteamId.ToString(), () =>
        {
            Debug.Log($"[NetworkLobbyManager] Lobby registered! ID: {_currentLobbyId}, SteamID: {hostSteamId}");
        });
    }


    public void JoinLobby(LobbyData lobby)
    {
        _transport.SetClientAddress(lobby.HostSteamId.ToString());
        _nm.ClientManager.StartConnection();
        _firebaseLobby.UpdatePlayerCount(lobby.LobbyId, lobby.CurrentPlayers + 1);
    }

    public void RefreshLobbies(System.Action<List<LobbyData>> callback)
    {
        _firebaseLobby.GetLobbies(callback);
    }
}