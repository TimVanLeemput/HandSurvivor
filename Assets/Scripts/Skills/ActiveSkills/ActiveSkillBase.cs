using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Abstract base class for all active skills
    /// Extend this to create new active skill types
    /// </summary>
    public abstract class ActiveSkillBase : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] protected ActiveSkillData data;

        // [Header("Activator")]
        // [SerializeField] protected MonoBehaviour activatorComponent;

        [Header("Events")]
        public UnityEvent OnPickup;
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
        public UnityEvent OnExpire;

        protected bool isActive = false;
        protected bool isOnCooldown = false;
        protected float activationTime = 0f;
        protected float cooldownEndTime = 0f;

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
                    Deactivate();
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
            PlayPickupEffects();

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

            if (data.ActiveSkillType == ActiveSkillType.OneTime && isActive)
                return false;

            if (data.ActiveSkillType == ActiveSkillType.Toggle && isActive)
                return false;

            return true;
        }

        /// <summary>
        /// Activate the active skill
        /// </summary>
        protected virtual void Activate()
        {
            isActive = true;
            activationTime = Time.time;

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

        protected virtual void PlayPickupEffects()
        {
            if (data.pickupVFX != null)
            {
                Instantiate(data.pickupVFX, transform.position, Quaternion.identity);
            }

            if (data.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(data.pickupSound, transform.position);
            }
        }

        protected virtual void PlayActivationEffects()
        {
            if (data.activationVFX != null)
            {
                Instantiate(data.activationVFX, transform.position, Quaternion.identity);
            }

            if (data.activationSound != null)
            {
                AudioSource.PlayClipAtPoint(data.activationSound, transform.position);
            }
        }

        public void ApplyCooldownMultiplier(float reduction)
        {
            cooldownMultiplier = Mathf.Max(0.1f, cooldownMultiplier - reduction);
            Debug.Log($"[{Data.displayName}] Cooldown multiplier: {cooldownMultiplier:F2}");
        }

        public void ApplyDamageMultiplier(float increase)
        {
            damageMultiplier += increase;
            Debug.Log($"[{Data.displayName}] Damage multiplier: {damageMultiplier:F2}");
        }

        public void ApplySizeMultiplier(float increase)
        {
            sizeMultiplier += increase;
            Debug.Log($"[{Data.displayName}] Size multiplier: {sizeMultiplier:F2}");
        }

        public float GetModifiedCooldown()
        {
            return data.cooldown * cooldownMultiplier;
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
    }
}
