using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.Stats
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("Achievements")]
        [SerializeField] private List<Achievement> allAchievements = new List<Achievement>();

        [Header("State")]
        private AchievementState achievementState = new AchievementState();

        [Header("Events")]
        public UnityEvent<Achievement> OnAchievementUnlocked = new UnityEvent<Achievement>();

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

            LoadAchievementStates();
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
                if (PlayerStatsManager.Instance != null)
                {
                    break;
                }
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (PlayerStatsManager.Instance != null)
            {
                SubscribeToEvents();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[AchievementManager] Initialized with {allAchievements.Count} achievements, {achievementState.GetUnlockCount()} unlocked");
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[AchievementManager] PlayerStatsManager not available after timeout");
            }
        }

        private void SubscribeToEvents()
        {
            if (PlayerStatsManager.Instance != null)
            {
                PlayerStatsManager.Instance.OnStatChanged.AddListener(OnStatChanged);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnsubscribeFromEvents();
                SaveAchievementStates();
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (PlayerStatsManager.Instance != null)
            {
                PlayerStatsManager.Instance.OnStatChanged.RemoveListener(OnStatChanged);
            }
        }

        private void OnApplicationQuit()
        {
            SaveAchievementStates();
        }

        // ===== EVENT LISTENERS =====

        private void OnStatChanged(StatType type, float value, string context)
        {
            // Check all achievements that match this stat type
            CheckAchievements(type, value, context);
        }

        // ===== ACHIEVEMENT CHECKING =====

        private void CheckAchievements(StatType statType, float value, string context)
        {
            foreach (Achievement achievement in allAchievements)
            {
                // Skip already unlocked
                if (achievementState.IsUnlocked(achievement.achievementId))
                    continue;

                // Check if achievement type matches stat type
                if (!DoesStatMatchAchievement(statType, achievement.type))
                    continue;

                // Check context filter if specified
                if (!string.IsNullOrEmpty(achievement.contextFilter) &&
                    achievement.contextFilter != context)
                    continue;

                // Get current value for this achievement
                float currentValue = GetCurrentValueForAchievement(achievement);

                // Check if condition is met
                if (achievement.CheckCondition(currentValue))
                {
                    UnlockAchievement(achievement);
                }
            }
        }

        private bool DoesStatMatchAchievement(StatType statType, AchievementType achievementType)
        {
            switch (achievementType)
            {
                case AchievementType.Kill:
                    return statType == StatType.TotalKills || statType == StatType.KillsByEnemy;
                case AchievementType.Damage:
                    return statType == StatType.TotalDamage || statType == StatType.DamageBySkill;
                case AchievementType.Survival:
                    return statType == StatType.SurvivalTime;
                case AchievementType.Skill:
                    return statType == StatType.SkillsAcquired || statType == StatType.SkillActivations;
                case AchievementType.Wave:
                    return statType == StatType.CurrentWave || statType == StatType.HighestWave;
                case AchievementType.Level:
                    return statType == StatType.CurrentLevel || statType == StatType.HighestLevel;
                case AchievementType.Passive:
                    return statType == StatType.PassiveUpgrades;
                case AchievementType.General:
                    return true; // General achievements can trigger on any stat
                default:
                    return false;
            }
        }

        private float GetCurrentValueForAchievement(Achievement achievement)
        {
            // Determine if we should check run stats or lifetime stats
            bool checkLifetime = achievement.condition == AchievementCondition.Total;

            if (checkLifetime)
            {
                return GetLifetimeValue(achievement);
            }
            else
            {
                return GetRunValue(achievement);
            }
        }

        private float GetLifetimeValue(Achievement achievement)
        {
            if (PlayerStatsManager.Instance == null)
                return 0f;

            PlayerLifetimeStats lifetime = PlayerStatsManager.Instance.GetLifetimeStats();

            switch (achievement.type)
            {
                case AchievementType.Kill:
                    if (!string.IsNullOrEmpty(achievement.contextFilter))
                        return lifetime.GetStat(StatType.KillsByEnemy, achievement.contextFilter);
                    return lifetime.totalKillsAllTime;

                case AchievementType.Damage:
                    if (!string.IsNullOrEmpty(achievement.contextFilter))
                        return lifetime.GetStat(StatType.DamageBySkill, achievement.contextFilter);
                    return lifetime.totalDamageAllTime;

                case AchievementType.Survival:
                    return lifetime.longestSurvivalTime;

                case AchievementType.Wave:
                    return lifetime.highestWaveReached;

                case AchievementType.Level:
                    return lifetime.highestLevelReached;

                case AchievementType.Skill:
                    return lifetime.allSkillsUnlocked.Count;

                case AchievementType.Passive:
                    return lifetime.totalPassiveUpgradesApplied;

                default:
                    return 0f;
            }
        }

        private float GetRunValue(Achievement achievement)
        {
            if (PlayerStatsManager.Instance == null)
                return 0f;

            PlayerRunStats run = PlayerStatsManager.Instance.GetCurrentRun();

            switch (achievement.type)
            {
                case AchievementType.Kill:
                    if (!string.IsNullOrEmpty(achievement.contextFilter) &&
                        run.killsByEnemyType.ContainsKey(achievement.contextFilter))
                        return run.killsByEnemyType[achievement.contextFilter];
                    return run.totalKills;

                case AchievementType.Damage:
                    if (!string.IsNullOrEmpty(achievement.contextFilter) &&
                        run.damageBySkill.ContainsKey(achievement.contextFilter))
                        return run.damageBySkill[achievement.contextFilter];
                    return run.totalDamageDealt;

                case AchievementType.Survival:
                    return run.survivalTime;

                case AchievementType.Wave:
                    return run.currentWave;

                case AchievementType.Level:
                    return run.currentLevel;

                case AchievementType.Skill:
                    return run.skillsAcquiredThisRun.Count;

                case AchievementType.Passive:
                    return run.passiveUpgradesApplied;

                default:
                    return 0f;
            }
        }

        private void UnlockAchievement(Achievement achievement)
        {
            achievement.Unlock();
            achievementState.UnlockAchievement(achievement.achievementId);
            SaveAchievementStates();

            OnAchievementUnlocked?.Invoke(achievement);

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[AchievementManager] Achievement Unlocked: {achievement.displayName}");
        }

        // ===== PUBLIC API =====

        /// <summary>
        /// Check if achievement is unlocked
        /// </summary>
        public bool IsAchievementUnlocked(string achievementId)
        {
            return achievementState.IsUnlocked(achievementId);
        }

        /// <summary>
        /// Get all unlocked achievements
        /// </summary>
        public List<Achievement> GetUnlockedAchievements()
        {
            return allAchievements.Where(a => achievementState.IsUnlocked(a.achievementId)).ToList();
        }

        /// <summary>
        /// Get all locked achievements
        /// </summary>
        public List<Achievement> GetLockedAchievements()
        {
            return allAchievements.Where(a => !achievementState.IsUnlocked(a.achievementId)).ToList();
        }

        /// <summary>
        /// Get all achievements
        /// </summary>
        public List<Achievement> GetAllAchievements()
        {
            return allAchievements;
        }

        /// <summary>
        /// Get achievement progress (0-1)
        /// </summary>
        public float GetAchievementProgress(Achievement achievement)
        {
            if (achievementState.IsUnlocked(achievement.achievementId))
                return 1f;

            float currentValue = GetCurrentValueForAchievement(achievement);
            return achievement.GetProgress(currentValue);
        }

        /// <summary>
        /// Get unlock percentage
        /// </summary>
        public float GetTotalUnlockPercentage()
        {
            if (allAchievements.Count == 0)
                return 0f;

            return (float)achievementState.GetUnlockCount() / allAchievements.Count;
        }

        // ===== PERSISTENCE =====

        private void SaveAchievementStates()
        {
            string json = JsonUtility.ToJson(achievementState);
            PlayerPrefs.SetString("AchievementState", json);
            PlayerPrefs.Save();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[AchievementManager] Saved {achievementState.GetUnlockCount()} unlocked achievements");
        }

        private void LoadAchievementStates()
        {
            if (PlayerPrefs.HasKey("AchievementState"))
            {
                string json = PlayerPrefs.GetString("AchievementState");
                achievementState = JsonUtility.FromJson<AchievementState>(json);

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[AchievementManager] Loaded {achievementState.GetUnlockCount()} unlocked achievements");
            }
            else
            {
                achievementState = new AchievementState();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[AchievementManager] No saved achievement state found, starting fresh");
            }
        }

        /// <summary>
        /// Reset all achievements (dev only)
        /// </summary>
        public void ResetAllAchievements()
        {
            achievementState.ResetAll();
            foreach (Achievement achievement in allAchievements)
            {
                achievement.ResetUnlock();
            }
            SaveAchievementStates();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.LogWarning("[AchievementManager] All achievements reset!");
        }
    }
}
