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

        [Header("VFX/Audio")]
        [Tooltip("Particle effect on pickup")]
        public GameObject pickupVFX;

        [Tooltip("Particle effect on activation")]
        public GameObject activationVFX;

        [Tooltip("Audio clip on pickup")]
        public AudioClip pickupSound;

        [Tooltip("Audio clip on activation")]
        public AudioClip activationSound;
    }
}
