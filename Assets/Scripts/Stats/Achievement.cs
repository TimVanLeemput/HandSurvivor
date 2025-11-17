using System;
using UnityEngine;

namespace HandSurvivor.Stats
{
    /// <summary>
    /// Achievement/Trophy definition
    /// Create as ScriptableObject assets in Unity
    /// </summary>
    [CreateAssetMenu(fileName = "Achievement", menuName = "HandSurvivor/Achievement", order = 100)]
    public class Achievement : ScriptableObject
    {
        [Header("Identification")]
        public string achievementId;
        public string displayName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Condition")]
        public AchievementType type;
        public AchievementCondition condition;
        public float targetValue;
        [Tooltip("Optional: Specific skill ID or enemy type for filtering")]
        public string contextFilter = "";

        [Header("State (Runtime)")]
        [HideInInspector]
        public bool isUnlocked;
        [HideInInspector]
        public string unlockedDateString; // Serialized DateTime

        /// <summary>
        /// Check if condition is met based on stat value
        /// </summary>
        public bool CheckCondition(float currentValue)
        {
            switch (condition)
            {
                case AchievementCondition.Total:
                case AchievementCondition.Single:
                    return currentValue >= targetValue;
                case AchievementCondition.Streak:
                    return currentValue >= targetValue;
                case AchievementCondition.Speed:
                    return currentValue <= targetValue; // For time-based, lower is better
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get progress percentage (0-1)
        /// </summary>
        public float GetProgress(float currentValue)
        {
            if (isUnlocked)
                return 1f;

            return Mathf.Clamp01(currentValue / targetValue);
        }

        /// <summary>
        /// Unlock the achievement
        /// </summary>
        public void Unlock()
        {
            if (isUnlocked)
                return;

            isUnlocked = true;
            unlockedDateString = DateTime.Now.ToString("O"); // ISO 8601 format
        }

        /// <summary>
        /// Reset unlock state (dev only)
        /// </summary>
        public void ResetUnlock()
        {
            isUnlocked = false;
            unlockedDateString = "";
        }

        /// <summary>
        /// Get unlocked date
        /// </summary>
        public DateTime? GetUnlockedDate()
        {
            if (string.IsNullOrEmpty(unlockedDateString))
                return null;

            if (DateTime.TryParse(unlockedDateString, out DateTime date))
                return date;

            return null;
        }
    }

    /// <summary>
    /// Type of achievement based on stat category
    /// </summary>
    public enum AchievementType
    {
        Kill,           // Enemy kills
        Damage,         // Damage dealt
        Survival,       // Survival time
        Skill,          // Skill-related (acquisition, usage)
        Wave,           // Wave completion
        Level,          // Player level
        Passive,        // Passive upgrades
        General         // Other/miscellaneous
    }

    /// <summary>
    /// Condition for achievement unlock
    /// </summary>
    public enum AchievementCondition
    {
        Total,          // Cumulative over all time (e.g., 1000 total kills)
        Single,         // Single run/instance (e.g., 100 kills in one run)
        Streak,         // Consecutive events (e.g., 10 kills without taking damage)
        Speed           // Time-based (e.g., survive 10 minutes)
    }
}
