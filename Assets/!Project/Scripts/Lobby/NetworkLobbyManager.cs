using FishNet.Managing;
using FishNet.Transporting;
using FishySteamworks;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkLobbyManager : MonoBehaviour
{
    public static NetworkLobbyManager Instance;

    [SerializeField] private FishNet.Managing.NetworkManager _nm;
    [SerializeField] TransportSwitcher _transportSwitcher;

    private int _currentLobbyId;
    private bool _waitingForHost = false;
    private LobbyData _pendingLobby;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!SteamAPI.Init())
        {
            Debug.LogError("[NetworkLobbyManager] SteamAPI.Init() failed");
            return;
        }
        Debug.Log($"[NetworkLobbyManager] Steam ID: {SteamUser.GetSteamID()}");

        Debug.Log("[NetworkLobbyManager] SteamAPI initialized");
        Debug.Log($"[NetworkLobbyManager] Steam Name: {SteamFriends.GetPersonaName()}");
        Debug.Log($"[NetworkLobbyManager] Steam ID: {SteamUser.GetSteamID()}");

        _nm.ServerManager.OnServerConnectionState += OnServerConnectionState;
    }

    void OnDestroy()
    {
        if (_nm != null && _nm.ServerManager != null)
        {
            _nm.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        }

        if (_nm.IsServerStarted && _currentLobbyId != 0)
        {
            FirebaseManager.Instance.RemoveLobby(_currentLobbyId);
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
        _currentLobbyId = UnityEngine.Random.Range(0, 999999);

        _pendingLobby = new LobbyData
        {
            LobbyId = _currentLobbyId,
            MaxPlayers = ProjectConstants.LOBBY_MAX_PLAYERS,
            CurrentPlayers = 1,
            HostName = SaveManager.GameData.PlayerName,
        };

        _waitingForHost = true;

        _nm.ServerManager.StartConnection();
        _nm.ClientManager.StartConnection();

        LobbyManager.Instance.SERVER_InitLobby(_pendingLobby);
        Debug.Log($"[NetworkLobbyManager] Create lobby.");

    }

    void RegisterLobbyInFirebase()
    {
        ulong hostSteamId =  (ulong)_currentLobbyId;
#if UNITY_EDITOR

#else
        CSteamID steamId = SteamUser.GetSteamID();
        ulong hostSteamId = steamId.m_SteamID;
        hostSteamIdString = hostSteamId.ToString();
        _pendingLobby.HostSteamId = hostSteamId;
#endif

        FirebaseManager.Instance.CreateLobby(_pendingLobby, hostSteamId.ToString(), () =>
        {
            Debug.Log($"[NetworkLobbyManager] Lobby registered! ID: {_currentLobbyId}, SteamID: {hostSteamId}");
        });
    }


    public void JoinLobby(LobbyData lobby)
    {
        if (!_transportSwitcher.IsLocalMode)
        {
            _transportSwitcher.SteamTransport.SetClientAddress(lobby.HostSteamId.ToString());
        }

        _nm.ClientManager.StartConnection(); 
    }

    public void RefreshLobbies(System.Action<List<LobbyData>> callback)
    {
        FirebaseManager.Instance.LoadLobbies(callback);
    }
}