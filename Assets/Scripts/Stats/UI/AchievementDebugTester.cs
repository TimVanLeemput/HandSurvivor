using UnityEngine;
using HandSurvivor.Stats;
using MyBox;

namespace HandSurvivor.Stats.UI
{
    /// <summary>
    /// Debug helper for testing achievement unlocks and notifications
    /// Add this to a GameObject in your scene for easy testing
    /// </summary>
    public class AchievementDebugTester : MonoBehaviour
    {
        [Header("Test Achievement")]
        [SerializeField] private Achievement testAchievement;

        [Header("Alternative: Find by ID")]
        [SerializeField] private string achievementIdToTest = "";

        [Header("Auto Stats (for testing triggers)")]
        [SerializeField] private int killsToAdd = 10;
        [SerializeField] private float damageToAdd = 100f;
        [SerializeField] private int levelsToAdd = 1;

        [Header("Info")]
        [SerializeField] private bool showDebugLogs = true;

        /// <summary>
        /// Manually trigger the test achievement (shows notification)
        /// </summary>
        [ContextMenu("Trigger Test Achievement")]
        [ButtonMethod]
        public void TriggerTestAchievement()
        {
            if (testAchievement == null)
            {
                Debug.LogWarning("[AchievementDebugTester] No test achievement assigned!");
                return;
            }

            if (AchievementNotificationManager.Instance == null)
            {
                Debug.LogWarning("[AchievementDebugTester] AchievementNotificationManager not found!");
                return;
            }

            // Manually show notification (bypasses AchievementManager unlock logic)
            AchievementNotificationManager.Instance.ShowNotification(testAchievement);

            if (showDebugLogs)
                Debug.Log($"[AchievementDebugTester] Triggered notification for: {testAchievement.displayName}");
        }

        /// <summary>
        /// Force unlock achievement through AchievementManager (proper unlock with state save)
        /// </summary>
        [ContextMenu("Force Unlock Achievement (Proper)")]
        [ButtonMethod]
        public void ForceUnlockAchievement()
        {
            Achievement achievement = GetTargetAchievement();
            if (achievement == null) return;

            if (AchievementManager.Instance == null)
            {
                Debug.LogWarning("[AchievementDebugTester] AchievementManager not found!");
                return;
            }

            // Check if already unlocked
            if (AchievementManager.Instance.IsAchievementUnlocked(achievement.achievementId))
            {
                Debug.LogWarning($"[AchievementDebugTester] Achievement '{achievement.displayName}' is already unlocked!");
                return;
            }

            // Manually trigger unlock (this will fire the event and show notification)
            achievement.Unlock();
            AchievementManager.Instance.OnAchievementUnlocked?.Invoke(achievement);

            if (showDebugLogs)
                Debug.Log($"[AchievementDebugTester] Force unlocked: {achievement.displayName}");
        }

        /// <summary>
        /// Test rapid multiple notifications (stacking behavior)
        /// </summary>
        [ContextMenu("Trigger 3 Rapid Notifications")]
        [ButtonMethod]
        public void TriggerMultipleNotifications()
        {
            if (testAchievement == null)
            {
                Debug.LogWarning("[AchievementDebugTester] No test achievement assigned!");
                return;
            }

            if (AchievementNotificationManager.Instance == null)
            {
                Debug.LogWarning("[AchievementDebugTester] AchievementNotificationManager not found!");
                return;
            }

            // Show same achievement 3 times (for testing stacking)
            for (int i = 0; i < 3; i++)
            {
                AchievementNotificationManager.Instance.ShowNotification(testAchievement);
            }

            if (showDebugLogs)
                Debug.Log($"[AchievementDebugTester] Triggered 3 rapid notifications");
        }

        /// <summary>
        /// Add kills to trigger kill-based achievements
        /// </summary>
        [ContextMenu("Add Test Kills")]
        [ButtonMethod]
        public void AddTestKills()
        {
            if (PlayerStatsManager.Instance == null)
            {
                Debug.LogWarning("[AchievementDebugTester] PlayerStatsManager not found!");
                return;
            }

            for (int i = 0; i < killsToAdd; i++)
            {
                PlayerStatsManager.Instance.RecordKill("TestEnemy");
            }

            if (showDebugLogs)
                Debug.Log($"[AchievementDebugTester] Added {killsToAdd} kills. Total: {PlayerStatsManager.Instance.GetCurrentRun().totalKills}");
        }

