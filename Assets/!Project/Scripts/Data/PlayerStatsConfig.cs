using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "GameData/PlayerStatsConfig")]
public class PlayerStatsConfig: SerializedScriptableObject
{
    public int Health = 0;
    public float MoveSpeed = 0;
    public int JumpPower = 0;

    public PlayerStatsConfig Clone()
    {
        return (PlayerStatsConfig)this.MemberwiseClone();
    }
}