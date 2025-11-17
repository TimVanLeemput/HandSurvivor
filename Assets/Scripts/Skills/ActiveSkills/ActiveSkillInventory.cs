using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using MyBox;
using HandSurvivor.Core.Passive;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Represents a stack of active skills of the same type
    /// </summary>
    [System.Serializable]
    public class ActiveSkillStack
    {
        public ActiveSkillBase activeSkillInstance;
        public int count;

        public ActiveSkillStack(ActiveSkillBase activeSkill, int initialCount = 1)
        {
            activeSkillInstance = activeSkill;
            count = initialCount;
        }
    }

    /// <summary>
    /// Singleton manager for player's collected active skills
    /// Manages inventory with stacking support - can collect multiple of the same type
    /// </summary>
    public class ActiveSkillInventory : Singleton<ActiveSkillInventory>
    {
        [Header("Events")]
        public UnityEvent<ActiveSkillBase, int> OnActiveSkillAdded;
        public UnityEvent<ActiveSkillBase, int> OnActiveSkillRemoved;
        public UnityEvent<ActiveSkillBase> OnActiveSkillTypeRemoved;

        [SerializeField, ReadOnly] private List<ActiveSkillStack> activeSkillStacks = new List<ActiveSkillStack>();
        private Dictionary<string, ActiveSkillStack> stackById = new Dictionary<string, ActiveSkillStack>();

        public IReadOnlyList<ActiveSkillStack> ActiveSkillStacks => activeSkillStacks.AsReadOnly();
        public int UniqueTypeCount => activeSkillStacks.Count;
        public int TotalCount => activeSkillStacks.Sum(stack => stack.count);

        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Add an active skill to the inventory (delegates to slot manager)
        /// </summary>
        /// <param name="activeSkill">The active skill to add</param>
        /// <param name="amount">Unused parameter (kept for backwards compatibility)</param>
        public bool AddActiveSkill(ActiveSkillBase activeSkill, int amount = 1)
        {
            if (activeSkill == null)
            {
                Debug.LogWarning("[ActiveSkillInventory] Attempted to add null active skill");
                return false;
            }

            if (ActiveSkillSlotManager.Instance == null)
            {
                Debug.LogError("[ActiveSkillInventory] ActiveSkillSlotManager.Instance is null!");
                return false;
            }

            if (!ActiveSkillSlotManager.Instance.CanAddSkill())
            {
                Debug.LogWarning($"[ActiveSkillInventory] Cannot add skill '{activeSkill.Data.displayName}' - all slots are full!");
                Destroy(activeSkill.gameObject);
                return false;
            }

            bool added = ActiveSkillSlotManager.Instance.TryAddSkill(activeSkill);

            if (added)
            {
                string activeSkillId = activeSkill.Data.activeSkillId;

                ActiveSkillStack newStack = new ActiveSkillStack(activeSkill, 1);
                activeSkillStacks.Add(newStack);
                stackById[activeSkillId] = newStack;

                // Apply all previously acquired passive upgrades retroactively
                ApplyRetroactivePassiveUpgrades(activeSkill);

                OnActiveSkillAdded?.Invoke(activeSkill, 1);
                Debug.Log($"[ActiveSkillInventory] Added active skill: {activeSkill.Data.displayName}");
            }

            return added;
        }

        /// <summary>
        /// Remove an active skill from the inventory (decrements stack)
        /// </summary>
        /// <param name="activeSkill">The active skill to remove</param>
        /// <param name="amount">How many to remove (default: 1)</param>
        public bool RemoveActiveSkill(ActiveSkillBase activeSkill, int amount = 1)
        {
            if (activeSkill == null)
            {
                return false;
            }

            return RemoveActiveSkillById(activeSkill.Data.activeSkillId, amount);
        }

        /// <summary>
        /// Remove an active skill by ID (decrements stack)
        /// </summary>
        /// <param name="activeSkillId">The active skill ID to remove</param>
        /// <param name="amount">How many to remove (default: 1)</param>
        public bool RemoveActiveSkillById(string activeSkillId, int amount = 1)
        {
            if (!stackById.ContainsKey(activeSkillId))
            {
                return false;
            }

            ActiveSkillStack stack = stackById[activeSkillId];
            stack.count -= amount;

            if (stack.count <= 0)
            {
                // Stack depleted, remove completely
                activeSkillStacks.Remove(stack);
                stackById.Remove(activeSkillId);

                OnActiveSkillTypeRemoved?.Invoke(stack.activeSkillInstance);
                Debug.Log($"[ActiveSkillInventory] Removed active skill type: {stack.activeSkillInstance.Data.displayName}");

                // Destroy the GameObject
                Destroy(stack.activeSkillInstance.gameObject);

                return true;
            }
            else
            {
                // Still have some left
                OnActiveSkillRemoved?.Invoke(stack.activeSkillInstance, stack.count);
                Debug.Log($"[ActiveSkillInventory] Decremented active skill: {stack.activeSkillInstance.Data.displayName} x{stack.count}");

                return true;
            }
        }

        /// <summary>
        /// Get an active skill by its ID
        /// </summary>
        public ActiveSkillBase GetActiveSkill(string activeSkillId)
        {
            if (stackById.TryGetValue(activeSkillId, out ActiveSkillStack stack))
            {
                return stack.activeSkillInstance;
            }
            return null;
        }

        /// <summary>
        /// Get the stack for an active skill by its ID
        /// </summary>
        public ActiveSkillStack GetActiveSkillStack(string activeSkillId)
        {
            stackById.TryGetValue(activeSkillId, out ActiveSkillStack stack);
            return stack;
        }

        /// <summary>
        /// Get the count of a specific active skill type
        /// </summary>
        public int GetActiveSkillCount(string activeSkillId)
        {
            if (stackById.TryGetValue(activeSkillId, out ActiveSkillStack stack))
            {
                return stack.count;
            }
            return 0;
        }

        /// <summary>
        /// Get the count of a specific active skill instance
        /// </summary>
        public int GetActiveSkillCount(ActiveSkillBase activeSkill)
        {
            if (activeSkill == null)
            {
                return 0;
            }
            return GetActiveSkillCount(activeSkill.Data.activeSkillId);
        }

        /// <summary>
        /// Get all active skills of a specific type
        /// </summary>
        public List<T> GetActiveSkillsOfType<T>() where T : ActiveSkillBase
        {
            List<T> results = new List<T>();
            foreach (ActiveSkillStack stack in activeSkillStacks)
            {
                if (stack.activeSkillInstance is T typedActiveSkill)
                {
                    results.Add(typedActiveSkill);
                }
            }
            return results;
        }

        /// <summary>
        /// Check if inventory contains a specific active skill ID
        /// </summary>
        public bool HasActiveSkill(string activeSkillId)
        {
            return stackById.ContainsKey(activeSkillId);
        }

        /// <summary>
        /// Get all owned active skill IDs
        /// </summary>
        public List<string> GetAllSkillIds()
        {
            return new List<string>(stackById.Keys);
        }

        /// <summary>
        /// Clear all active skills from inventory
        /// </summary>
        public void ClearInventory()
        {
            // Create copy to avoid modification during iteration
            List<ActiveSkillStack> stacksCopy = new List<ActiveSkillStack>(activeSkillStacks);

            foreach (ActiveSkillStack stack in stacksCopy)
            {
                if (stack.activeSkillInstance != null)
                {
                    Destroy(stack.activeSkillInstance.gameObject);
                }
            }

            activeSkillStacks.Clear();
            stackById.Clear();

            Debug.Log("[ActiveSkillInventory] Inventory cleared");
        }

        /// <summary>
        /// Get all active active skills
        /// </summary>
        public List<ActiveSkillBase> GetActiveActiveSkills()
        {
            List<ActiveSkillBase> active = new List<ActiveSkillBase>();
            foreach (ActiveSkillStack stack in activeSkillStacks)
            {
                if (stack.activeSkillInstance.IsActive)
                {
                    active.Add(stack.activeSkillInstance);
                }
            }
            return active;
        }

        /// <summary>
        /// Get all active skills on cooldown
        /// </summary>
        public List<ActiveSkillBase> GetCooldownActiveSkills()
        {
            List<ActiveSkillBase> onCooldown = new List<ActiveSkillBase>();
            foreach (ActiveSkillStack stack in activeSkillStacks)
            {
                if (stack.activeSkillInstance.IsOnCooldown)
                {
                    onCooldown.Add(stack.activeSkillInstance);
                }
            }
            return onCooldown;
        }

        /// <summary>
        /// Get all unique active skill instances (one per type)
        /// </summary>
        public List<ActiveSkillBase> GetAllUniqueActiveSkills()
        {
            List<ActiveSkillBase> activeSkills = new List<ActiveSkillBase>();
            foreach (ActiveSkillStack stack in activeSkillStacks)
            {
                activeSkills.Add(stack.activeSkillInstance);
            }
            return activeSkills;
        }

        /// <summary>
        /// Apply all previously acquired passive upgrades to a newly added active skill
        /// </summary>
        private void ApplyRetroactivePassiveUpgrades(ActiveSkillBase activeSkill)
        {
            if (PassiveUpgradeManager.Instance == null)
            {
                Debug.LogWarning("[ActiveSkillInventory] PassiveUpgradeManager.Instance is null - cannot apply retroactive upgrades");
                return;
            }

            PassiveUpgradeManager.Instance.ApplyAllUpgradesTo(activeSkill);
        }
    }
}
