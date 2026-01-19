using System;
using UnityEditor;
using UnityEngine;

public class PlayerStats
{
    public int Health {  get; private set; }
    public int MoveSpeed {  get; private set; }
    public int JumpPower{  get; private set; }
    public event Action<int> OnHealthChanged;

    public PlayerStats(PlayerStatsData playerStatsData)
    {
        Health = playerStatsData.Health;
        MoveSpeed = playerStatsData.MoveSpeed;
        JumpPower = playerStatsData.JumpPower;
    }
}