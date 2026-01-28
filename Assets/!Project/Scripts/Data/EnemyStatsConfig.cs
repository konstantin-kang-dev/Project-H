using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatsConfig", menuName = "GameData/EnemyStatsConfig")]
public class EnemyStatsConfig : SerializedScriptableObject
{
    public float MoveSpeed = 2f;
    public float SprintSpeed = 2f;
    public float BlindFollowDuration = 10f;
    public int RequiredPointsToAggro = 10;

    public EnemyStatsConfig Clone()
    {
        return (EnemyStatsConfig)this.MemberwiseClone();
    }
}