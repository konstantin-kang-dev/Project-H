using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "GameData/EnemyConfig")]
public class EnemyStats : ScriptableObject
{
    public float MoveSpeed = 2f;
    public float SprintSpeed = 2f;
    public float BlindFollowDuration = 10f;
    public int RequiredPointsToAggro = 10;
}