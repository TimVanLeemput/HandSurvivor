using MyBox;
using UnityEngine;
using UnityEngine.Events;
using HandSurvivor.Core.Passive;
using HandSurvivor.Upgrades;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Abstract base class for all active skills
    /// Extend this to create new active skill types
    /// </summary>
    public abstract class ActiveSkillBase : MonoBehaviour, IUpgradeable
    {
        [Header("Debug")]
        [SerializeField] protected bool showDebugLogs = true;

         [Header("Configuration")]
        [SerializeField] protected ActiveSkillData data;

        // [Header("Activator")]
        // [SerializeField] protected MonoBehaviour activatorComponent;

        [Header("Events")]
        public UnityEvent OnPickup;
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
        public UnityEvent OnExpire;
        public UnityEvent OnMaxPassiveReached;

        protected bool isActive = false;
        protected bool isOnCooldown = false;
        protected float activationTime = 0f;
        protected float cooldownEndTime = 0f;

        protected AudioSource activationAudioSource;
        protected GameObject audioObject;

        [Header("Passive System")]
        protected PassiveUpgradePath upgradePath;

        [Header("Passive Multipliers")]
        protected float cooldownMultiplier = 1f;
        protected float damageMultiplier = 1f;
        protected float sizeMultiplier = 1f;

        public ActiveSkillData Data => data;
        public bool IsActive => isActive;
        public bool IsOnCooldown => isOnCooldown;
        public float RemainingDuration => Mathf.Max(0f, (activationTime + GetModifiedDuration()) - Time.time);
        public float RemainingCooldown => Mathf.Max(0f, cooldownEndTime - Time.time);

        public float CooldownMultiplier => cooldownMultiplier;
        public float DamageMultiplier => damageMultiplier;
        public float SizeMultiplier => sizeMultiplier;
        public PassiveUpgradePath UpgradePath => upgradePath;

        protected virtual void Awake()
        {
            upgradePath = GetComponent<PassiveUpgradePath>();
        }
        
        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
            // Update cooldown
            if (isOnCooldown && Time.time >= cooldownEndTime)
            {
                isOnCooldown = false;
            }

            // Update duration-based active skills
            if (isActive && (data.ActiveSkillType == ActiveSkillType.Duration || data.ActiveSkillType == ActiveSkillType.Toggle))
            {
                if (Time.time >= activationTime + GetModifiedDuration())
                {
                    // Duration expired - deactivate visual state but don't restart cooldown
                    isActive = false;
                    OnDeactivate?.Invoke();
                    OnDeactivated();
                    OnExpired();
                }
            }
        }

        /// <summary>
        /// Called when active skill is picked up from the world
        /// </summary>
        public virtual void Pickup()
        {
            OnPickup?.Invoke();

            // Add to inventory
            ActiveSkillInventory.Instance.AddActiveSkill(this);
        }

        /// <summary>
        /// Attempt to activate the active skill
        /// </summary>
        public void TryActivate()
        {
            if (CanActivate())
            {
                Activate();
            }
        }

        /// <summary>
        /// Check if active skill can be activated
        /// </summary>
        public virtual bool CanActivate()
        {
            if (isOnCooldown)
                return false;

            // OneTime skills cannot be reactivated while active
            if (data.ActiveSkillType == ActiveSkillType.OneTime && isActive)
                return false;

            // Toggle skills cannot be reactivated while active (must deactivate first)
            if (data.ActiveSkillType == ActiveSkillType.Toggle && isActive)
                return false;

            // Duration skills CAN be reactivated even while active (allows overlapping/stacking)
            return true;
        }

        /// <summary>
        /// Activate the active skill
        /// </summary>
        protected virtual void Activate()
        {
            isActive = true;
            activationTime = Time.time;

            // Start cooldown immediately on activation
            if (data.cooldown > 0f)
            {
                isOnCooldown = true;
                cooldownEndTime = Time.time + GetModifiedCooldown();
            }

            OnActivate?.Invoke();
            PlayActivationEffects();
            OnActivated();
        }

        /// <summary>
        /// Attempt to deactivate the active skill (for toggle types)
        /// </summary>
        public void TryDeactivate()
        {
            if (isActive && data.ActiveSkillType == ActiveSkillType.Toggle)
            {
                Deactivate();
            }
        }

        /// <summary>
        /// Deactivate the active skill
        /// </summary>
        protected virtual void Deactivate()
        {
            isActive = false;

            OnDeactivate?.Invoke();
            OnDeactivated();

            // Start cooldown
            if (data.cooldown > 0f)
            {
                isOnCooldown = true;
                cooldownEndTime = Time.time + GetModifiedCooldown();
            }

            // Remove from inventory if one-time use
            if (data.ActiveSkillType == ActiveSkillType.OneTime)
            {
                ActiveSkillInventory.Instance.RemoveActiveSkill(this);
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Override this in derived classes for activation logic
        /// </summary>
        [ButtonMethod]
        protected abstract void OnActivated();

        /// <summary>
        /// Override this in derived classes for deactivation logic
        /// </summary>
        protected abstract void OnDeactivated();

        /// <summary>
        /// Called when duration expires (for Duration/Toggle types)
        /// </summary>
        protected virtual void OnExpired()
        {
            OnExpire?.Invoke();
        }


        protected virtual void PlayActivationEffects()
        {
            if (data.activationSound != null)
            {
                // Create audio GameObject on first use
                if (audioObject == null)
                {
                    audioObject = new GameObject($"{data.displayName}_ActivationAudio");
                    audioObject.transform.SetParent(transform);
                    activationAudioSource = audioObject.AddComponent<AudioSource>();
                }

                // Stop previous audio if still playing
                if (activationAudioSource.isPlaying)
                {
                    activationAudioSource.Stop();
                }

                // Configure and play
                audioObject.transform.position = transform.position;
                activationAudioSource.clip = data.activationSound;
                activationAudioSource.loop = data.loopActivationSound;
                activationAudioSource.volume = data.activationSoundVolume;
                activationAudioSource.spatialBlend = data.soundSpatialization;
                activationAudioSource.Play();
            }
        }

        public void ApplyCooldownMultiplier(float reduction)
        {
            float minClamp = data != null ? data.minCooldownMultiplier : 0.1f;
            float previousMultiplier = cooldownMultiplier;
            cooldownMultiplier = Mathf.Max(minClamp, cooldownMultiplier - reduction);

            // Check if we just hit the minimum cooldown for the first time
            if (previousMultiplier > minClamp && cooldownMultiplier <= minClamp)
            {
                OnMaxPassiveReached?.Invoke();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[{Data.displayName}] MAX COOLDOWN REACHED - Event fired!");
            }

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


                Debug.Log($"[{Data.displayName}] Cooldown multiplier: {cooldownMultiplier:F2}");
        }

        public void ApplyDamageMultiplier(float increase)
        {
            damageMultiplier += increase;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[{Data.displayName}] Damage multiplier: {damageMultiplier:F2}");
        }

        public void ApplySizeMultiplier(float increase)
        {
            sizeMultiplier += increase;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[{Data.displayName}] Size multiplier: {sizeMultiplier:F2}");
        }

        /// <summary>
        /// Manually trigger max passive reached event (for size or other non-clamped stats)
        /// </summary>
        public void TriggerMaxPassiveReached()
        {
            OnMaxPassiveReached?.Invoke();
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[{Data.displayName}] MAX PASSIVE REACHED - Event fired!");
        }

        public float GetModifiedCooldown()
        {
            float baseCooldown = data.cooldown * cooldownMultiplier;

            // Check if we've hit minimum cooldown and have a max upgraded repeat rate
            if (data.maxUpgradedRepeatRate > 0f && cooldownMultiplier <= data.minCooldownMultiplier)
            {
                return data.maxUpgradedRepeatRate;
            }

            return baseCooldown;
        }

        public float GetModifiedDuration()
        {
            return data.duration;
        }

        public float GetModifiedDamage(float baseDamage)
        {
            return baseDamage * damageMultiplier;
        }

        public float GetModifiedSize(float baseSize)
        {
            return baseSize * sizeMultiplier;
        }

        #region IUpgradeable Implementation

        public string GetUpgradeableId()
        {
            return data != null ? data.activeSkillId : string.Empty;
        }

        public virtual void ApplyPassiveUpgrade(PassiveUpgradeData upgrade)
        {
            // Route to existing multiplier methods
            switch (upgrade.type)
            {
                case PassiveType.CooldownReduction:
                    ApplyCooldownMultiplier(upgrade.value / 100f);
                    break;

                case PassiveType.DamageIncrease:
                    ApplyDamageMultiplier(upgrade.value / 100f);
                    break;

                case PassiveType.SizeIncrease:
                    ApplySizeMultiplier(upgrade.value / 100f);
                    break;

                case PassiveType.RangeIncrease:
                    // ActiveSkills don't typically have range, but can be extended in derived classes
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                        Debug.LogWarning($"[{Data.displayName}] RangeIncrease not supported for ActiveSkills by default");
                    break;
            }

            // Apply to passive upgrade path if it exists
            if (upgradePath != null)
            {
                upgradePath.ApplyPassiveUpgrade(upgrade);
            }
        }

        #endregion
    }
}
