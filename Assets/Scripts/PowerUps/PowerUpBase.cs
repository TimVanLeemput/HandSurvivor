using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Abstract base class for all power-ups
    /// Extend this to create new power-up types
    /// </summary>
    public abstract class PowerUpBase : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] protected PowerUpData data;

        [Header("Activator")]
        [SerializeField] protected MonoBehaviour activatorComponent;

        [Header("Events")]
        public UnityEvent OnPickup;
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
        public UnityEvent OnExpire;

        protected IPowerUpActivator activator;
        protected bool isActive = false;
        protected bool isOnCooldown = false;
        protected float activationTime = 0f;
        protected float cooldownEndTime = 0f;

        public PowerUpData Data => data;
        public bool IsActive => isActive;
        public bool IsOnCooldown => isOnCooldown;
        public float RemainingDuration => Mathf.Max(0f, (activationTime + data.duration) - Time.time);
        public float RemainingCooldown => Mathf.Max(0f, cooldownEndTime - Time.time);

        protected virtual void Awake()
        {
            // Get activator from component if provided
            if (activatorComponent != null && activatorComponent is IPowerUpActivator)
            {
                activator = activatorComponent as IPowerUpActivator;
            }
        }

        protected virtual void Start()
        {
            if (activator != null)
            {
                activator.Initialize();
                activator.OnActivationTriggered.AddListener(TryActivate);
                activator.OnDeactivationTriggered.AddListener(TryDeactivate);
            }
        }

        protected virtual void OnDestroy()
        {
            if (activator != null)
            {
                activator.OnActivationTriggered.RemoveListener(TryActivate);
                activator.OnDeactivationTriggered.RemoveListener(TryDeactivate);
                activator.Cleanup();
            }
        }

        protected virtual void Update()
        {
            // Update cooldown
            if (isOnCooldown && Time.time >= cooldownEndTime)
            {
                isOnCooldown = false;
            }

            // Update duration-based power-ups
            if (isActive && (data.powerUpType == PowerUpType.Duration || data.powerUpType == PowerUpType.Toggle))
            {
                if (Time.time >= activationTime + data.duration)
                {
                    Deactivate();
                    OnExpired();
                }
            }
        }

        /// <summary>
        /// Called when power-up is picked up from the world
        /// </summary>
        public virtual void Pickup()
        {
            OnPickup?.Invoke();
            PlayPickupEffects();

            // Add to inventory
            PowerUpInventory.Instance.AddPowerUp(this);
        }

        /// <summary>
        /// Attempt to activate the power-up
        /// </summary>
        public void TryActivate()
        {
            if (CanActivate())
            {
                Activate();
            }
        }

        /// <summary>
        /// Check if power-up can be activated
        /// </summary>
        public virtual bool CanActivate()
        {
            if (isOnCooldown)
                return false;

            if (data.powerUpType == PowerUpType.OneTime && isActive)
                return false;

            if (data.powerUpType == PowerUpType.Toggle && isActive)
                return false;

            return true;
        }

        /// <summary>
        /// Activate the power-up
        /// </summary>
        protected virtual void Activate()
        {
            isActive = true;
            activationTime = Time.time;

            if (activator != null)
            {
                activator.Enable();
            }

            OnActivate?.Invoke();
            PlayActivationEffects();
            OnActivated();
        }

        /// <summary>
        /// Attempt to deactivate the power-up (for toggle types)
        /// </summary>
        public void TryDeactivate()
        {
            if (isActive && data.powerUpType == PowerUpType.Toggle)
            {
                Deactivate();
            }
        }

        /// <summary>
        /// Deactivate the power-up
        /// </summary>
        protected virtual void Deactivate()
        {
            isActive = false;

            if (activator != null)
            {
                activator.Disable();
            }

            OnDeactivate?.Invoke();
            OnDeactivated();

            // Start cooldown
            if (data.cooldown > 0f)
            {
                isOnCooldown = true;
                cooldownEndTime = Time.time + data.cooldown;
            }

            // Remove from inventory if one-time use
            if (data.powerUpType == PowerUpType.OneTime)
            {
                PowerUpInventory.Instance.RemovePowerUp(this);
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Override this in derived classes for activation logic
        /// </summary>
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
    }
}
