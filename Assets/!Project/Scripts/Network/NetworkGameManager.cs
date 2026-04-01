using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using Saves;
using Steamworks;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkGameManager : MonoBehaviour
{
    public static NetworkGameManager Instance;

    [SerializeField] public NetworkManager NetworkManager;
    [SerializeField] NetworkObject _networkManagersPrefab;
    NetworkObject _networkManagers;
    [SerializeField] private bool _isLocalMode = false;

    private bool _isConnecting = false;
    private int _currentLobbyId;
    private bool _waitingForHost = false;
    private LobbyData _pendingLobby;

    public event Action OnLocalClientConnected;
    public event Action<NetworkConnection> OnClientConnected;
    public event Action OnLocalClientDisconnected;
    public event Action OnLocalClientConnectionFailed;
    public event Action<NetworkConnection> OnClientDisconnected;
    public event Action<NetworkConnection, bool> OnClientLoadedStartScene;

    private void Awake() => Instance = this;

    void Start()
    {
        NetworkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChange;
        NetworkManager.ServerManager.OnServerConnectionState += OnServerConnectionStateChange;
        NetworkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChange;
        NetworkManager.SceneManager.OnClientLoadedStartScenes += HandleOnClientLoadedStartScene;
    }

    void OnDestroy() => Cleanup();
    private void OnApplicationQuit() => Cleanup();

    void OnServerConnectionStateChange(ServerConnectionStateArgs args)
    {
        Debug.Log($"[NetworkGameManager] Server state: {args.ConnectionState}");

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
                if (LobbyManager.Instance.IsLobbyFull())
                {
                    NetworkManager.ServerManager.Kick(conn.ClientId, KickReason.UnusualActivity);
                    return;
                }
                OnClientConnected?.Invoke(conn);
                break;
        }
    }

    void OnClientConnectionStateChange(ClientConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case LocalConnectionState.Stopped:
                if (_isConnecting)
                {
                    _isConnecting = false;
                    LoadingManager.Instance.SetLoadingProgress(1f);
                    PopupsManager.Instance.ShowPopup("Error while creating/joining lobby. Try again.");
                    OnLocalClientConnectionFailed?.Invoke();
                }
                OnLocalClientDisconnected?.Invoke();
                break;
            case LocalConnectionState.Started:
                _isConnecting = false;
                LoadingManager.Instance.SetLoadingProgress(1f);
                OnLocalClientConnected?.Invoke();
                break;
        }
    }

    void HandleOnClientLoadedStartScene(NetworkConnection conn, bool asServer)
    {
        if (asServer)
        {
            UnityEngine.SceneManagement.Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Menu");
            NetworkManager.SceneManager.AddConnectionToScene(conn, menuScene);
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

        LoadingManager.Instance.ShowLoading(LoadingWindowType.Popup);
        NetworkManager.ServerManager.StartConnection();

#if !UNITY_EDITOR
        CreateSteamLobby();
#endif
        await UniTask.WaitUntil(() => NetworkManager.ServerManager.Started);

        NetworkRoomManager.Instance.SERVER_Init();
        NetworkManager.ClientManager.StartConnection();
        LobbyManager.Instance.SERVER_InitLobby(_pendingLobby);
    }

    public async void JoinLobby(LobbyData lobby)
    {
        if(lobby.CurrentPlayers >= lobby.MaxPlayers)
        {
            PopupsManager.Instance.ShowPopup($"Lobby is full. {lobby.CurrentPlayers}/{lobby.MaxPlayers}");
            return;
        }
        Debug.Log($"[NetworkGameManager] Join attempt. Host SteamID: {lobby.HostSteamId}");

        if (!_isLocalMode)
            NetworkManager.TransportManager.Transport.SetClientAddress(lobby.HostSteamId.ToString());

        LoadingManager.Instance.ShowLoading(LoadingWindowType.Popup);
        await UniTask.WaitForSeconds(1.5f);
        _isConnecting = true;
        NetworkManager.ClientManager.StartConnection();
    }

    void RegisterLobbyInFirebase()
    {
        ulong hostSteamId = (ulong)_currentLobbyId;
#if !UNITY_EDITOR
        CSteamID steamId = SteamUser.GetSteamID();
        hostSteamId = steamId.m_SteamID;
        _pendingLobby.HostSteamId = hostSteamId;
#endif
        FirebaseManager.Instance.CreateLobby(_pendingLobby, hostSteamId.ToString(), () =>
        {
            Debug.Log($"[NetworkGameManager] Lobby registered! ID: {_pendingLobby.LobbyId}");
        });
    }

    public void Disconnect()
    {
        if (NetworkManager.IsServerStarted && NetworkManager.IsClientStarted)
            StopHost();
        else if (NetworkManager.ClientManager.Started)
            StopClient();
    }

    public void StopHost()
    {
        NetworkManager.ClientManager.StopConnection();
        NetworkManager.ServerManager.StopConnection(true);
    }

    public void StopClient()
    {
        NetworkManager.ClientManager.StopConnection();
    }

    public async Task<Texture2D> GetSteamAvatar(string steamIdString)
    {
        CSteamID steamId = new CSteamID(ulong.Parse(steamIdString));
        SteamFriends.RequestUserInformation(steamId, false);

        int avatarHandle = SteamFriends.GetLargeFriendAvatar(steamId);

        await UniTask.WaitUntil(() => {
            int handle = SteamFriends.GetLargeFriendAvatar(steamId);
            if (handle > 0) { avatarHandle = handle; return true; }
            return false;
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
                    imageData[srcIdx], imageData[srcIdx + 1],
                    imageData[srcIdx + 2], imageData[srcIdx + 3]
                );
            }

        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
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
            FirebaseManager.Instance.RemoveLobby(_currentLobbyId);
    }
}