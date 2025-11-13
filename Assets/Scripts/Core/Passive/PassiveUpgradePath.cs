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
        [Header("Configuration")]
        [SerializeField] private int maxCustomBehaviorLevel = 5;
        [Tooltip("Current passive level for this skill")]
        [SerializeField,ReadOnly] private int currentLevel = 1;

        [SerializeField] private LevelBehavior[] levelBehaviors;

        public int CurrentLevel => currentLevel;
        public int MaxCustomBehaviorLevel => maxCustomBehaviorLevel;

        /// <summary>
        /// Apply passive upgrade - increments level and triggers behavior changes
        /// </summary>
        public void ApplyPassiveUpgrade()
        {
            currentLevel++;

            // Apply custom behavior if within defined range
            if (currentLevel <= maxCustomBehaviorLevel && currentLevel <= levelBehaviors.Length)
            {
                ApplyLevelBehavior(currentLevel);
            }
            else
            {
                // Beyond max custom level - only damage scaling applies
                Debug.Log($"[PassiveUpgradePath] Level {currentLevel} - Custom behaviors maxed, damage scaling continues");
            }
        }

        /// <summary>
        /// Apply behavior for specific level
        /// </summary>
        private void ApplyLevelBehavior(int level)
        {
            if (level < 1 || level > levelBehaviors.Length)
            {
                Debug.LogWarning($"[PassiveUpgradePath] Level {level} out of range");
                return;
            }

            LevelBehavior behavior = levelBehaviors[level - 1];
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
                Debug.LogWarning("[PassiveUpgradePath] Max custom behavior level must be at least 1");
            }

            if (levelBehaviors != null && levelBehaviors.Length > maxCustomBehaviorLevel)
            {
                Debug.LogWarning($"[PassiveUpgradePath] {levelBehaviors.Length} behaviors defined but max level is {maxCustomBehaviorLevel}");
            }
        }
    }
}
