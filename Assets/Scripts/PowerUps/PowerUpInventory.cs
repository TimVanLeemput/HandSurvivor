using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyBox;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Singleton manager for player's collected power-ups
    /// Manages inventory, activation, and querying of available abilities
    /// </summary>
    public class PowerUpInventory : Singleton<PowerUpInventory>
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxInventorySize = 10;

        [Header("Events")]
        public UnityEvent<PowerUpBase> OnPowerUpAdded;
        public UnityEvent<PowerUpBase> OnPowerUpRemoved;
        public UnityEvent OnInventoryFull;

        private List<PowerUpBase> powerUps = new List<PowerUpBase>();
        private Dictionary<string, PowerUpBase> powerUpById = new Dictionary<string, PowerUpBase>();

        public IReadOnlyList<PowerUpBase> PowerUps => powerUps.AsReadOnly();
        public int Count => powerUps.Count;
        public bool IsFull => powerUps.Count >= maxInventorySize;

        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Add a power-up to the inventory
        /// </summary>
        public bool AddPowerUp(PowerUpBase powerUp)
        {
            if (powerUp == null)
            {
                Debug.LogWarning("[PowerUpInventory] Attempted to add null power-up");
                return false;
            }

            if (IsFull)
            {
                Debug.LogWarning($"[PowerUpInventory] Inventory full! Cannot add {powerUp.Data.displayName}");
                OnInventoryFull?.Invoke();
                return false;
            }

            // Check if we already have this power-up ID
            if (powerUpById.ContainsKey(powerUp.Data.powerUpId))
            {
                Debug.LogWarning($"[PowerUpInventory] Already have power-up: {powerUp.Data.displayName}");
                return false;
            }

            powerUps.Add(powerUp);
            powerUpById[powerUp.Data.powerUpId] = powerUp;

            // Parent to inventory so it persists
            powerUp.transform.SetParent(transform);
            powerUp.gameObject.SetActive(true);

            OnPowerUpAdded?.Invoke(powerUp);
            Debug.Log($"[PowerUpInventory] Added power-up: {powerUp.Data.displayName} ({powerUps.Count}/{maxInventorySize})");

            return true;
        }

        /// <summary>
        /// Remove a power-up from the inventory
        /// </summary>
        public bool RemovePowerUp(PowerUpBase powerUp)
        {
            if (powerUp == null || !powerUps.Contains(powerUp))
            {
                return false;
            }

            powerUps.Remove(powerUp);
            powerUpById.Remove(powerUp.Data.powerUpId);

            OnPowerUpRemoved?.Invoke(powerUp);
            Debug.Log($"[PowerUpInventory] Removed power-up: {powerUp.Data.displayName} ({powerUps.Count}/{maxInventorySize})");

            return true;
        }

        /// <summary>
        /// Get a power-up by its ID
        /// </summary>
        public PowerUpBase GetPowerUp(string powerUpId)
        {
            powerUpById.TryGetValue(powerUpId, out PowerUpBase powerUp);
            return powerUp;
        }

        /// <summary>
        /// Get all power-ups of a specific type
        /// </summary>
        public List<T> GetPowerUpsOfType<T>() where T : PowerUpBase
        {
            List<T> results = new List<T>();
            foreach (PowerUpBase powerUp in powerUps)
            {
                if (powerUp is T typedPowerUp)
                {
                    results.Add(typedPowerUp);
                }
            }
            return results;
        }

        /// <summary>
        /// Check if inventory contains a specific power-up ID
        /// </summary>
        public bool HasPowerUp(string powerUpId)
        {
            return powerUpById.ContainsKey(powerUpId);
        }

        /// <summary>
        /// Clear all power-ups from inventory
        /// </summary>
        public void ClearInventory()
        {
            // Create copy to avoid modification during iteration
            List<PowerUpBase> powerUpsCopy = new List<PowerUpBase>(powerUps);

            foreach (PowerUpBase powerUp in powerUpsCopy)
            {
                RemovePowerUp(powerUp);
                Destroy(powerUp.gameObject);
            }

            powerUps.Clear();
            powerUpById.Clear();

            Debug.Log("[PowerUpInventory] Inventory cleared");
        }

        /// <summary>
        /// Get all active power-ups
        /// </summary>
        public List<PowerUpBase> GetActivePowerUps()
        {
            List<PowerUpBase> active = new List<PowerUpBase>();
            foreach (PowerUpBase powerUp in powerUps)
            {
                if (powerUp.IsActive)
                {
                    active.Add(powerUp);
                }
            }
            return active;
        }

        /// <summary>
        /// Get all power-ups on cooldown
        /// </summary>
        public List<PowerUpBase> GetCooldownPowerUps()
        {
            List<PowerUpBase> onCooldown = new List<PowerUpBase>();
            foreach (PowerUpBase powerUp in powerUps)
            {
                if (powerUp.IsOnCooldown)
                {
                    onCooldown.Add(powerUp);
                }
            }
            return onCooldown;
        }
    }
}
