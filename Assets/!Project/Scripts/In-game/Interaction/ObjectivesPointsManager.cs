using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectivesPointsManager: SerializedMonoBehaviour
{
    public static ObjectivesPointsManager Instance;

    [SerializeField] List<ItemsContainer> _itemsCommonContainers = new List<ItemsContainer>();
    [SerializeField] List<ItemsContainer> _itemsRequiredContainers = new List<ItemsContainer>();

    Queue<NetworkObject> _freeCommonPoints = new Queue<NetworkObject>();
    Queue<NetworkObject> _freeRequiredPoints = new Queue<NetworkObject>();

    private void Awake()
    {
        Instance = this;

        Init();
    }

    void Init()
    {
        ResetAll();

        InitCommonPoints();
        InitRequiredPoints();
    }

    void InitCommonPoints()
    {
        List<NetworkObject> commonPoints = new List<NetworkObject>();
        foreach (var itemContainer in _itemsCommonContainers)
        {
            List<NetworkObject> points = itemContainer.GetAllSpawnPoints();

            foreach (var point in points)
            {
                commonPoints.Add(point);
            }
        }

        commonPoints.Shuffle();
        foreach (var point in commonPoints)
        {
            _freeCommonPoints.Enqueue(point);
        }
    }

    void InitRequiredPoints()
    {
        List<NetworkObject> requiredPoints = new List<NetworkObject>();
        foreach (var itemContainer in _itemsRequiredContainers)
        {
            List<NetworkObject> points = itemContainer.GetAllSpawnPoints();

            foreach (var point in points)
            {
                requiredPoints.Add(point);
            }
        }

        requiredPoints.Shuffle();
        foreach (var point in requiredPoints)
        {
            _freeRequiredPoints.Enqueue(point);
        }
    }

    public NetworkObject GetFreeCommonPoint()
    {
        if(_itemsCommonContainers.Count == 0) return null;

        return _freeCommonPoints.Dequeue();
    }

    public NetworkObject GetFreeRequiredPoint()
    {
        if(_itemsCommonContainers.Count == 0) return null;

        return _freeCommonPoints.Dequeue();
    }

    public void ResetAll()
    {
        _freeCommonPoints.Clear();
        _freeRequiredPoints.Clear();
    }
}