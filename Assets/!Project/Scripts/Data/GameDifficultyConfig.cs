using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDifficultyConfig", menuName = "GameData/GameDifficultyConfig")]
public class GameDifficultyConfig : SerializedScriptableObject
{
    public string DifficultyName = "";
    public PlayerStatsConfig PlayersStats;
    public EnemyStatsConfig EnemyStats;

    public GameDifficultyConfig Clone()
    {
        GameDifficultyConfig config = CreateInstance<GameDifficultyConfig>();
        config.DifficultyName = DifficultyName;
        config.PlayersStats = PlayersStats.Clone();
        config.EnemyStats = EnemyStats.Clone();

        return config;
    }
}