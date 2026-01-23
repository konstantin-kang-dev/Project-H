using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {  get; private set; }

    [SerializeField] Player _playerPrefab;
    [SerializeField] List<Transform> _spawnPoints = new List<Transform>();

    readonly SyncVar<Dictionary<int, bool>> _playersReadyToStart = new SyncVar<Dictionary<int, bool>>();
    public event Action OnAllPlayersReadyToStart;

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

        if (!IsServerStarted)
        {
            OnAllPlayersReadyToStart += Init;
        }
        RPC_RequestSetReadyToStart(ClientManager.Connection.ClientId);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _playersReadyToStart.Value = new Dictionary<int, bool>();
        OnAllPlayersReadyToStart += Init;
        SpawnPlayers();
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

            Transform spawnPoint = _spawnPoints[key];
            player.transform.position = spawnPoint.position;

            _players.Add(playerDataBlock.Key, player);

            key += 1;
        }
    }

    [Server]
    void SetupPlayers()
    {
        Dictionary<int, NetworkPlayerData> playersData = LobbyManager.Instance.ConnectedPlayers;

        foreach (var playerDataBlock in playersData)
        {
            if (!_players.ContainsKey(playerDataBlock.Key)) continue;
            NetworkPlayerData networkPlayerData = playerDataBlock.Value;
            Debug.Log($"[GameManager] Set up player for: {networkPlayerData.ClientId}");

            Player player = _players[playerDataBlock.Key];

            player.SERVER_SetPlayerName(networkPlayerData.PlayerName);
            player.SERVER_SetModelKey(networkPlayerData.ModelKey);

        }
    }

    public void Init()
    {
        if (IsServerStarted)
        {
            SetupPlayers();

        }
        else
        {

        }

        Debug.Log($"[GameManager] Initialized");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RPC_RequestSetReadyToStart(int clientId)
    {
        _playersReadyToStart.Value.Add(clientId, true);

        if(_playersReadyToStart.Value.Count == LobbyManager.Instance.ConnectedPlayers.Count)
        {
            OnAllPlayersReadyToStart?.Invoke();
        }
    }

    void Update()
    {

    }
}