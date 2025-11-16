using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.Core.Passive
{
    /// <summary>
    /// Defines custom behavior upgrades for an active skill based on passive level
    /// Attach to ActiveSkill prefabs to define level-specific visual/behavior changes
    /// </summary>
    public class PassiveUpgradePath : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Configuration")] [SerializeField]
        private int maxCustomBehaviorLevel = 5;

        [Tooltip("Current passive level for this skill")] [SerializeField, ReadOnly]
        private int currentLevel = 1;

        [Tooltip("Specific passive upgrade that triggers this path (leave null for any upgrade)")] [SerializeField]
        private PassiveUpgradeData targetPassiveUpgrade;

        [Tooltip("Active skill data reference (for calculating max levels)")] [SerializeField]
        private HandSurvivor.ActiveSkills.ActiveSkillData activeSkillData;

        [Header("Calculated Info")]
        [Tooltip("Calculated max levels needed to reach min multiplier (based on passive upgrade value)")]
        [SerializeField, ReadOnly]
        private int calculatedMaxLevels = 0;

        [SerializeField] private LevelBehavior[] levelBehaviors;

        public int CurrentLevel => currentLevel;
        public int MaxCustomBehaviorLevel => maxCustomBehaviorLevel;
        public PassiveUpgradeData TargetPassiveUpgrade => targetPassiveUpgrade;

        /// <summary>
        /// Apply passive upgrade - increments level and triggers behavior changes
        /// </summary>
        /// <param name="appliedUpgrade">The passive upgrade that was applied (null = accept any)</param>
        public void ApplyPassiveUpgrade(PassiveUpgradeData appliedUpgrade = null)
        {
            // Check if this path should respond to the applied upgrade
            if (targetPassiveUpgrade != null && appliedUpgrade != targetPassiveUpgrade)
            {
                // This path only responds to a specific upgrade, and this isn't it
                return;
            }

            currentLevel++;

            // Apply custom behavior if within defined range
            if (currentLevel <= maxCustomBehaviorLevel && currentLevel <= levelBehaviors.Length)
            {
                ApplyLevelBehavior(currentLevel);
            }
            else
            {
                // Beyond max custom level - only damage scaling applies
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log(
                    $"[PassiveUpgradePath] Level {currentLevel} - Custom behaviors maxed, damage scaling continues");
            }
        }

        /// <summary>
        /// Apply behavior for specific level
        /// </summary>
        private void ApplyLevelBehavior(int level)
        {
            if (level < 1 || level > levelBehaviors.Length)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning($"[PassiveUpgradePath] Level {level} out of range");
                return;
            }

            LevelBehavior behavior = levelBehaviors[level - 1];
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[PassiveUpgradePath] Applying Level {level}: {behavior.description}");

            // Invoke Unity Events for custom logic
            behavior.onLevelReached?.Invoke();

            // Enable/disable GameObjects
            foreach (GameObject obj in behavior.objectsToEnable)
            {
                if (obj != null)
                    obj.SetActive(true);
            }

            foreach (GameObject obj in behavior.objectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        /// <summary>
        /// Set level directly (useful for testing or save/load)
        /// </summary>
        public void SetLevel(int level)
        {
            currentLevel = Mathf.Clamp(level, 1, int.MaxValue);

            // Apply all behaviors up to current level
            for (int i = 1; i <= Mathf.Min(currentLevel, maxCustomBehaviorLevel); i++)
            {
                if (i <= levelBehaviors.Length)
                {
                    ApplyLevelBehavior(i);
                }
            }
        }

        [System.Serializable]
        public class LevelBehavior
        {
            [Tooltip("Description of what changes at this level")]
            public string description;

            [Tooltip("Unity Event triggered when this level is reached")]
            public UnityEvent onLevelReached;

            [Tooltip("GameObjects to enable at this level")]
            public GameObject[] objectsToEnable;

            [Tooltip("GameObjects to disable at this level")]
            public GameObject[] objectsToDisable;
        }

        private void OnValidate()
        {
            if (maxCustomBehaviorLevel < 1)
            {
                maxCustomBehaviorLevel = 1;
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning("[PassiveUpgradePath] Max custom behavior level must be at least 1");
            }

            if (levelBehaviors != null && levelBehaviors.Length > maxCustomBehaviorLevel)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning(
                    $"[PassiveUpgradePath] {levelBehaviors.Length} behaviors defined but max level is {maxCustomBehaviorLevel}");
            }
        }

        /// <summary>
        /// Calculate how many levels are needed to reach the minimum cooldown/size multiplier
        /// Called from editor script button
        /// </summary>
        public void CalculateMaxLevelsEditor()
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log(" Max Levels being calculated!");

            if (targetPassiveUpgrade == null || activeSkillData == null)
            {
                calculatedMaxLevels = 0;
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log(" [PassiveUpgradePath] Invalid target upgrade or active skill data");

                return;
            }

            // Only calculate for Cooldown and Size types
            if (targetPassiveUpgrade.type != PassiveType.CooldownReduction &&
                targetPassiveUpgrade.type != PassiveType.SizeIncrease)
            {
                calculatedMaxLevels = 0;
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log("Only CooldownReduction and SizeIncrease types are supported for this calculation");
                return;
            }

            // Calculate for cooldown reduction
            if (targetPassiveUpgrade.type == PassiveType.CooldownReduction)
            {
                float minMultiplier = activeSkillData.minCooldownMultiplier;
                float upgradeValuePercent = targetPassiveUpgrade.value / 100f;

                if (upgradeValuePercent <= 0f)
                {
                    calculatedMaxLevels = 0;
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                        Debug.Log($" [PassiveUpgradePath] Invalid upgrade value: {targetPassiveUpgrade.value}");

                    return;
                }
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log(" [PassiveUpgradePath] Calculating max levels for CooldownReduction");
                

                // Starting from 1.0, how many upgrades to reach minMultiplier?
                // 1.0 - (upgradeValue * levels) = minMultiplier
                // levels = (1.0 - minMultiplier) / upgradeValue
                float levelsNeeded = (1f - minMultiplier) / upgradeValuePercent;
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($" [PassiveUpgradePath] Levels needed: {levelsNeeded}");
                calculatedMaxLevels = Mathf.CeilToInt(levelsNeeded);
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($" [PassiveUpgradePath] Max levels calculated: {calculatedMaxLevels}");
            }
            // For size, there's no hard limit, so we use a different calculation
            else if (targetPassiveUpgrade.type == PassiveType.SizeIncrease)
            {
                // For size, use the maxCustomBehaviorLevel as the meaningful max
                calculatedMaxLevels = maxCustomBehaviorLevel;
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log(
                    $"[PassiveUpgradePath] Max levels calculated for SizeIncrease: calculcated : {calculatedMaxLevels}    ");
            }
        }
    }
}