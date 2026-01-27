using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDifficultyConfig", menuName = "GameData/GameDifficultyConfig")]
public class GameDifficultyConfig : ScriptableObject
{
    public string DifficultyName = "";
    public PlayerStatsConfig PlayersStats;
    public EnemyStatsConfig EnemyStats;

    public GameDifficultyConfig Clone()
    {
        GameDifficultyConfig config = ScriptableObject.CreateInstance<GameDifficultyConfig>();
        config.DifficultyName = DifficultyName;
        config.PlayersStats = PlayersStats.Clone();
        config.EnemyStats = EnemyStats.Clone();

        return config;
    }
}