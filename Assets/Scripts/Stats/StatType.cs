namespace HandSurvivor.Stats
{
    /// <summary>
    /// Types of statistics tracked by the PlayerStatsManager
    /// </summary>
    public enum StatType
    {
        // Combat Stats
        TotalDamage,
        DamageBySkill,
        TotalKills,
        KillsByEnemy,
        SkillActivations,
        ActivationsBySkill,

        // Survival Stats
        SurvivalTime,
        CurrentWave,
        NexusDamage,

        // Progression Stats
        CurrentLevel,
        XPEarned,
        SkillsAcquired,
        PassiveUpgrades,

        // Lifetime Stats
        TotalRuns,
        TotalDeaths,
        HighestWave,
        HighestLevel,
        LongestSurvival,
        TotalPlayTime
    }
}
