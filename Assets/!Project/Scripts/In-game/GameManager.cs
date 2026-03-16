using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {  get; private set; }

    readonly SyncVar<GameState> _gameState = new SyncVar<GameState>();
    public GameState GameState => _gameState.Value;

    readonly SyncDictionary<int, bool> _playersReadyToStart = new SyncDictionary<int, bool>();
    public event Action OnAllPlayersReadyToStart;

    readonly SyncVar<DifficultyType> _gameDifficulty = new SyncVar<DifficultyType>();
    public DifficultyType GameDifficulty => _gameDifficulty.Value;
    public GameDifficultyConfig GameDifficultyConfig { get; private set; }

    [field: SerializeField] public Player LocalPlayer { get; private set; }

    Dictionary<int, Player> _serverPlayers = new Dictionary<int, Player>();
    public Dictionary<int, Player> SERVER_Players => _serverPlayers;

    public float SessionTimer { get; private set; } = 0f;
    public bool IsWin { get; private set; } = false;
    public bool IsInitialized { get; private set; } = false;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Init();

        _gameState.OnChange += HandleGameStateChange;
        _gameDifficulty.OnChange += HandleDifficultyChange;

        LoadingManager.Instance.SetLoadingProgress(0.75f);

        RPC_RequestSetReadyToStart(ClientManager.Connection.ClientId);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _gameState.Value = GameState.PreparingToStart;
        _playersReadyToStart.Clear();
        OnAllPlayersReadyToStart += SERVER_StartGame;

        _gameDifficulty.Value = GameDifficultyManager.Instance.SelectedConfig.DifficultyType;

        NetworkGameManager.Instance.OnClientDisconnected += HandlePlayerDisconnect;
        Debug.Log($"[GameManager] Server is loaded. Difficulty: {_gameDifficulty.Value}");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        _gameState.OnChange -= HandleGameStateChange;

        _gameDifficulty.OnChange -= HandleDifficultyChange;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        OnAllPlayersReadyToStart -= SERVER_StartGame;
    }

    public void Init()
    {
        if (IsServerStarted)
        {
            //SetupPlayers();
        }
        else
        {
            
        }

        Debug.Log($"[GameManager] Initialized");
    }

    private void FixedUpdate()
    {
        if (GameState == GameState.Started)
        {
            SessionTimer += Time.deltaTime;
        }
    }

    [Server]
    public async void SERVER_StartGame()
    {
        _serverPlayers = PlayersSpawnManager.Instance.SpawnPlayers(NetworkRoomManager.Instance.ConnectedPlayers.Values.ToList());

        foreach (var playerBlock in _serverPlayers)
        {
            Player player = playerBlock.Value;
            player.SERVER_SetReadyToInit(true);
        }

        await UniTask.WaitForSeconds(3);

        _gameState.Value = GameState.Started;
    }

    [Client]
    public void RegisterLocalPlayer(Player player)
    {
        LocalPlayer = player;
    }
    [Server]
    public Player GetRandomPlayer()
    {
        int randomIndex = UnityEngine.Random.Range(0, _serverPlayers.Count);

        return _serverPlayers.Values.ToList()[randomIndex];
    }

    [Server] 
    void HandlePlayerDisconnect(NetworkConnection conn)
    {
        var nullKeys = _serverPlayers.Where(x => x.Value == null).Select(x => x.Key).ToList();
        foreach (var key in nullKeys)
            _serverPlayers.Remove(key);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RPC_RequestSetReadyToStart(int clientId)
    {
        _playersReadyToStart.Add(clientId, true);

        if(_playersReadyToStart.Count == NetworkRoomManager.Instance.ConnectedPlayersCount)
        {
            OnAllPlayersReadyToStart?.Invoke();
        }

        Debug.Log($"[GameManager] Player {clientId} is ready to start game. Total {_playersReadyToStart.Count}/{NetworkRoomManager.Instance.ConnectedPlayersCount}");
    }

    [Client]
    void HandleGameStateChange(GameState prev, GameState next, bool asServer)
    {
        if (asServer) return;

        switch (next)
        {
            case GameState.PreparingToStart:
                break;
            case GameState.Started:
                InitLocation();
                break;
            case GameState.Ended:
                break;
        }
    }
    [Client]
    void HandleDifficultyChange(DifficultyType prev, DifficultyType next, bool asServer)
    {
        if(asServer) return;

        GameDifficultyConfig = GameDifficultyManager.Instance.GetConfigByType(next);
    }

    void InitLocation()
    {
        GameUI.Instance.Init();

        if (IsServerStarted)
        {
            ObjectivesManager.Instance.Init(GameDifficulty, _serverPlayers.Count);
            EnemiesManager.Instance.Init(GameDifficultyConfig);
        }

        LoadingManager.Instance.SetLoadingProgress(1f);
        GlobalAudioManager.Instance.Play(SoundType.GameAmbient);

        ResultsUI.Instance.OnContinueBtn += QuitToMenu;
    }

    [Server]
    public void HandlePlayerKnockedDown(Player player)
    {
        bool isAllKnockedDown = true;

        foreach (var playerBlock in SERVER_Players)
        {
            Player checkingPlayer = playerBlock.Value;
            if (!checkingPlayer.IsKnockedDown)
            {
                isAllKnockedDown = false;
                break;
            }
        }

        if (isAllKnockedDown) SERVER_EndGame(false);
    }

    [Server]
    public async void SERVER_EndGame(bool isWin)
    {
        _gameState.Value = GameState.Ended;
        IsWin = isWin;

        await UniTask.WaitForSeconds(2f);

        RPC_SendSessionDataToClients(SessionTimer, isWin);
    }

    [ObserversRpc]
    void RPC_SendSessionDataToClients(float sessionTime, bool isWin)
    {
        SessionTimer = sessionTime;
        IsWin = isWin;

        List<NetworkPlayerData> playersData = NetworkRoomManager.Instance.ConnectedPlayers.Values.ToList(); 

        GlobalAudioManager.Instance.Play(isWin ? SoundType.UIGameVictory : SoundType.UIGameLose);
        ResultsUI.Instance.SetVisibility(true, IsWin, playersData, _gameDifficulty.Value, SessionTimer);
        LocalPlayer.HandleEndGame();
    }
    public async void QuitToMenu()
    {
        NetworkGameManager.Instance.Disconnect();
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Screen, "Loading Menu...");

        await UniTask.WaitForSeconds(0.5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager manager = (GameManager)target;

        EditorGUILayout.Space(10);

    }
}
#endif