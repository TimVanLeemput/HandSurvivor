using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Upgrades;

namespace HandSurvivor.Core.Passive
{
    public class PassiveUpgradeManager : MonoBehaviour
    {
        public static PassiveUpgradeManager Instance;

        [Header("Events")]
        public UnityEvent<PassiveUpgradeData> OnUpgradeApplied;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private Dictionary<string, int> appliedUpgrades = new Dictionary<string, int>();
        private Dictionary<string, List<PassiveUpgradeData>> upgradeDataByType = new Dictionary<string, List<PassiveUpgradeData>>();

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
            }
        }

        public void ApplyUpgrade(PassiveUpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogError("[PassiveUpgradeManager] Cannot apply null upgrade!");
                return;
            }

            string upgradeId = upgrade.upgradeId;

            if (!appliedUpgrades.ContainsKey(upgradeId))
            {
                appliedUpgrades[upgradeId] = 0;
            }

            appliedUpgrades[upgradeId]++;
            int stackCount = appliedUpgrades[upgradeId];

            // Store the upgrade data reference for retroactive application
            if (!upgradeDataByType.ContainsKey(upgradeId))
            {
                upgradeDataByType[upgradeId] = new List<PassiveUpgradeData>();
            }
            upgradeDataByType[upgradeId].Add(upgrade);

            if (showDebugLogs)
            {
                Debug.Log($"[PassiveUpgradeManager] Applying upgrade: {upgrade.displayName} (Stack: {stackCount})");
            }

            // Get all upgradeable targets (both ActiveSkills and basic abilities)
            List<IUpgradeable> targets = GetUpgradeableTargets(upgrade.targetSkillId);

            if (targets.Count == 0)
            {
                Debug.LogWarning($"[PassiveUpgradeManager] No upgradeable targets found for: {upgrade.targetSkillId}");
                return;
            }

            // Apply upgrade to each target
            foreach (IUpgradeable target in targets)
            {
                target.ApplyPassiveUpgrade(upgrade);
            }

            OnUpgradeApplied?.Invoke(upgrade);
        }

        /// <summary>
        /// Gets all IUpgradeable components (both ActiveSkills and basic abilities)
        /// </summary>
        private List<IUpgradeable> GetUpgradeableTargets(string targetSkillId)
        {
            List<IUpgradeable> targets = new List<IUpgradeable>();

            // Get all MonoBehaviours that implement IUpgradeable (includes ActiveSkills and basic abilities)
            MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
            IUpgradeable[] allUpgradeables = allMonoBehaviours.OfType<IUpgradeable>().ToArray();

            if (string.IsNullOrEmpty(targetSkillId))
            {
                // Global upgrade - apply to all upgradeables
                targets.AddRange(allUpgradeables);
            }
            else
            {
                // Targeted upgrade - filter by upgradeableId
                foreach (IUpgradeable upgradeable in allUpgradeables)
                {
                    if (upgradeable.GetUpgradeableId() == targetSkillId)
                    {
                        targets.Add(upgradeable);
                    }
                }
            }

            return targets;
        }

        public int GetUpgradeStackCount(string upgradeId)
        {
            if (appliedUpgrades.TryGetValue(upgradeId, out int count))
            {
                return count;
            }
            return 0;
        }

        public Dictionary<string, int> GetAllAppliedUpgrades()
        {
            return new Dictionary<string, int>(appliedUpgrades);
        }

        public void ClearAllUpgrades()
        {
            appliedUpgrades.Clear();
            upgradeDataByType.Clear();
            Debug.Log("[PassiveUpgradeManager] All upgrades cleared");
        }

        /// <summary>
        /// Apply all previously acquired upgrades to a newly added target (e.g., newly unlocked active skill)
        /// </summary>
        public void ApplyAllUpgradesTo(IUpgradeable target)
        {
            if (target == null)
            {
                Debug.LogWarning("[PassiveUpgradeManager] Cannot apply upgrades to null target");
                return;
            }

            string targetId = target.GetUpgradeableId();
            int appliedCount = 0;

            foreach (KeyValuePair<string, List<PassiveUpgradeData>> kvp in upgradeDataByType)
            {
                List<PassiveUpgradeData> upgradeList = kvp.Value;

                foreach (PassiveUpgradeData upgrade in upgradeList)
                {
                    // Check if this upgrade applies to this target (global or targeted)
                    bool isGlobal = string.IsNullOrEmpty(upgrade.targetSkillId);
                    bool isTargeted = upgrade.targetSkillId == targetId;

                    if (isGlobal || isTargeted)
                    {
                        target.ApplyPassiveUpgrade(upgrade);
                        appliedCount++;
                    }
                }
            }

            if (showDebugLogs && appliedCount > 0)
            {
                Debug.Log($"[PassiveUpgradeManager] Applied {appliedCount} retroactive upgrades to {targetId}");
            }
        }
    }
}
