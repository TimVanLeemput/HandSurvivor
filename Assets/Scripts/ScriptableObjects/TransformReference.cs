using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor
{
    /// <summary>
    /// ScriptableObject that holds runtime Transform references, shared across scenes.
    /// Supports single transform or multiple fixed spawn slot transforms.
    /// </summary>
    [CreateAssetMenu(fileName = "New Transform Reference", menuName = "HandSurvivor/Transform Reference")]
    public class TransformReference : ScriptableObject
    {
        [System.NonSerialized] private Transform runtimeValue;
        [System.NonSerialized] private List<Transform> spawnSlots = new List<Transform>();

        public Transform Value
        {
            get => runtimeValue;
            set => runtimeValue = value;
        }

        /// <summary>
        /// Get a spawn slot by index. Returns slots within maxSlots range only.
        /// Falls back to main value if slots not configured.
        /// </summary>
        public Transform GetSpawnSlot(int slotIndex, int maxSlots)
        {
            // If spawn slots exist, use them
            if (spawnSlots != null && spawnSlots.Count > 0)
            {
                // Clamp to available slots and maxSlots
                int actualMax = Mathf.Min(maxSlots, spawnSlots.Count);
                if (slotIndex >= 0 && slotIndex < actualMax)
                {
                    return spawnSlots[slotIndex];
                }
            }

            // Fallback to main value for all slots if no slots configured
            // This allows the system to work with single spawn point until slots are set up
            return runtimeValue;
        }

        /// <summary>
        /// Get total number of spawn slots available
        /// </summary>
        public int GetTotalSlotCount()
        {
            if (spawnSlots != null && spawnSlots.Count > 0)
                return spawnSlots.Count;

            return runtimeValue != null ? 1 : 0;
        }

        /// <summary>
        /// Set spawn slot transforms (manual slots from scene)
        /// </summary>
        public void SetSpawnSlots(List<Transform> slots)
        {
            if (spawnSlots == null)
                spawnSlots = new List<Transform>();

            spawnSlots.Clear();
            spawnSlots.AddRange(slots);
        }

        /// <summary>
        /// Clear all references (useful on scene unload)
        /// </summary>
        public void Clear()
        {
            runtimeValue = null;
            spawnSlots?.Clear();
        }

        private void OnDisable()
        {
            // Clear reference when exiting play mode to avoid dangling references
            Clear();
        }
    }
}
