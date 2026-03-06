using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class ItemsContainer : MonoBehaviour
{
    [SerializeField] List<NetworkObject> _spawnPoints = new List<NetworkObject>();

    public List<NetworkObject> GetAllSpawnPoints()
    {
        return _spawnPoints;
    }
}
