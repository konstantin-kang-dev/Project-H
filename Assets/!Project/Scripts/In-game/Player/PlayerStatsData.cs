using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "GameData/PlayerConfig")]
public class PlayerStatsData: ScriptableObject
{
    [SerializeField] public int Health = 0;
    [SerializeField] public int MoveSpeed = 0;
    [SerializeField] public int JumpPower = 0;
}