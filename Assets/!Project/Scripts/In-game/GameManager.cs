using Cysharp.Threading.Tasks;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {  get; private set; }

    readonly SyncVar<GameState> _gameState = new SyncVar<GameState>();
    public GameState GameState => _gameState.Value;

    readonly SyncDictionary<int, bool> _playersReadyToStart = new SyncDictionary<int, bool>();
    public event Action OnAllPlayersReadyToStart;

    readonly SyncVar<DifficultyType> _gameDifficulty = new SyncVar<DifficultyType>();
    public DifficultyType GameDifficulty => _gameDifficulty.Value;

    [field: SerializeField] public Player LocalPlayer { get; private set; }

    Dictionary<int, Player> _players = new Dictionary<int, Player>();
    public Dictionary<int, Player> Players => _players;

    readonly SyncVar<float> _gameSpeed = new SyncVar<float>();

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

        _gameSpeed.OnChange += HandleGameSpeedChange;
        _gameState.OnChange += HandleGameStateChange;

        LoadingManager.Instance.SetLoadingProgress(0.75f);

        RPC_RequestSetReadyToStart(ClientManager.Connection.ClientId);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _gameSpeed.Value = 1;
        _gameState.Value = GameState.PreparingToStart;
        _playersReadyToStart.Clear();
        OnAllPlayersReadyToStart += SERVER_StartGame;

        _gameDifficulty.Value = GameDifficultyManager.Instance.SelectedConfig.DifficultyType;
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

    [Server]
    public async void SERVER_StartGame()
    {
        _players = PlayersSpawnManager.Instance.SpawnPlayers(LobbyManager.Instance.ConnectedPlayers.Values.ToList());

        foreach (var playerBlock in _players)
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

    [ServerRpc(RequireOwnership = false)]
    public void RPC_RequestSetReadyToStart(int clientId)
    {
        _playersReadyToStart.Add(clientId, true);

        if(_playersReadyToStart.Count == LobbyManager.Instance.ConnectedPlayers.Count)
        {
            OnAllPlayersReadyToStart?.Invoke();
        }

        Debug.Log($"[GameManager] Player {clientId} is ready to start game. Total {_playersReadyToStart.Count}/{LobbyManager.Instance.ConnectedPlayers.Count}");
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

    void InitLocation()
    {
        GameCanvas.Instance.Init();
        if (IsServerStarted)
        {
            ObjectivesManager.Instance.Init(GameDifficulty, LobbyManager.Instance.ConnectedPlayers.Count);
            EnemiesManager.Instance.Init();
        }

        LoadingManager.Instance.SetLoadingProgress(1f);
    }

    [Server]
    public void SERVER_SetGameSpeed(float speed)
    {
        _gameSpeed.Value = speed;
    }

    [Client]
    void HandleGameSpeedChange(float prev, float next, bool asServer)
    {
        if (asServer) return;

        Time.timeScale = next;
    }

    void Update()
    {

    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        _gameSpeed.OnChange -= HandleGameSpeedChange;
        _gameState.OnChange -= HandleGameStateChange;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        OnAllPlayersReadyToStart -= SERVER_StartGame;
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
        EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel);

        if (GUILayout.Button("Set speed 0.2x"))
            manager.SERVER_SetGameSpeed(0.2f);

        if (GUILayout.Button("Set speed 0.5x"))
            manager.SERVER_SetGameSpeed(0.5f);

        if (GUILayout.Button("Set speed 1x"))
            manager.SERVER_SetGameSpeed(1f);

    }
}
#endif