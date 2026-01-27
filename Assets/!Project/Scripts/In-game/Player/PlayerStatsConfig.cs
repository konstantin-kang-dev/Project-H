using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "GameData/PlayerStatsConfig")]
public class PlayerStatsConfig: ScriptableObject
{
    [SerializeField] public int Health = 0;
    [SerializeField] public int MoveSpeed = 0;
    [SerializeField] public int JumpPower = 0;

    public PlayerStatsConfig Clone()
    {
        return (PlayerStatsConfig)this.MemberwiseClone();
    }
}