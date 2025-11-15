using UnityEngine;
using UnityEngine.UI;

namespace HandSurvivor.Core.Passive
{
    public enum PassiveType
    {
        CooldownReduction,
        DamageIncrease,
        SizeIncrease,
        RangeIncrease
    }

    [CreateAssetMenu(fileName = "PassiveUpgrade_", menuName = "HandSurvivor/Passive Upgrade Data", order = 1)]
    public class PassiveUpgradeData : ScriptableObject
    {
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

        [Tooltip("Target active skill ID (leave empty for global upgrade)")]
        public string targetSkillId;

        [Tooltip("Upgrade value (e.g., 10 = +10% or -10%)")]
        public float value = 10f;

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

            if (value < 0f)
            {
                Debug.LogWarning($"[PassiveUpgradeData] '{displayName}' has negative value. Ensure this is intentional.");
            }
        }
    }
}
