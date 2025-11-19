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
        [System.NonSerialized] private bool randomizeSpawn = false;

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
        /// Get a random spawn slot from available slots within maxSlots range.
        /// Falls back to main value if slots not configured.
        /// </summary>
        public Transform GetRandomSpawnSlot(int maxSlots)
        {
            // If spawn slots exist, pick random one
            if (spawnSlots != null && spawnSlots.Count > 0)
            {
                int actualMax = Mathf.Min(maxSlots, spawnSlots.Count);
                int randomIndex = Random.Range(0, actualMax);
                return spawnSlots[randomIndex];
            }

            // Fallback to main value if no slots configured
            return runtimeValue;
        }

        /// <summary>
        /// Get next spawn slot based on randomize setting (configured via SetTransformReference).
        /// Uses random selection if randomizeSpawn is enabled, otherwise returns first slot (index 0).
        /// Falls back to main value if slots not configured.
        /// </summary>
        public Transform GetNextSpawnSlot(int maxSlots)
        {
            if (randomizeSpawn)
            {
                return GetRandomSpawnSlot(maxSlots);
            }
            else
            {
                return GetSpawnSlot(slotIndex: 0, maxSlots);
            }
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
        public void SetSpawnSlots(List<Transform> slots, bool randomize = false)
        {
            if (spawnSlots == null)
                spawnSlots = new List<Transform>();

            spawnSlots.Clear();
            spawnSlots.AddRange(slots);
            randomizeSpawn = randomize;
        }

        /// <summary>
        /// Clear all references (useful on scene unload)
        /// </summary>
        public void Clear()
        {
            runtimeValue = null;
            spawnSlots?.Clear();
            randomizeSpawn = false;
        }

        private void OnDisable()
        {
            // Clear reference when exiting play mode to avoid dangling references
            Clear();
        }
    }
}
