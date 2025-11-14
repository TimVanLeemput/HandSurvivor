using UnityEngine;

namespace HandSurvivor
{
    /// <summary>
    /// ScriptableObject that holds a runtime Transform reference, shared across scenes
    /// </summary>
    [CreateAssetMenu(fileName = "New Transform Reference", menuName = "HandSurvivor/Transform Reference")]
    public class TransformReference : ScriptableObject
    {
        [System.NonSerialized] private Transform runtimeValue;

        public Transform Value
        {
            get => runtimeValue;
            set => runtimeValue = value;
        }

        /// <summary>
        /// Clear the reference (useful on scene unload)
        /// </summary>
        public void Clear()
        {
            runtimeValue = null;
        }

        private void OnDisable()
        {
            // Clear reference when exiting play mode to avoid dangling references
            Clear();
        }
    }
}
