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
            List<IUpgradeable> targets = GetUpgradeableTargets(upgrade);

            if (targets.Count == 0)
            {
                Debug.LogWarning($"[PassiveUpgradeManager] No upgradeable targets found for upgrade: {upgrade.displayName}");
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
        private List<IUpgradeable> GetUpgradeableTargets(PassiveUpgradeData upgrade)
        {
            List<IUpgradeable> targets = new List<IUpgradeable>();

            // Get all MonoBehaviours that implement IUpgradeable (includes ActiveSkills and basic abilities)
            MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
            IUpgradeable[] allUpgradeables = allMonoBehaviours.OfType<IUpgradeable>().ToArray();

            // Global upgrade
            if (upgrade.applyGlobally)
            {
                targets.AddRange(allUpgradeables);
                return targets;
            }

            // Target active skills via direct reference
            if (upgrade.targetActiveSkills != null && upgrade.targetActiveSkills.Count > 0)
            {
                foreach (IUpgradeable upgradeable in allUpgradeables)
                {
                    if (upgradeable is ActiveSkillBase activeSkill)
                    {
                        if (upgrade.targetActiveSkills.Contains(activeSkill.Data))
                        {
                            targets.Add(upgradeable);
                        }
                    }
                }
            }

            // Target XPGrabber
            if (upgrade.targetXPGrabber)
            {
                foreach (IUpgradeable upgradeable in allUpgradeables)
                {
                    if (upgradeable is XPGrabber)
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

            int appliedCount = 0;

            foreach (KeyValuePair<string, List<PassiveUpgradeData>> kvp in upgradeDataByType)
            {
                List<PassiveUpgradeData> upgradeList = kvp.Value;

                foreach (PassiveUpgradeData upgrade in upgradeList)
                {
                    bool shouldApply = false;

                    // Check global
                    if (upgrade.applyGlobally)
                    {
                        shouldApply = true;
                    }
                    // Check ActiveSkill targets
                    else if (target is ActiveSkillBase activeSkill)
                    {
                        if (upgrade.targetActiveSkills != null && upgrade.targetActiveSkills.Contains(activeSkill.Data))
                        {
                            shouldApply = true;
                        }
                    }
                    // Check XPGrabber target
                    else if (target is XPGrabber && upgrade.targetXPGrabber)
                    {
                        shouldApply = true;
                    }

                    if (shouldApply)
                    {
                        target.ApplyPassiveUpgrade(upgrade);
                        appliedCount++;
                    }
                }
            }

            if (showDebugLogs && appliedCount > 0)
            {
                Debug.Log($"[PassiveUpgradeManager] Applied {appliedCount} retroactive upgrades to {target.GetUpgradeableId()}");
            }
        }
    }
}
