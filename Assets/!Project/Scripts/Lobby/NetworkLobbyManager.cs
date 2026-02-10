using Cysharp.Threading.Tasks;
using FishNet.Managing;
using FishNet.Managing.Scened;
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
    [SerializeField] private bool _isLocalMode = false;

    private int _currentLobbyId;
    private bool _waitingForHost = false;
    private LobbyData _pendingLobby;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        _nm.ServerManager.OnServerConnectionState += OnServerConnectionState;
    }
    void Update()
    {
        SteamAPI.RunCallbacks();
    }

    void OnDestroy()
    {
        Cleanup();

    }

    private void OnApplicationQuit()
    {
        Cleanup();
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

    public async void CreateLobby()
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

        Debug.Log($"[NetworkLobbyManager] Create lobby attempt. Active transport: {_nm.TransportManager.Transport}");

        _nm.ServerManager.StartConnection();

        SceneLoadData sld = new SceneLoadData("Menu")
        {
            ReplaceScenes = ReplaceOption.None
        };
        _nm.SceneManager.LoadGlobalScenes(sld);
        await UniTask.WaitForSeconds(2f);

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
        hostSteamId = steamId.m_SteamID;
        _pendingLobby.HostSteamId = hostSteamId;
#endif

        FirebaseManager.Instance.CreateLobby(_pendingLobby, hostSteamId.ToString(), () =>
        {
            Debug.Log($"[NetworkLobbyManager] Lobby registered! ID: {_currentLobbyId}, SteamID: {hostSteamId}");
        });
    }


    public void JoinLobby(LobbyData lobby)
    {
        Debug.Log($"[NetworkLobbyManager] Join attempt. Host SteamID: {lobby.HostSteamId}");
        Debug.Log($"[NetworkLobbyManager] Active transport: {_nm.TransportManager.Transport}");

        if (!_isLocalMode)
        {
            _nm.TransportManager.Transport.SetClientAddress(lobby.HostSteamId.ToString());
        }

        Debug.Log($"[NetworkLobbyManager] Join lobby, host steam id: {lobby.HostSteamId}");
        _nm.ClientManager.StartConnection(); 
    }

    public void RefreshLobbies(System.Action<List<LobbyData>> callback)
    {
        FirebaseManager.Instance.LoadLobbies(callback);
    }

    void Cleanup()
    {
        if (_nm == null) return;

        if (_nm.ServerManager != null)
        {
            _nm.ServerManager.OnServerConnectionState -= OnServerConnectionState;

            if (_nm.ServerManager.Started)
                _nm.ServerManager.StopConnection(true);
        }

        if (_nm.ClientManager != null && _nm.ClientManager.Started)
            _nm.ClientManager.StopConnection();

        if (_nm.IsServerStarted && _currentLobbyId != 0)
        {
            FirebaseManager.Instance.RemoveLobby(_currentLobbyId);
        }
    }
}