using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using FishySteamworks;
using Saves;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.GridLayoutGroup;

public class NetworkGameManager : MonoBehaviour
{
    public static NetworkGameManager Instance;

    [SerializeField] public NetworkManager NetworkManager;
    [SerializeField] NetworkObject _networkManagersPrefab;
    NetworkObject _networkManagers;

    [SerializeField] private bool _isLocalMode = false;

    private int _currentLobbyId;
    private bool _waitingForHost = false;
    private LobbyData _pendingLobby;

    public event Action OnLocalClientConnected;
    public event Action<NetworkConnection> OnClientConnected;
    public event Action OnLocalClientDisconnected;
    public event Action<NetworkConnection> OnClientDisconnected;

    public event Action<NetworkConnection, bool> OnClientLoadedStartScene;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        NetworkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChange;
        NetworkManager.ServerManager.OnServerConnectionState += OnServerConnectionStateChange;
        NetworkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChange;
        NetworkManager.SceneManager.OnClientLoadedStartScenes += HandleOnClientLoadedStartScene;
    }
    void Update()
    {
        if (SteamAPI.IsSteamRunning())
        {
            SteamAPI.RunCallbacks();
        }
    }

    void OnDestroy()
    {
        Cleanup();

    }

    private void OnApplicationQuit()
    {
        Cleanup();
    }

    void OnServerConnectionStateChange(ServerConnectionStateArgs args)
    {
        Debug.Log($"[NetworkLobbyManager] Server state changed: {args.ConnectionState}");

        if (args.ConnectionState == LocalConnectionState.Started && _waitingForHost)
        {
            _waitingForHost = false;
            RegisterLobbyInFirebase(); 

            if (_networkManagers == null)
            {
                _networkManagers = Instantiate(_networkManagersPrefab);
                NetworkManager.ServerManager.Spawn(_networkManagers);
            }
        }
    }

    void OnRemoteConnectionStateChange(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case RemoteConnectionState.Stopped:
                OnClientDisconnected?.Invoke(conn);
                break;
            case RemoteConnectionState.Started:
                OnClientConnected?.Invoke(conn);
                break;
        }
    }
    void OnClientConnectionStateChange(ClientConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case LocalConnectionState.Stopped:
                OnLocalClientDisconnected?.Invoke();
                break;
            case LocalConnectionState.Stopping:
                break;
            case LocalConnectionState.Starting:
                break;
            case LocalConnectionState.Started:
                OnLocalClientConnected?.Invoke();
                break;
        }
    }
    void HandleOnClientLoadedStartScene(NetworkConnection conn, bool asServer)
    {
        if (asServer)
        {
            Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Menu");
            NetworkManager.SceneManager.AddConnectionToScene(conn, menuScene);
        }

        if (conn.IsLocalClient)
        {
        }

        OnClientLoadedStartScene?.Invoke(conn, asServer);
    }

    public async void CreateLobby()
    {
        _currentLobbyId = UnityEngine.Random.Range(0, 999999);

        _pendingLobby = new LobbyData
        {
            LobbyId = _currentLobbyId,
            MaxPlayers = ProjectConstants.LOBBY_MAX_PLAYERS,
            CurrentPlayers = 1,
            HostName = SaveManager.GameSave.PlayerSave.PlayerName,
        };

        _waitingForHost = true;

        Debug.Log($"[NetworkLobbyManager] Create lobby attempt. Active transport: {NetworkManager.TransportManager.Transport}");

        NetworkManager.ServerManager.StartConnection();

        await UniTask.WaitForSeconds(1.5f);
        NetworkRoomManager.Instance.Init();

        NetworkManager.ClientManager.StartConnection();

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
            Debug.Log($"[NetworkLobbyManager] Lobby registered! ID: {_pendingLobby.LobbyId}, HostName: {_pendingLobby.HostName}");
        });

    }


    public async void JoinLobby(LobbyData lobby)
    {
        Debug.Log($"[NetworkLobbyManager] Join attempt. Host SteamID: {lobby.HostSteamId}");
        Debug.Log($"[NetworkLobbyManager] Active transport: {NetworkManager.TransportManager.Transport}");

        if (!_isLocalMode)
        {
            NetworkManager.TransportManager.Transport.SetClientAddress(lobby.HostSteamId.ToString());
        }

        await UniTask.WaitForSeconds(1.5f);
        Debug.Log($"[NetworkLobbyManager] Join lobby, host steam id: {lobby.HostSteamId}");
        NetworkManager.ClientManager.StartConnection(); 
    }

    public async Task<Texture2D> GetSteamAvatar(string steamIdString)
    {
        CSteamID steamId = new CSteamID(ulong.Parse(steamIdString));
        SteamFriends.RequestUserInformation(steamId, false);

        int avatarHandle = SteamFriends.GetLargeFriendAvatar(steamId);

        await UniTask.WaitUntil(() =>
        {
            return avatarHandle != -1 && avatarHandle != 0;
        });

        SteamUtils.GetImageSize(avatarHandle, out uint width, out uint height);

        byte[] imageData = new byte[4 * width * height];
        SteamUtils.GetImageRGBA(avatarHandle, imageData, imageData.Length);

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Trilinear;

        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int srcIdx = (int)((height - 1 - y) * width + x) * 4;
                pixels[y * width + x] = new Color32(
                    imageData[srcIdx],
                    imageData[srcIdx + 1],
                    imageData[srcIdx + 2],
                    imageData[srcIdx + 3]
                );
            }

        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    public void Disconnect()
    {
        bool isServerActive = NetworkManager.ServerManager.Started;

        bool isClientActive = NetworkManager.ClientManager.Started;

        bool isHost = NetworkManager.IsServerStarted && NetworkManager.IsClientStarted;

        if (isHost)
        {
            StopHost();
        }
        else if (isClientActive)
        {
            StopClient();
        }
    }
    public void StopHost()
    {
        NetworkManager.ClientManager.StopConnection();

        NetworkManager.ServerManager.StopConnection(true);
        Debug.Log("[NetworkLobbyManager] Host stoped");
    }

    public void StopClient()
    {
        NetworkManager.ClientManager.StopConnection();
        Debug.Log($"[NetworkLobbyManager] Client stoped");
    }
    void Cleanup()
    {
        if (NetworkManager == null) return;

        if (NetworkManager.ServerManager != null)
        {
            NetworkManager.ServerManager.OnServerConnectionState -= OnServerConnectionStateChange;

            if (NetworkManager.ServerManager.Started)
                NetworkManager.ServerManager.StopConnection(true);
        }

        if (NetworkManager.ClientManager != null && NetworkManager.ClientManager.Started)
            NetworkManager.ClientManager.StopConnection();

        if (NetworkManager.IsServerStarted && _currentLobbyId != 0)
        {
            FirebaseManager.Instance.RemoveLobby(_currentLobbyId);
        }

    }
}