        /// <summary>
        /// Add damage to trigger damage-based achievements
        /// </summary>
        [ContextMenu("Add Test Damage")]
        [ButtonMethod]
        public void AddTestDamage()
        {
            if (PlayerStatsManager.Instance == null)
            {
                Debug.LogWarning("[AchievementDebugTester] PlayerStatsManager not found!");
                return;
            }

            PlayerStatsManager.Instance.RecordDamage("TestSkill", damageToAdd);

            if (showDebugLogs)
                Debug.Log($"[AchievementDebugTester] Added {damageToAdd} damage. Total: {PlayerStatsManager.Instance.GetCurrentRun().totalDamageDealt}");
        }

        /// <summary>
        /// Add XP to trigger level-based achievements
        /// </summary>
        [ContextMenu("Add Test XP (Level Up)")]
        [ButtonMethod]
        public void AddTestXP()
        {
            if (XPManager.Instance == null)
            {
                Debug.LogWarning("[AchievementDebugTester] XPManager not found!");
                return;
            }

            // Add enough XP to level up from current level
            int xpNeeded = XPManager.Instance.GetXPForNextLevel() - XPManager.Instance.GetCurrentXP();
            XPManager.Instance.AddXP(xpNeeded);

            if (showDebugLogs)
                Debug.Log($"[AchievementDebugTester] Added {xpNeeded} XP. Current Level: {XPManager.Instance.CurrentLevel}");
        }

        /// <summary>
        /// Clear all active notifications
        /// </summary>
        [ContextMenu("Clear All Notifications")]
        [ButtonMethod]
        public void ClearNotifications()
        {
            if (AchievementNotificationManager.Instance != null)
            {
                AchievementNotificationManager.Instance.ClearAllNotifications();
                if (showDebugLogs)
                    Debug.Log("[AchievementDebugTester] Cleared all notifications");
            }
        }

        /// <summary>
        /// Reset all achievements (WARNING: clears unlock state)
        /// </summary>
        [ContextMenu("RESET All Achievements")]
        [ButtonMethod]
        public void ResetAllAchievements()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.ResetAllAchievements();
                if (showDebugLogs)
                    Debug.LogWarning("[AchievementDebugTester] RESET all achievements!");
            }
        }

        /// <summary>
        /// Reset player stats (WARNING: clears all run/lifetime stats)
        /// </summary>
        [ContextMenu("RESET Player Stats")]
        [ButtonMethod]
        public void ResetPlayerStats()
        {
            if (PlayerStatsManager.Instance != null)
            {
                PlayerStatsManager.Instance.ResetAllStats();
                if (showDebugLogs)
                    Debug.LogWarning("[AchievementDebugTester] RESET all player stats!");
            }
        }

        /// <summary>
        /// Show current achievement progress
        /// </summary>
        [ContextMenu("Show Achievement Progress")]
        [ButtonMethod]
        public void ShowAchievementProgress()
        {
            Achievement achievement = GetTargetAchievement();
            if (achievement == null) return;

            if (AchievementManager.Instance == null)
            {
                Debug.LogWarning("[AchievementDebugTester] AchievementManager not found!");
                return;
            }

            bool unlocked = AchievementManager.Instance.IsAchievementUnlocked(achievement.achievementId);
            float progress = AchievementManager.Instance.GetAchievementProgress(achievement);
            string progressString = AchievementManager.Instance.GetAchievementProgressString(achievement);

            Debug.Log($"[AchievementDebugTester] Achievement: {achievement.displayName}\n" +
                      $"Unlocked: {unlocked}\n" +
                      $"Progress: {progress:P0} ({progressString})");
        }

        private Achievement GetTargetAchievement()
        {
            // Prefer direct assignment
            if (testAchievement != null)
                return testAchievement;

            // Fall back to finding by ID
            if (!string.IsNullOrEmpty(achievementIdToTest) && AchievementManager.Instance != null)
            {
                Achievement found = AchievementManager.Instance.GetAllAchievements()
                    .Find(a => a.achievementId == achievementIdToTest);

                if (found != null)
                    return found;

                Debug.LogWarning($"[AchievementDebugTester] Achievement with ID '{achievementIdToTest}' not found!");
                return null;
            }

            Debug.LogWarning("[AchievementDebugTester] No achievement specified!");
            return null;
        }
    }
}
