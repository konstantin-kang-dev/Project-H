using UnityEngine;
using System.Collections;
using FishNet.Object;
using System.Collections.Generic;
using System.Linq;

public class EnemiesManager : NetworkBehaviour
{
    public static EnemiesManager Instance;

    [SerializeField] GameObject _enemyPrefab;
    [SerializeField] List<Transform> _spawnPoints = new List<Transform>();
    List<EnemyController> _enemies = new List<EnemyController>();

    [SerializeField] bool _safeMode = false;
    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        if (!_safeMode)
        {
            SpawnEnemies(1);
        }

        IsInitialized = true;
    }

    void SpawnEnemies(int amount)
    {
        List<Transform> spawnPoints = _spawnPoints.ToList();

        for (int i = 0; i < amount; i++)
        {
            Transform spawnPoint = spawnPoints[i];
            spawnPoints.RemoveAt(i);

            GameObject enemyGO = Instantiate(_enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            EnemyController enemy = enemyGO.GetComponent<EnemyController>();
            Spawn(enemy);

            _enemies.Add(enemy);
        }
    }
}