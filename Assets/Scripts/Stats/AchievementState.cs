using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor.Stats
{
    /// <summary>
    /// Serializable state for all achievements
    /// Used for saving/loading achievement unlock states to PlayerPrefs
    /// </summary>
    [Serializable]
    public class AchievementState
    {
        public List<string> unlockedAchievementIds = new List<string>();
        public List<string> unlockedDates = new List<string>();

        /// <summary>
        /// Check if achievement is unlocked
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return unlockedAchievementIds.Contains(achievementId);
        }

        /// <summary>
        /// Add unlocked achievement
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!unlockedAchievementIds.Contains(achievementId))
            {
                unlockedAchievementIds.Add(achievementId);
                unlockedDates.Add(DateTime.Now.ToString("O"));
            }
        }

        /// <summary>
        /// Get unlock date for achievement
        /// </summary>
        public DateTime? GetUnlockDate(string achievementId)
        {
            int index = unlockedAchievementIds.IndexOf(achievementId);
            if (index >= 0 && index < unlockedDates.Count)
            {
                if (DateTime.TryParse(unlockedDates[index], out DateTime date))
                    return date;
            }
            return null;
        }

        /// <summary>
        /// Reset all achievement states (dev only)
        /// </summary>
        public void ResetAll()
        {
            unlockedAchievementIds.Clear();
            unlockedDates.Clear();
        }

        /// <summary>
        /// Get unlock count
        /// </summary>
        public int GetUnlockCount()
        {
            return unlockedAchievementIds.Count;
        }
    }
}
