using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HandSurvivor.ActiveSkills;

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

            if (showDebugLogs)
            {
                Debug.Log($"[PassiveUpgradeManager] Applying upgrade: {upgrade.displayName} (Stack: {stackCount})");
            }

            switch (upgrade.type)
            {
                case PassiveType.CooldownReduction:
                    ApplyCooldownReduction(upgrade);
                    break;

                case PassiveType.DamageIncrease:
                    ApplyDamageIncrease(upgrade);
                    break;

                case PassiveType.SizeIncrease:
                    ApplySizeIncrease(upgrade);
                    break;

                default:
                    Debug.LogWarning($"[PassiveUpgradeManager] Unknown passive type: {upgrade.type}");
                    break;
            }

            OnUpgradeApplied?.Invoke(upgrade);
        }

        private void ApplyCooldownReduction(PassiveUpgradeData upgrade)
        {
            float reductionPercent = upgrade.value / 100f;

            List<ActiveSkillBase> targetSkills = GetTargetSkills(upgrade.targetSkillId);

            if (targetSkills.Count == 0)
            {
                Debug.LogWarning($"[PassiveUpgradeManager] No skills found for cooldown reduction: {upgrade.targetSkillId}");
                return;
            }

            foreach (ActiveSkillBase skill in targetSkills)
            {
                skill.ApplyCooldownMultiplier(reductionPercent);

                // Apply upgrade path level progression
                if (skill.UpgradePath != null)
                {
                    skill.UpgradePath.ApplyPassiveUpgrade(upgrade);
                }

                if (showDebugLogs)
                {
                    Debug.Log($"[PassiveUpgradeManager] Applied -{upgrade.value}% cooldown to {skill.Data.displayName}");
                }
            }
        }

        private void ApplyDamageIncrease(PassiveUpgradeData upgrade)
        {
            float increasePercent = upgrade.value / 100f;

            List<ActiveSkillBase> targetSkills = GetTargetSkills(upgrade.targetSkillId);

            if (targetSkills.Count == 0)
            {
                Debug.LogWarning($"[PassiveUpgradeManager] No skills found for damage increase: {upgrade.targetSkillId}");
                return;
            }

            foreach (ActiveSkillBase skill in targetSkills)
            {
                skill.ApplyDamageMultiplier(increasePercent);

                // Apply upgrade path level progression
                if (skill.UpgradePath != null)
                {
                    skill.UpgradePath.ApplyPassiveUpgrade(upgrade);
                }

                if (showDebugLogs)
                {
                    Debug.Log($"[PassiveUpgradeManager] Applied +{upgrade.value}% damage to {skill.Data.displayName}");
                }
            }
        }

        private void ApplySizeIncrease(PassiveUpgradeData upgrade)
        {
            float increasePercent = upgrade.value / 100f;

            List<ActiveSkillBase> targetSkills = GetTargetSkills(upgrade.targetSkillId);

            if (targetSkills.Count == 0)
            {
                Debug.LogWarning($"[PassiveUpgradeManager] No skills found for size increase: {upgrade.targetSkillId}");
                return;
            }

            foreach (ActiveSkillBase skill in targetSkills)
            {
                skill.ApplySizeMultiplier(increasePercent);

                // Apply upgrade path level progression
                if (skill.UpgradePath != null)
                {
                    skill.UpgradePath.ApplyPassiveUpgrade(upgrade);
                }

                if (showDebugLogs)
                {
                    Debug.Log($"[PassiveUpgradeManager] Applied +{upgrade.value}% size to {skill.Data.displayName}");
                }
            }
        }

        private List<ActiveSkillBase> GetTargetSkills(string targetSkillId)
        {
            List<ActiveSkillBase> targetSkills = new List<ActiveSkillBase>();

            if (ActiveSkillSlotManager.Instance == null)
            {
                Debug.LogError("[PassiveUpgradeManager] ActiveSkillSlotManager.Instance is null!");
                return targetSkills;
            }

            List<ActiveSkillBase> allSkills = ActiveSkillSlotManager.Instance.GetSlottedSkills();

            if (string.IsNullOrEmpty(targetSkillId))
            {
                return allSkills;
            }

            foreach (ActiveSkillBase skill in allSkills)
            {
                if (skill != null && skill.Data.activeSkillId == targetSkillId)
                {
                    targetSkills.Add(skill);
                }
            }

            return targetSkills;
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
            Debug.Log("[PassiveUpgradeManager] All upgrades cleared");
        }
    }
}
