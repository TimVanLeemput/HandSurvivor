using UnityEngine;

namespace HandSurvivor.Stats.Platforms
{
    /// <summary>
    /// Mock platform implementation for testing
    /// Demonstrates how to implement IAchievementPlatform
    /// Replace with actual Steam/Meta implementations later
    /// </summary>
    public class MockAchievementPlatform : MonoBehaviour, IAchievementPlatform
    {
        [Header("Settings")]
        [SerializeField] private string platformName = "Mock";
        [SerializeField] private bool isEnabled = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private bool isInitialized = false;

        public void Initialize()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[MockAchievementPlatform] Initialized - {platformName}");

            // Register with bridge
            if (PlatformAchievementBridge.Instance != null)
            {
                PlatformAchievementBridge.Instance.RegisterPlatform(this);
            }
        }

        public void UnlockAchievement(string achievementId)
        {
            if (!isEnabled)
                return;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[MockAchievementPlatform] Achievement unlocked on {platformName}: {achievementId}");

            // In a real implementation, this would call the platform SDK
            // For example: SteamUserStats.SetAchievement(achievementId);
        }

        public void SetAchievementProgress(string achievementId, float progress)
        {
            if (!isEnabled)
                return;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[MockAchievementPlatform] Achievement progress on {platformName}: {achievementId} = {progress:F2}");

            // In a real implementation, this would update platform-specific progress
            // For example: SteamUserStats.IndicateAchievementProgress(achievementId, (uint)(progress * 100), 100);
        }

        public bool IsAvailable()
        {
            return isEnabled && isInitialized;
        }

        public string GetPlatformName()
        {
            return platformName;
        }

        private void OnDestroy()
        {
            // Unregister from bridge
            if (PlatformAchievementBridge.Instance != null)
            {
                PlatformAchievementBridge.Instance.UnregisterPlatform(this);
            }
        }
    }
}
