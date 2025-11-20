using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor.Stats.Platforms
{
    /// <summary>
    /// Bridge between AchievementManager and external platform integrations
    /// Forwards unlock events to all registered platforms (Steam, Meta, etc.)
    /// </summary>
    public class PlatformAchievementBridge : MonoBehaviour
    {
        public static PlatformAchievementBridge Instance;

        [Header("Platform Mapping")]
        [SerializeField] private PlatformIDMapping platformMapping;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private List<IAchievementPlatform> registeredPlatforms = new List<IAchievementPlatform>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            StartCoroutine(InitializeWithRetry());
        }

        private IEnumerator InitializeWithRetry()
        {
            float timeout = 5f;
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                if (AchievementManager.Instance != null)
                {
                    break;
                }
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (AchievementManager.Instance != null)
            {
                SubscribeToEvents();
                InitializePlatforms();

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[PlatformAchievementBridge] Initialized with {registeredPlatforms.Count} platforms");
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[PlatformAchievementBridge] AchievementManager not available after timeout");
            }
        }

        private void SubscribeToEvents()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked.AddListener(OnAchievementUnlocked);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnsubscribeFromEvents();
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked.RemoveListener(OnAchievementUnlocked);
            }
        }

        private void InitializePlatforms()
        {
            // Initialize all registered platforms
            foreach (IAchievementPlatform platform in registeredPlatforms)
            {
                platform.Initialize();

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[PlatformAchievementBridge] Initialized platform: {platform.GetPlatformName()}");
            }
        }

        // ===== EVENT HANDLERS =====

        private void OnAchievementUnlocked(Achievement achievement)
        {
            if (achievement == null)
                return;

            // Forward to all registered platforms
            foreach (IAchievementPlatform platform in registeredPlatforms)
            {
                if (platform.IsAvailable())
                {
                    platform.UnlockAchievement(achievement.achievementId);

                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log($"[PlatformAchievementBridge] Unlocked '{achievement.achievementId}' on {platform.GetPlatformName()}");
                }
            }
        }

        // ===== PUBLIC API =====

        /// <summary>
        /// Register a new platform integration
        /// </summary>
        public void RegisterPlatform(IAchievementPlatform platform)
        {
            if (platform == null)
                return;

            if (!registeredPlatforms.Contains(platform))
            {
                registeredPlatforms.Add(platform);
                platform.Initialize();

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[PlatformAchievementBridge] Registered platform: {platform.GetPlatformName()}");
            }
        }

        /// <summary>
        /// Unregister a platform integration
        /// </summary>
        public void UnregisterPlatform(IAchievementPlatform platform)
        {
            if (platform == null)
                return;

            if (registeredPlatforms.Contains(platform))
            {
                registeredPlatforms.Remove(platform);

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[PlatformAchievementBridge] Unregistered platform: {platform.GetPlatformName()}");
            }
        }

        /// <summary>
        /// Get all registered platforms
        /// </summary>
        public List<IAchievementPlatform> GetRegisteredPlatforms()
        {
            return new List<IAchievementPlatform>(registeredPlatforms);
        }

        /// <summary>
        /// Check if a specific platform is registered
        /// </summary>
        public bool IsPlatformRegistered(string platformName)
        {
            foreach (IAchievementPlatform platform in registeredPlatforms)
            {
                if (platform.GetPlatformName() == platformName)
                    return true;
            }
            return false;
        }
    }
}
