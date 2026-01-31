using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectivesPointsManager: SerializedMonoBehaviour
{
    public static ObjectivesPointsManager Instance;

    [SerializeField] List<Transform> _points = new List<Transform>();
    Queue<Transform> _freePoints = new Queue<Transform>();

    private void Awake()
    {
        Instance = this;

        ResetAll();
    }

    public Transform GetFreePoint()
    {
        if(_freePoints.Count == 0) return null;

        return _freePoints.Dequeue();
    }

    public void ResetAll()
    {
        _freePoints.Clear();

        foreach (var point in _points)
        {
            for (var i = 0; i < point.childCount; i++)
            {
                Transform child = point.GetChild(i);
                child.SetParent(null);
            }

            _freePoints.Enqueue(point);
        }
    }
}