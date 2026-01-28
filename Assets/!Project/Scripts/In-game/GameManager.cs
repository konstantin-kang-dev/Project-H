using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using Zenject.Asteroids;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {  get; private set; }

    readonly SyncVar<GameState> _gameState = new SyncVar<GameState>();
    public GameState GameState => _gameState.Value;

    [SerializeField] Player _playerPrefab;
    [SerializeField] List<Transform> _spawnPoints = new List<Transform>();

    readonly SyncVar<Dictionary<int, bool>> _playersReadyToStart = new SyncVar<Dictionary<int, bool>>();
    public event Action OnAllPlayersReadyToStart;

    [field: SerializeField] public Player LocalPlayer { get; private set; }

    Dictionary<int, Player> _players = new Dictionary<int, Player>();
    public Dictionary<int, Player> Players => _players;

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

        RPC_RequestSetReadyToStart(ClientManager.Connection.ClientId);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _gameState.Value = GameState.PreparingToStart;
        _playersReadyToStart.Value = new Dictionary<int, bool>();
        OnAllPlayersReadyToStart += SERVER_StartGame;
        
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
        SpawnPlayers();

        foreach (var playerBlock in _players)
        {
            Player player = playerBlock.Value;
            player.SERVER_SetReadyToInit(true);
        }

        await UniTask.WaitForSeconds(3);

        _gameState.Value = GameState.Started;
    }

    [Server]
    void SpawnPlayers()
    {
        Dictionary<int, NetworkPlayerData> playersData = LobbyManager.Instance.ConnectedPlayers;

        int key = 0;
        foreach (var playerDataBlock in playersData)
        {
            NetworkPlayerData networkPlayerData = playerDataBlock.Value;
            NetworkConnection connection;
            if (ServerManager.Clients.TryGetValue(networkPlayerData.ClientId, out NetworkConnection conn))
            {
                connection = conn;
            }
            else
            {
                continue;
            }
            Debug.Log($"[GameManager] Spawned player for: {networkPlayerData.ClientId}");

            Player player = Instantiate(_playerPrefab);
            Spawn(player, connection);

            player.SERVER_SetPlayerName(networkPlayerData.PlayerName);
            player.SERVER_SetModelKey(networkPlayerData.ModelKey);

            Transform spawnPoint = _spawnPoints[key];
            player.transform.position = spawnPoint.position;

            _players.Add(playerDataBlock.Key, player);

            key += 1;
        }
    }

    [Client]
    public void RegisterLocalPlayer(Player player)
    {
        LocalPlayer = player;
    }


    [ServerRpc(RequireOwnership = false)]
    public void RPC_RequestSetReadyToStart(int clientId)
    {
        _playersReadyToStart.Value.Add(clientId, true);

        if(_playersReadyToStart.Value.Count == LobbyManager.Instance.ConnectedPlayers.Count)
        {
            OnAllPlayersReadyToStart?.Invoke();
        }

        Debug.Log($"[GameManager] Player {clientId} is ready to start game. Total {_playersReadyToStart.Value.Count}/{LobbyManager.Instance.ConnectedPlayers.Count}");
    }

    [Client]
    void HandleGameStateChange(GameState prev, GameState next, bool asServer)
    {
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
    }

    void Update()
    {

    }
}