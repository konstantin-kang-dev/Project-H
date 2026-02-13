using System.Collections.Generic;
using UnityEngine;

public class ItemsContainer : MonoBehaviour
{
    [SerializeField] List<Transform> _spawnPoints = new List<Transform>();

    public List<Transform> GetAllSpawnPoints()
    {
        return _spawnPoints;
    }
}
