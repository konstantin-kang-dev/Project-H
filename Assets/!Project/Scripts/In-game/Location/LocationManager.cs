
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    public List<Transform> LocationEnemyKeyPoints = new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetRandomClosestPoint(Vector3 point)
    {

        List<Transform> points = LocationEnemyKeyPoints.OrderBy((x)=> Vector3.Distance(x.position, point)).ToList();

        int randomValidPoints = points.Count < 3 ? points.Count : 3;
        points = points.GetRange(0, randomValidPoints);

        int randomKey = Random.Range(0, points.Count);
        return points[randomKey];
    }
}