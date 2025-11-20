using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Core.Passive;

namespace HandSurvivor.Stats
{
    public class PlayerStatsManager : MonoBehaviour
    {
        public static PlayerStatsManager Instance;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("Current Run")]
        [SerializeField] private PlayerRunStats currentRun = new PlayerRunStats();

        [Header("Lifetime Stats")]
        [SerializeField] private PlayerLifetimeStats lifetime = new PlayerLifetimeStats();

        [Header("Runtime Tracking")]
        private float runStartTime;
        private bool isRunActive = false;

        [Header("Lifetime Stats Behavior")]
        [Tooltip("If true, lifetime stats update in real-time during run. If false, only update on EndRun()")]
        [SerializeField] private bool updateLifetimeStatsInRealTime = true;

        [Header("Events")]
        public UnityEvent<StatType, float, string> OnStatChanged = new UnityEvent<StatType, float, string>();

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

            LoadStats();
        }

        private void Start()
        {
            StartCoroutine(InitializeWithRetry());
        }

        private IEnumerator InitializeWithRetry()
        {
            // Always start run tracking immediately - don't wait for other managers
            if (!isRunActive)
            {
                StartNewRun();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[PlayerStatsManager] Run tracking started");
            }

            // Wait for other managers to subscribe to their events (optional)
            float timeout = 5f;
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                if (AllManagersReady())
                {
                    break;
                }
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            // Subscribe to events from managers that exist
            SubscribeToEvents();

            // Log which managers are available
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
            {
                Debug.Log($"[PlayerStatsManager] Initialization complete. Available managers: " +
                          $"XP={XPManager.Instance != null}, " +
                          $"Passive={PassiveUpgradeManager.Instance != null}, " +
                          $"ActiveSkills={ActiveSkillInventory.Instance != null}");
            }
        }

        private bool AllManagersReady()
        {
            return XPManager.Instance != null &&
                   PassiveUpgradeManager.Instance != null &&
                   ActiveSkillInventory.Instance != null;
        }

        private void SubscribeToEvents()
        {
            // XP and Level events
            if (XPManager.Instance != null)
            {
                XPManager.Instance.OnXPAdded.AddListener(OnXPAdded);
                XPManager.Instance.OnLevelUp.AddListener(OnLevelUp);
            }

            // Passive upgrade events
            if (PassiveUpgradeManager.Instance != null)
            {
                PassiveUpgradeManager.Instance.OnUpgradeApplied.AddListener(OnPassiveUpgradeApplied);
            }

            // Active skill events
            if (ActiveSkillInventory.Instance != null)
            {
                ActiveSkillInventory.Instance.OnActiveSkillAdded.AddListener(OnSkillAdded);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnsubscribeFromEvents();
                SaveStats();
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (XPManager.Instance != null)
            {
                XPManager.Instance.OnXPAdded.RemoveListener(OnXPAdded);
                XPManager.Instance.OnLevelUp.RemoveListener(OnLevelUp);
            }

            if (PassiveUpgradeManager.Instance != null)
            {
                PassiveUpgradeManager.Instance.OnUpgradeApplied.RemoveListener(OnPassiveUpgradeApplied);
            }

            if (ActiveSkillInventory.Instance != null)
            {
                ActiveSkillInventory.Instance.OnActiveSkillAdded.RemoveListener(OnSkillAdded);
            }
        }

        private void Update()
        {
            if (isRunActive)
            {
                currentRun.survivalTime = Time.time - runStartTime;
            }
        }

        private void OnApplicationQuit()
        {
            SaveStats();
        }

        // ===== PUBLIC API - Direct Stat Recording =====

        /// <summary>
        /// Record damage dealt to enemy
        /// </summary>
        public void RecordDamage(string skillId, float damage, string enemyType = "")
        {
            if (!isRunActive)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[PlayerStatsManager] RecordDamage called but run is not active! Call StartNewRun() first.");
                return;
            }

            currentRun.AddDamage(skillId, damage);

            // Update lifetime stats in real-time if enabled
            if (updateLifetimeStatsInRealTime)
            {
                lifetime.totalDamageAllTime += damage;
                if (!string.IsNullOrEmpty(skillId))
                {
                    if (!lifetime.damageBySkillAllTime.ContainsKey(skillId))
                        lifetime.damageBySkillAllTime[skillId] = 0f;
                    lifetime.damageBySkillAllTime[skillId] += damage;
                }
            }

            OnStatChanged?.Invoke(StatType.TotalDamage, currentRun.totalDamageDealt, "");
            OnStatChanged?.Invoke(StatType.DamageBySkill, damage, skillId);

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[PlayerStatsManager] Damage: {damage} by {skillId} to {enemyType}. Total: {currentRun.totalDamageDealt}");
        }

        /// <summary>
        /// Record enemy kill
        /// </summary>
        public void RecordKill(string enemyType, int xpDropped = 0)
        {
            if (!isRunActive)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[PlayerStatsManager] RecordKill called but run is not active! Call StartNewRun() first.");
                return;
            }

            currentRun.AddKill(enemyType);

            // Update lifetime stats in real-time if enabled
            if (updateLifetimeStatsInRealTime)
            {
                lifetime.totalKillsAllTime++;
                if (!string.IsNullOrEmpty(enemyType))
                {
                    if (!lifetime.killsByEnemyAllTime.ContainsKey(enemyType))
                        lifetime.killsByEnemyAllTime[enemyType] = 0;
                    lifetime.killsByEnemyAllTime[enemyType]++;
                }
            }

            OnStatChanged?.Invoke(StatType.TotalKills, currentRun.totalKills, "");
            OnStatChanged?.Invoke(StatType.KillsByEnemy, 1, enemyType);

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[PlayerStatsManager] Kill: {enemyType}. Total: {currentRun.totalKills}");
        }

        /// <summary>
        /// Record damage taken by Nexus
        /// </summary>
        public void RecordNexusDamage(int damage)
        {
            if (!isRunActive) return;

            currentRun.damageToNexus += damage;
            OnStatChanged?.Invoke(StatType.NexusDamage, currentRun.damageToNexus, "");

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[PlayerStatsManager] Nexus damage: {damage}. Total: {currentRun.damageToNexus}");
        }

        /// <summary>
        /// Record skill activation
        /// </summary>
        public void RecordSkillActivation(string skillId)
        {
            if (!isRunActive) return;

            currentRun.AddSkillActivation(skillId);
            OnStatChanged?.Invoke(StatType.SkillActivations, currentRun.skillActivations, "");
            OnStatChanged?.Invoke(StatType.ActivationsBySkill, 1, skillId);
        }

        /// <summary>
        /// Set current wave number
        /// </summary>
        public void SetCurrentWave(int wave)
        {
            if (!isRunActive) return;

            currentRun.currentWave = wave;
            OnStatChanged?.Invoke(StatType.CurrentWave, wave, "");
        }

        // ===== EVENT LISTENERS =====

        private void OnXPAdded(int amount)
        {
            if (!isRunActive) return;

            currentRun.xpEarnedThisRun += amount;
            OnStatChanged?.Invoke(StatType.XPEarned, currentRun.xpEarnedThisRun, "");
        }

        private void OnLevelUp(int newLevel)
        {
            if (!isRunActive) return;

            currentRun.currentLevel = newLevel;
            OnStatChanged?.Invoke(StatType.CurrentLevel, newLevel, "");
        }

        private void OnSkillAdded(ActiveSkillBase skill, int amount)
        {
            if (!isRunActive || skill == null || skill.Data == null) return;

            string skillId = skill.Data.activeSkillId;
            if (!currentRun.skillsAcquiredThisRun.Contains(skillId))
            {
                currentRun.skillsAcquiredThisRun.Add(skillId);
                OnStatChanged?.Invoke(StatType.SkillsAcquired, currentRun.skillsAcquiredThisRun.Count, skillId);
            }
        }

        private void OnPassiveUpgradeApplied(PassiveUpgradeData upgrade)
        {
            if (!isRunActive) return;

            currentRun.passiveUpgradesApplied++;
            OnStatChanged?.Invoke(StatType.PassiveUpgrades, currentRun.passiveUpgradesApplied, "");
        }

        // ===== RUN MANAGEMENT =====

        /// <summary>
        /// Start tracking a new run
        /// </summary>
        public void StartNewRun()
        {
            currentRun.Reset();
            runStartTime = Time.time;
            isRunActive = true;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[PlayerStatsManager] New run started");
        }

        /// <summary>
        /// End the current run and merge stats into lifetime
        /// </summary>
        public void EndRun(bool victory)
        {
            if (!isRunActive) return;

            isRunActive = false;

            // Merge run stats into lifetime
            lifetime.MergeRunStats(currentRun);
            lifetime.AddPlayTime(currentRun.survivalTime);

            if (!victory)
            {
                lifetime.RecordDeath();
            }

            SaveStats();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[PlayerStatsManager] Run ended. Victory: {victory}, Duration: {currentRun.survivalTime:F1}s, Kills: {currentRun.totalKills}");
        }

        // ===== QUERY METHODS =====

        /// <summary>
        /// Get current run stats
        /// </summary>
        public PlayerRunStats GetCurrentRun() => currentRun;

        /// <summary>
        /// Get lifetime stats
        /// </summary>
        public PlayerLifetimeStats GetLifetimeStats() => lifetime;

        /// <summary>
        /// Get specific stat value from current run
        /// </summary>
        public float GetRunStat(StatType type, string context = "")
        {
            switch (type)
            {
                case StatType.TotalDamage:
                    return currentRun.totalDamageDealt;
                case StatType.TotalKills:
                    return currentRun.totalKills;
                case StatType.CurrentLevel:
                    return currentRun.currentLevel;
                case StatType.CurrentWave:
                    return currentRun.currentWave;
                case StatType.SurvivalTime:
                    return currentRun.survivalTime;
                case StatType.DamageBySkill:
                    if (!string.IsNullOrEmpty(context) && currentRun.damageBySkill.ContainsKey(context))
                        return currentRun.damageBySkill[context];
                    return 0f;
                case StatType.KillsByEnemy:
                    if (!string.IsNullOrEmpty(context) && currentRun.killsByEnemyType.ContainsKey(context))
                        return currentRun.killsByEnemyType[context];
                    return 0f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Get specific stat value from lifetime stats
        /// </summary>
        public float GetLifetimeStat(StatType type, string context = "")
        {
            return lifetime.GetStat(type, context);
        }

        // ===== PERSISTENCE =====

        /// <summary>
        /// Save stats to PlayerPrefs
        /// </summary>
        public void SaveStats()
        {
            string json = JsonUtility.ToJson(lifetime);
            PlayerPrefs.SetString("PlayerLifetimeStats", json);
            PlayerPrefs.Save();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[PlayerStatsManager] Stats saved to PlayerPrefs");
        }

        /// <summary>
        /// Load stats from PlayerPrefs
        /// </summary>
        public void LoadStats()
        {
            if (PlayerPrefs.HasKey("PlayerLifetimeStats"))
            {
                string json = PlayerPrefs.GetString("PlayerLifetimeStats");
                lifetime = JsonUtility.FromJson<PlayerLifetimeStats>(json);

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[PlayerStatsManager] Stats loaded: {lifetime.totalKillsAllTime} total kills, {lifetime.totalRuns} runs");
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[PlayerStatsManager] No saved stats found, starting fresh");
            }
        }

        /// <summary>
        /// Reset all lifetime stats (dev only)
        /// </summary>
        public void ResetAllStats()
        {
            lifetime = new PlayerLifetimeStats();
            currentRun.Reset();
            SaveStats();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.LogWarning("[PlayerStatsManager] All stats reset!");
        }

        /// <summary>
        /// Debug method to check current tracking status
        /// </summary>
        public void LogTrackingStatus()
        {
            Debug.Log($"[PlayerStatsManager] === TRACKING STATUS ===\n" +
                      $"Run Active: {isRunActive}\n" +
                      $"Current Level: {currentRun.currentLevel}\n" +
                      $"Total Kills: {currentRun.totalKills}\n" +
                      $"Total Damage: {currentRun.totalDamageDealt}\n" +
                      $"Survival Time: {currentRun.survivalTime:F1}s\n" +
                      $"Current Wave: {currentRun.currentWave}\n" +
                      $"Lifetime Kills: {lifetime.totalKillsAllTime}\n" +
                      $"Lifetime Damage: {lifetime.totalDamageAllTime}\n" +
                      $"Managers - XP: {XPManager.Instance != null}, Passive: {PassiveUpgradeManager.Instance != null}, ActiveSkills: {ActiveSkillInventory.Instance != null}");
        }
    }
}
