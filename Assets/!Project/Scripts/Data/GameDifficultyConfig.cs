using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum DifficultyType
{
    FiftyPercent = 0,
    HundredPercent = 1,
    TwoHundredPercent = 2,
    FourHundredPercent = 3,
}

[CreateAssetMenu(fileName = "GameDifficultyConfig", menuName = "GameData/GameDifficultyConfig")]
public class GameDifficultyConfig : SerializedScriptableObject
{
    public DifficultyType DifficultyType;
    public PlayerStatsConfig PlayersStats;
    public int EnemiesCount;
    public EnemyStatsConfig EnemyStats;

    public GameDifficultyConfig Clone()
    {
        GameDifficultyConfig config = CreateInstance<GameDifficultyConfig>();
        config.DifficultyType = DifficultyType;
        config.PlayersStats = PlayersStats.Clone();
        config.EnemiesCount = EnemiesCount;
        config.EnemyStats = EnemyStats.Clone();

        return config;
    }
}