using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayersSpawnManager : NetworkBehaviour
{
    public static PlayersSpawnManager Instance;

    [SerializeField] Player _playerPrefab;
    [SerializeField] List<Transform> _spawnPoints = new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public Dictionary<int, Player> SpawnPlayers(List<NetworkPlayerData> playersDataList)
    {
        int key = 0;
        Dictionary<int, Player> players = new Dictionary<int, Player>();

        foreach (var networkPlayerData in playersDataList)
        {
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

            Transform spawnPoint = _spawnPoints[key];
            Player player = Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation);

            player.NetworkObject.SetParent(this);

            Spawn(player, connection);

            player.SERVER_SetPlayerName(networkPlayerData.PlayerName);
            player.SERVER_SetModelKey(networkPlayerData.ModelKey);

            players.Add(networkPlayerData.ClientId, player);

            key += 1;
        }

        return players;
    }
}
