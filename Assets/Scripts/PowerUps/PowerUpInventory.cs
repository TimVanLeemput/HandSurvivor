using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using MyBox;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Represents a stack of power-ups of the same type
    /// </summary>
    [System.Serializable]
    public class PowerUpStack
    {
        public PowerUpBase powerUpInstance;
        public int count;

        public PowerUpStack(PowerUpBase powerUp, int initialCount = 1)
        {
            powerUpInstance = powerUp;
            count = initialCount;
        }
    }

    /// <summary>
    /// Singleton manager for player's collected power-ups
    /// Manages inventory with stacking support - can collect multiple of the same type
    /// </summary>
    public class PowerUpInventory : Singleton<PowerUpInventory>
    {
        [Header("Events")]
        public UnityEvent<PowerUpBase, int> OnPowerUpAdded; // PowerUpBase, new count
        public UnityEvent<PowerUpBase, int> OnPowerUpRemoved; // PowerUpBase, new count
        public UnityEvent<PowerUpBase> OnPowerUpTypeRemoved; // When stack reaches 0

        [SerializeField, ReadOnly] private List<PowerUpStack> powerUpStacks = new List<PowerUpStack>();
        private Dictionary<string, PowerUpStack> stackById = new Dictionary<string, PowerUpStack>();

        public IReadOnlyList<PowerUpStack> PowerUpStacks => powerUpStacks.AsReadOnly();
        public int UniqueTypeCount => powerUpStacks.Count;
        public int TotalCount => powerUpStacks.Sum(stack => stack.count);

        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Add a power-up to the inventory (supports stacking)
        /// </summary>
        /// <param name="powerUp">The power-up to add</param>
        /// <param name="amount">How many to add (default: 1)</param>
        public bool AddPowerUp(PowerUpBase powerUp, int amount = 1)
        {
            if (powerUp == null)
            {
                Debug.LogWarning("[PowerUpInventory] Attempted to add null power-up");
                return false;
            }

            if (amount <= 0)
            {
                Debug.LogWarning($"[PowerUpInventory] Invalid amount: {amount}");
                return false;
            }

            string powerUpId = powerUp.Data.powerUpId;

            // Check if we already have this power-up type
            if (stackById.ContainsKey(powerUpId))
            {
                // Stack it
                PowerUpStack existingStack = stackById[powerUpId];
                existingStack.count += amount;

                Debug.Log($"[PowerUpInventory] Stacked power-up: {powerUp.Data.displayName} x{existingStack.count}");

                // Destroy the new GameObject since we're just incrementing count
                Destroy(powerUp.gameObject);

                OnPowerUpAdded?.Invoke(existingStack.powerUpInstance, existingStack.count);
                return true;
            }
            else
            {
                // Create new stack
                PowerUpStack newStack = new PowerUpStack(powerUp, amount);
                powerUpStacks.Add(newStack);
                stackById[powerUpId] = newStack;

                // Parent to inventory so it persists
                powerUp.transform.SetParent(transform);
                powerUp.gameObject.SetActive(true);

                OnPowerUpAdded?.Invoke(powerUp, newStack.count);
                Debug.Log($"[PowerUpInventory] Added new power-up: {powerUp.Data.displayName} x{newStack.count}");

                return true;
            }
        }

        /// <summary>
        /// Remove a power-up from the inventory (decrements stack)
        /// </summary>
        /// <param name="powerUp">The power-up to remove</param>
        /// <param name="amount">How many to remove (default: 1)</param>
        public bool RemovePowerUp(PowerUpBase powerUp, int amount = 1)
        {
            if (powerUp == null)
            {
                return false;
            }

            return RemovePowerUpById(powerUp.Data.powerUpId, amount);
        }

        /// <summary>
        /// Remove a power-up by ID (decrements stack)
        /// </summary>
        /// <param name="powerUpId">The power-up ID to remove</param>
        /// <param name="amount">How many to remove (default: 1)</param>
        public bool RemovePowerUpById(string powerUpId, int amount = 1)
        {
            if (!stackById.ContainsKey(powerUpId))
            {
                return false;
            }

            PowerUpStack stack = stackById[powerUpId];
            stack.count -= amount;

            if (stack.count <= 0)
            {
                // Stack depleted, remove completely
                powerUpStacks.Remove(stack);
                stackById.Remove(powerUpId);

                OnPowerUpTypeRemoved?.Invoke(stack.powerUpInstance);
                Debug.Log($"[PowerUpInventory] Removed power-up type: {stack.powerUpInstance.Data.displayName}");

                // Destroy the GameObject
                Destroy(stack.powerUpInstance.gameObject);

                return true;
            }
            else
            {
                // Still have some left
                OnPowerUpRemoved?.Invoke(stack.powerUpInstance, stack.count);
                Debug.Log($"[PowerUpInventory] Decremented power-up: {stack.powerUpInstance.Data.displayName} x{stack.count}");

                return true;
            }
        }

        /// <summary>
        /// Get a power-up by its ID
        /// </summary>
        public PowerUpBase GetPowerUp(string powerUpId)
        {
            if (stackById.TryGetValue(powerUpId, out PowerUpStack stack))
            {
                return stack.powerUpInstance;
            }
            return null;
        }

        /// <summary>
        /// Get the stack for a power-up by its ID
        /// </summary>
        public PowerUpStack GetPowerUpStack(string powerUpId)
        {
            stackById.TryGetValue(powerUpId, out PowerUpStack stack);
            return stack;
        }

        /// <summary>
        /// Get the count of a specific power-up type
        /// </summary>
        public int GetPowerUpCount(string powerUpId)
        {
            if (stackById.TryGetValue(powerUpId, out PowerUpStack stack))
            {
                return stack.count;
            }
            return 0;
        }

        /// <summary>
        /// Get the count of a specific power-up instance
        /// </summary>
        public int GetPowerUpCount(PowerUpBase powerUp)
        {
            if (powerUp == null)
            {
                return 0;
            }
            return GetPowerUpCount(powerUp.Data.powerUpId);
        }

        /// <summary>
        /// Get all power-ups of a specific type
        /// </summary>
        public List<T> GetPowerUpsOfType<T>() where T : PowerUpBase
        {
            List<T> results = new List<T>();
            foreach (PowerUpStack stack in powerUpStacks)
            {
                if (stack.powerUpInstance is T typedPowerUp)
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
            return stackById.ContainsKey(powerUpId);
        }

        /// <summary>
        /// Clear all power-ups from inventory
        /// </summary>
        public void ClearInventory()
        {
            // Create copy to avoid modification during iteration
            List<PowerUpStack> stacksCopy = new List<PowerUpStack>(powerUpStacks);

            foreach (PowerUpStack stack in stacksCopy)
            {
                if (stack.powerUpInstance != null)
                {
                    Destroy(stack.powerUpInstance.gameObject);
                }
            }

            powerUpStacks.Clear();
            stackById.Clear();

            Debug.Log("[PowerUpInventory] Inventory cleared");
        }

        /// <summary>
        /// Get all active power-ups
        /// </summary>
        public List<PowerUpBase> GetActivePowerUps()
        {
            List<PowerUpBase> active = new List<PowerUpBase>();
            foreach (PowerUpStack stack in powerUpStacks)
            {
                if (stack.powerUpInstance.IsActive)
                {
                    active.Add(stack.powerUpInstance);
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
            foreach (PowerUpStack stack in powerUpStacks)
            {
                if (stack.powerUpInstance.IsOnCooldown)
                {
                    onCooldown.Add(stack.powerUpInstance);
                }
            }
            return onCooldown;
        }

        /// <summary>
        /// Get all unique power-up instances (one per type)
        /// </summary>
        public List<PowerUpBase> GetAllUniquePowerUps()
        {
            List<PowerUpBase> powerUps = new List<PowerUpBase>();
            foreach (PowerUpStack stack in powerUpStacks)
            {
                powerUps.Add(stack.powerUpInstance);
            }
            return powerUps;
        }
    }
}
