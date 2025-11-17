using UnityEngine;
using HandSurvivor.Stats;
using MyBox;
using NUnit.Framework.Constraints;

/// <summary>
/// Example script showing how to track achievement progress
/// This is just a reference - delete or disable when not needed
/// </summary>
public class AchievementProgressExample : MonoBehaviour
{
    [Header("Example Achievement")] [SerializeField]
    private Achievement exampleAchievement;
    
    [SerializeField] private bool showDebugLogs = false;

    private void Start()
    {
        // Subscribe to achievement unlocks
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked.AddListener(OnAchievementUnlocked);
        }
    }

    private void OnAchievementUnlocked(Achievement achievement)
    {
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"🏆 Achievement Unlocked: {achievement.displayName} - {achievement.description}");
    }

    [ButtonMethod]
    private void PrintAllAchievementProgress()
    {
        if (AchievementManager.Instance == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogWarning)

                Debug.LogWarning("AchievementManager not found!");
            return;
        }

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)


            Debug.Log("=== ACHIEVEMENT PROGRESS ===");

        var allAchievements = AchievementManager.Instance.GetAllAchievements();

        if (allAchievements.Count == 0)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogWarning)

                Debug.LogWarning(
                "No achievements found! Make sure to assign achievements to AchievementManager in the Inspector.");
            return;
        }

        foreach (var achievement in allAchievements)
        {
            PrintAchievementProgress(achievement);
        }

        float totalProgress = AchievementManager.Instance.GetTotalUnlockPercentage() * 100f;
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Total Progress: {totalProgress:F1}%");
    }

    [ButtonMethod]
    private void PrintCurrentStats()
    {
        if (PlayerStatsManager.Instance == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogWarning)

                Debug.LogWarning("PlayerStatsManager not found!");
            return;
        }

        var run = PlayerStatsManager.Instance.GetCurrentRun();
        var lifetime = PlayerStatsManager.Instance.GetLifetimeStats();

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)


            Debug.Log("=== CURRENT RUN STATS ===");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Kills: {run.totalKills}");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Damage: {run.totalDamageDealt:F0}");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Survival Time: {run.survivalTime:F1}s");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Level: {run.currentLevel}");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Wave: {run.currentWave}");

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)


            Debug.Log("=== LIFETIME STATS ===");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Total Kills: {lifetime.totalKillsAllTime}");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Total Damage: {lifetime.totalDamageAllTime:F0}");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Highest Wave: {lifetime.highestWaveReached}");
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Total Runs: {lifetime.totalRuns}");
    }

    private void PrintAchievementProgress(Achievement achievement)
    {
        if (AchievementManager.Instance == null)
            return;

        bool isUnlocked = AchievementManager.Instance.IsAchievementUnlocked(achievement.achievementId);
        float progress = AchievementManager.Instance.GetAchievementProgress(achievement);
        string progressString = AchievementManager.Instance.GetAchievementProgressString(achievement);

        string status = isUnlocked ? "✅ UNLOCKED" : $"⏳ {progress * 100f:F1}%";

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)


            Debug.Log($"{status} | {achievement.displayName} | {progressString}");
    }

    /// <summary>
    /// Example: Get progress for specific achievement by ID
    /// </summary>
    public void CheckSpecificAchievement(string achievementId)
    {
        if (AchievementManager.Instance == null)
            return;

        float progress = AchievementManager.Instance.GetAchievementProgress(achievementId);
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)

            Debug.Log($"Achievement '{achievementId}' progress: {progress * 100f:F1}%");
    }

    /// <summary>
    /// Example: Get raw progress values for UI display
    /// </summary>
    public void DisplayProgressInUI(Achievement achievement)
    {
        if (AchievementManager.Instance == null)
            return;

        AchievementManager.Instance.GetAchievementProgressValues(achievement, out float current, out float target);

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog)


            Debug.Log($"Current: {current}, Target: {target}");
        // Use these values to update UI elements like progress bars, text, etc.
    }

    /// <summary>
    /// Example: Check if close to unlocking (within 90%)
    /// </summary>
    public bool IsCloseToUnlock(Achievement achievement, float threshold = 0.9f)
    {
        if (AchievementManager.Instance == null)
            return false;

        float progress = AchievementManager.Instance.GetAchievementProgress(achievement);
        return progress >= threshold && progress < 1f;
    }

    private void OnDestroy()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked.RemoveListener(OnAchievementUnlocked);
        }
    }
}