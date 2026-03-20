using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Managing;
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

    private int _currentLobbyId;
    private bool _waitingForHost = false;
    private LobbyData _pendingLobby;
    private CSteamID _steamLobbyId;

    private CallResult<LobbyCreated_t> _lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;

    public event Action OnLocalClientConnected;
    public event Action<NetworkConnection> OnClientConnected;
    public event Action OnLocalClientDisconnected;
    public event Action<NetworkConnection> OnClientDisconnected;
    public event Action<NetworkConnection, bool> OnClientLoadedStartScene;

    private void Awake() => Instance = this;

    void Start()
    {
        NetworkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChange;
        NetworkManager.ServerManager.OnServerConnectionState += OnServerConnectionStateChange;
        NetworkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChange;
        NetworkManager.SceneManager.OnClientLoadedStartScenes += HandleOnClientLoadedStartScene;

        _lobbyCreated = CallResult<LobbyCreated_t>.Create(OnSteamLobbyCreated);
        _lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
    }

    void OnDestroy() => Cleanup();
    private void OnApplicationQuit() => Cleanup();

    void CreateSteamLobby()
    {
        var call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, ProjectConstants.LOBBY_MAX_PLAYERS);
        _lobbyCreated.Set(call);
    }

    void OnSteamLobbyCreated(LobbyCreated_t result, bool failure)
    {
        if (failure || result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"[NetworkGameManager] Steam lobby creation failed: {result.m_eResult}");
            return;
        }

        _steamLobbyId = new CSteamID(result.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(_steamLobbyId, "hostSteamId", SteamUser.GetSteamID().ToString());
        Debug.Log($"[NetworkGameManager] Steam lobby created: {_steamLobbyId}");
    }

    void OnLobbyJoinRequested(GameLobbyJoinRequested_t result)
    {
        CSteamID lobbyId = result.m_steamIDLobby;
        Debug.Log($"[NetworkGameManager] Join requested via Steam invite, lobby: {lobbyId}");

        SteamMatchmaking.RequestLobbyData(lobbyId);

        JoinViaLobbyId(lobbyId).Forget();
    }

    async UniTaskVoid JoinViaLobbyId(CSteamID lobbyId)
    {
        await UniTask.WaitForSeconds(1f);

        string hostSteamId = SteamMatchmaking.GetLobbyData(lobbyId, "hostSteamId");
        if (string.IsNullOrEmpty(hostSteamId))
        {
            Debug.LogError("[NetworkGameManager] Failed to get hostSteamId from lobby data");
            return;
        }

        Debug.Log($"[NetworkGameManager] Joining host: {hostSteamId}");
        NetworkManager.TransportManager.Transport.SetClientAddress(hostSteamId);
        NetworkManager.ClientManager.StartConnection();
    }

    public void OpenInviteDialog()
    {
        if (_steamLobbyId == CSteamID.Nil)
        {
            Debug.LogError("[NetworkGameManager] Steam lobby not created yet");
            return;
        }
        SteamFriends.ActivateGameOverlayInviteDialog(_steamLobbyId);
    }

    void LeaveSteamLobby()
    {
        if (_steamLobbyId != CSteamID.Nil)
        {
            SteamMatchmaking.LeaveLobby(_steamLobbyId);
            _steamLobbyId = CSteamID.Nil;
        }
    }

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
            case LocalConnectionState.Started:
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

        NetworkManager.ServerManager.StartConnection();

#if !UNITY_EDITOR
        CreateSteamLobby();
#endif

        await UniTask.WaitForSeconds(1.5f);
        NetworkRoomManager.Instance.SERVER_Init();
        NetworkManager.ClientManager.StartConnection();
        LobbyManager.Instance.SERVER_InitLobby(_pendingLobby);
    }

    public async void JoinLobby(LobbyData lobby)
    {
        Debug.Log($"[NetworkGameManager] Join attempt. Host SteamID: {lobby.HostSteamId}");

        if (!_isLocalMode)
            NetworkManager.TransportManager.Transport.SetClientAddress(lobby.HostSteamId.ToString());

        await UniTask.WaitForSeconds(1.5f);
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
        LeaveSteamLobby();
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

        await UniTask.WaitUntil(() => avatarHandle != -1 && avatarHandle != 0);

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

        LeaveSteamLobby();

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