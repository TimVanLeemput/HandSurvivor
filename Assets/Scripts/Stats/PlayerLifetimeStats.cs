using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor.Stats
{
    /// <summary>
    /// All-time persistent statistics across all gameplay sessions
    /// Saved to PlayerPrefs and persists between sessions
    /// </summary>
    [Serializable]
    public class PlayerLifetimeStats
    {
        [Header("All-Time Combat Stats")]
        public float totalDamageAllTime;
        public SerializableDictionary<string, float> damageBySkillAllTime = new SerializableDictionary<string, float>();
        public int totalKillsAllTime;
        public SerializableDictionary<string, int> killsByEnemyAllTime = new SerializableDictionary<string, int>();
        public int totalSkillActivationsAllTime;

        [Header("All-Time Records")]
        public int totalRuns;
        public int totalDeaths;
        public float longestSurvivalTime;
        public int highestWaveReached;
        public int highestLevelReached;
        public float totalPlayTime;
        public float highestSingleHitAllTime;

        [Header("Progression")]
        public List<string> allSkillsUnlocked = new List<string>();
        public int totalPassiveUpgradesApplied;

        /// <summary>
        /// Merge current run stats into lifetime stats
        /// </summary>
        public void MergeRunStats(PlayerRunStats run)
        {
            // Add combat stats
            totalDamageAllTime += run.totalDamageDealt;
            totalKillsAllTime += run.totalKills;
            totalSkillActivationsAllTime += run.skillActivations;

            // Merge per-skill damage
            foreach (string skill in run.damageBySkill.ToDictionary().Keys)
            {
                if (!damageBySkillAllTime.ContainsKey(skill))
                    damageBySkillAllTime[skill] = 0f;
                damageBySkillAllTime[skill] += run.damageBySkill[skill];
            }

            // Merge per-enemy kills
            foreach (string enemy in run.killsByEnemyType.ToDictionary().Keys)
            {
                if (!killsByEnemyAllTime.ContainsKey(enemy))
                    killsByEnemyAllTime[enemy] = 0;
                killsByEnemyAllTime[enemy] += run.killsByEnemyType[enemy];
            }

            // Update records
            if (run.survivalTime > longestSurvivalTime)
                longestSurvivalTime = run.survivalTime;

            if (run.currentWave > highestWaveReached)
                highestWaveReached = run.currentWave;

            if (run.currentLevel > highestLevelReached)
                highestLevelReached = run.currentLevel;

            if (run.highestSingleHit > highestSingleHitAllTime)
                highestSingleHitAllTime = run.highestSingleHit;

            // Add unlocked skills
            foreach (string skillId in run.skillsAcquiredThisRun)
            {
                if (!allSkillsUnlocked.Contains(skillId))
                    allSkillsUnlocked.Add(skillId);
            }

            totalPassiveUpgradesApplied += run.passiveUpgradesApplied;
            totalRuns++;
        }

        /// <summary>
        /// Record a death
        /// </summary>
        public void RecordDeath()
        {
            totalDeaths++;
        }

        /// <summary>
        /// Add playtime
        /// </summary>
        public void AddPlayTime(float seconds)
        {
            totalPlayTime += seconds;
        }

        /// <summary>
        /// Get specific stat value
        /// </summary>
        public float GetStat(StatType type, string context = "")
        {
            switch (type)
            {
                case StatType.TotalDamage:
                    return totalDamageAllTime;
                case StatType.TotalKills:
                    return totalKillsAllTime;
                case StatType.TotalRuns:
                    return totalRuns;
                case StatType.TotalDeaths:
                    return totalDeaths;
                case StatType.HighestWave:
                    return highestWaveReached;
                case StatType.HighestLevel:
                    return highestLevelReached;
                case StatType.LongestSurvival:
                    return longestSurvivalTime;
                case StatType.TotalPlayTime:
                    return totalPlayTime;
                case StatType.DamageBySkill:
                    if (!string.IsNullOrEmpty(context) && damageBySkillAllTime.ContainsKey(context))
                        return damageBySkillAllTime[context];
                    return 0f;
                case StatType.KillsByEnemy:
                    if (!string.IsNullOrEmpty(context) && killsByEnemyAllTime.ContainsKey(context))
                        return killsByEnemyAllTime[context];
                    return 0f;
                default:
                    return 0f;
            }
        }
    }
}
