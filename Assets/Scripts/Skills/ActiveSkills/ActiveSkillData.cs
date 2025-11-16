using UnityEngine;
using UnityEngine.UI;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// ScriptableObject configuration for active skills
    /// Allows designers to create active skill variants without code changes
    /// </summary>
    [CreateAssetMenu(fileName = "ActiveSkill_", menuName = "HandSurvivor/Active Skill Data", order = 0)]
    public class ActiveSkillData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this active skill")]
        public string activeSkillId;

        [Tooltip("Display name for UI")]
        public string displayName;

        [Tooltip("Description for UI")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Icon for UI display")]
        public Sprite skillImage;

        [Header("Behavior")]
        [Tooltip("How this active skill behaves")]
        public ActiveSkillType ActiveSkillType = ActiveSkillType.Duration;

        [Tooltip("Duration in seconds (for Duration and Toggle types)")]
        public float duration = 3f;

        [Tooltip("Cooldown in seconds before can be used again")]
        public float cooldown = 0f;

        [Header("Stats")]
        [Tooltip("Raw damage value (usage depends on skill type)")]
        public int damage = 100;

        [Tooltip("Minimum cooldown multiplier (0.01 = 1% of base cooldown, prevents infinite spam)")]
        [Range(0.01f, 1f)]
        public float minCooldownMultiplier = 0.1f;

        [Tooltip("Repeat rate when min cooldown is hit (seconds between activations, 0 = use min cooldown)")]
        public float maxUpgradedRepeatRate = 0f;

        [Tooltip("Audio clip on activation")]
        public AudioClip activationSound;

        [Tooltip("Should the activation sound loop during skill duration")]
        public bool loopActivationSound = true;
        
        [Header("Comments")]
        [TextArea]
        public string comments;    }
}
