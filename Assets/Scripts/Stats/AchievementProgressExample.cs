using UnityEngine;
using HandSurvivor.Stats;
using MyBox;

/// <summary>
/// Example script showing how to track achievement progress
/// This is just a reference - delete or disable when not needed
/// </summary>
public class AchievementProgressExample : MonoBehaviour
{
    [Header("Example Achievement")] [SerializeField]
    private Achievement exampleAchievement;

    private void Start()
    {
        // Subscribe to achievement unlocks
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked.AddListener(OnAchievementUnlocked);
        }
    }

    private void Update()
    {
        PrintAllAchievementProgress();
        PrintAchievementProgress(exampleAchievement);
    }

    private void OnAchievementUnlocked(Achievement achievement)
    {
        Debug.Log($"🏆 Achievement Unlocked: {achievement.displayName} - {achievement.description}");
    }

    [ButtonMethod]
    private void PrintAllAchievementProgress()
    {
        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("AchievementManager not found!");
            return;
        }

        Debug.Log("=== ACHIEVEMENT PROGRESS ===");

        var allAchievements = AchievementManager.Instance.GetAllAchievements();

        if (allAchievements.Count == 0)
        {
            Debug.LogWarning(
                "No achievements found! Make sure to assign achievements to AchievementManager in the Inspector.");
            return;
        }

        foreach (var achievement in allAchievements)
        {
            PrintAchievementProgress(achievement);
        }

        float totalProgress = AchievementManager.Instance.GetTotalUnlockPercentage() * 100f;
        Debug.Log($"Total Progress: {totalProgress:F1}%");
    }

    [ButtonMethod]
    private void PrintCurrentStats()
    {
        if (PlayerStatsManager.Instance == null)
        {
            Debug.LogWarning("PlayerStatsManager not found!");
            return;
        }

        var run = PlayerStatsManager.Instance.GetCurrentRun();
        var lifetime = PlayerStatsManager.Instance.GetLifetimeStats();

        Debug.Log("=== CURRENT RUN STATS ===");
        Debug.Log($"Kills: {run.totalKills}");
        Debug.Log($"Damage: {run.totalDamageDealt:F0}");
        Debug.Log($"Survival Time: {run.survivalTime:F1}s");
        Debug.Log($"Level: {run.currentLevel}");
        Debug.Log($"Wave: {run.currentWave}");

        Debug.Log("=== LIFETIME STATS ===");
        Debug.Log($"Total Kills: {lifetime.totalKillsAllTime}");
        Debug.Log($"Total Damage: {lifetime.totalDamageAllTime:F0}");
        Debug.Log($"Highest Wave: {lifetime.highestWaveReached}");
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