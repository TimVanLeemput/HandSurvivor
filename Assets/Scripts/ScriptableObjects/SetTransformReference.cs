using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor
{
    /// <summary>
    /// Sets Transform references into a TransformReference ScriptableObject.
    /// Supports single transform or multiple fixed spawn slots (manually positioned in scene).
    /// </summary>
    public class SetTransformReference : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Transform to Set")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private bool useSelfIfNull = true;

        [Header("ScriptableObject Reference")]
        [SerializeField] private TransformReference transformReference;

        [Header("Settings")]
        [SerializeField] private bool setOnStart = true;
        [SerializeField] private bool setOnEnable = false;

        [Header("Multi-Slot System")]
        [Tooltip("Enable to use multiple spawn slots")]
        [SerializeField] private bool enableMultiSlot = false;

        [Tooltip("Manual spawn slot transforms (create 10 child GameObjects and assign them here)")]
        [SerializeField] private List<Transform> spawnSlots = new List<Transform>();

        private void Start()
        {
            if (setOnStart)
            {
                SetReference();
            }
        }

        private void OnEnable()
        {
            if (setOnEnable)
            {
                SetReference();
            }
        }

        private void OnDisable()
        {
            // Optionally clear the reference when this object is disabled
            if (transformReference != null)
            {
                transformReference.Clear();
            }
        }

        /// <summary>
        /// Set the Transform reference in the ScriptableObject
        /// </summary>
        public void SetReference()
        {
            if (transformReference == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning($"[SetTransformReference] No TransformReference assigned on {gameObject.name}");
                return;
            }

            // Multi-slot system
            if (enableMultiSlot && spawnSlots != null && spawnSlots.Count > 0)
            {
                transformReference.SetSpawnSlots(spawnSlots);
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[SetTransformReference] Set {spawnSlots.Count} spawn slots to TransformReference '{transformReference.name}'");
            }
            else
            {
                // Standard single transform
                Transform transformToSet = GetTargetTransform();
                if (transformToSet == null)
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.LogWarning($"[SetTransformReference] No Transform to set on {gameObject.name}");
                    return;
                }

                transformReference.Value = transformToSet;
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[SetTransformReference] Set '{transformToSet.name}' to TransformReference '{transformReference.name}'");
            }
        }

        /// <summary>
        /// Clear the Transform reference in the ScriptableObject
        /// </summary>
        public void ClearReference()
        {
            if (transformReference != null)
            {
                transformReference.Clear();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[SetTransformReference] Cleared TransformReference '{transformReference.name}'");
            }
        }

        private Transform GetTargetTransform()
        {
            if (targetTransform != null)
            {
                return targetTransform;
            }

            if (useSelfIfNull)
            {
                return transform;
            }

            return null;
        }

        [ContextMenu("Set Reference Now")]
        private void SetReferenceContextMenu()
        {
            SetReference();
        }

        [ContextMenu("Clear Reference Now")]
        private void ClearReferenceContextMenu()
        {
            ClearReference();
        }
    }
}
