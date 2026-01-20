using UnityEditor;
using UnityEngine;

public class EnemyStats : ScriptableObject
{
    public float MoveSpeed = 2f;
    public float BlindFollowDuration = 10f;
    public int RequiredPointsToAggro = 10;
}