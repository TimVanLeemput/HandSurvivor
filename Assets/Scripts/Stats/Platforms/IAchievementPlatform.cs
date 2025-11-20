namespace HandSurvivor.Stats.Platforms
{
    /// <summary>
    /// Interface for external achievement platform integrations
    /// (Steam, Meta, etc.)
    /// </summary>
    public interface IAchievementPlatform
    {
        /// <summary>
        /// Initialize the platform integration
        /// </summary>
        void Initialize();

        /// <summary>
        /// Unlock an achievement on this platform
        /// </summary>
        /// <param name="achievementId">Internal achievement ID</param>
        void UnlockAchievement(string achievementId);

        /// <summary>
        /// Set achievement progress (if platform supports it)
        /// </summary>
        /// <param name="achievementId">Internal achievement ID</param>
        /// <param name="progress">Progress value (0-1)</param>
        void SetAchievementProgress(string achievementId, float progress);

        /// <summary>
        /// Check if this platform is available/initialized
        /// </summary>
        bool IsAvailable();

        /// <summary>
        /// Get the platform name (e.g., "Steam", "Meta")
        /// </summary>
        string GetPlatformName();
    }
}
