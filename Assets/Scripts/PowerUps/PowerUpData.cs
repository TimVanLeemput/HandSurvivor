using UnityEngine;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// ScriptableObject configuration for power-ups
    /// Allows designers to create power-up variants without code changes
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUp_", menuName = "HandSurvivor/PowerUp Data", order = 0)]
    public class PowerUpData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this power-up")]
        public string powerUpId;

        [Tooltip("Display name for UI")]
        public string displayName;

        [Tooltip("Description for UI")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Icon for UI display")]
        public Sprite icon;

        [Header("Behavior")]
        [Tooltip("How this power-up behaves")]
        public PowerUpType powerUpType = PowerUpType.Duration;

        [Tooltip("Duration in seconds (for Duration and Toggle types)")]
        public float duration = 3f;

        [Tooltip("Cooldown in seconds before can be used again")]
        public float cooldown = 0f;

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
