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

    Queue<Transform> _freeCommonPoints = new Queue<Transform>();
    Queue<Transform> _freeRequiredPoints = new Queue<Transform>();

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
        List<Transform> commonPoints = new List<Transform>();
        foreach (var itemContainer in _itemsCommonContainers)
        {
            List<Transform> points = itemContainer.GetAllSpawnPoints();

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
        List<Transform> requiredPoints = new List<Transform>();
        foreach (var itemContainer in _itemsRequiredContainers)
        {
            List<Transform> points = itemContainer.GetAllSpawnPoints();

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

    public Transform GetFreeCommonPoint()
    {
        if(_itemsCommonContainers.Count == 0) return null;

        return _freeCommonPoints.Dequeue();
    }

    public Transform GetFreeRequiredPoint()
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