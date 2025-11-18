using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Skills;

namespace HandSurvivor.Core.Passive
{
    public enum PassiveType
    {
        CooldownReduction,
        DamageIncrease,
        SizeIncrease,
        RangeIncrease,
        ChargesIncrease
    }

    [CreateAssetMenu(fileName = "PassiveUpgrade_", menuName = "HandSurvivor/Passive Upgrade Data", order = 1)]
    public class PassiveUpgradeData : ScriptableObject
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Identity")]
        [Tooltip("Unique identifier for this passive upgrade")]
        public string upgradeId;

        [Tooltip("Display name for UI")]
        public string displayName;

        [Tooltip("Description for UI (use {value} placeholder for value)")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Icon for UI display")]
        public Sprite passiveImage;

        [Header("Upgrade Properties")]
        [Tooltip("Type of upgrade")]
        public PassiveType type = PassiveType.DamageIncrease;

        [Tooltip("Upgrade value (e.g., 10 = +10% or -10%)")]
        public float value = 10f;

        [Header("Targeting")]
        [Tooltip("Target active skills (leave empty for global upgrade). Supports multiple targets.")]
        public List<ActiveSkillData> targetActiveSkills = new List<ActiveSkillData>();

        [Tooltip("Enable to target XPGrabber (range upgrades)")]
        public bool targetXPGrabber = false;

        [Tooltip("Enable to apply globally to all upgradeable components")]
        public bool applyGlobally = false;

        [Tooltip("Should trigger OnMaxPassiveReached event when hitting max upgrade (for Cooldown/Size only)")]
        public bool triggersMaxEvent = false;

        public string GetFormattedDescription()
        {
            return description.Replace("{value}", value.ToString("F0"));
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(upgradeId))
            {
                upgradeId = name;
            }

            // Validate targeting configuration
            bool hasActiveSkillTargets = targetActiveSkills != null && targetActiveSkills.Count > 0;
            int targetCount = (hasActiveSkillTargets ? 1 : 0) + (targetXPGrabber ? 1 : 0) + (applyGlobally ? 1 : 0);

            if (targetCount == 0)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning($"[PassiveUpgradeData] '{displayName}' has no targets configured. Set applyGlobally, targetActiveSkills, or targetXPGrabber.");
            }
            else if (targetCount > 1 && applyGlobally)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning($"[PassiveUpgradeData] '{displayName}' has applyGlobally=true but also has specific targets. Global will override specific targets.");
            }

            if (value < 0f)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning($"[PassiveUpgradeData] '{displayName}' has negative value. Ensure this is intentional.");
            }
        }
    }
}
