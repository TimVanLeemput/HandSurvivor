using UnityEngine;

namespace HandSurvivor
{
    /// <summary>
    /// Sets a Transform into a TransformReference ScriptableObject
    /// </summary>
    public class SetTransformReference : MonoBehaviour
    {
        [Header("Transform to Set")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private bool useSelfIfNull = true;

        [Header("ScriptableObject Reference")]
        [SerializeField] private TransformReference transformReference;

        [Header("Settings")]
        [SerializeField] private bool setOnStart = true;
        [SerializeField] private bool setOnEnable = false;

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
            if (transformReference != null && transformReference.Value == GetTargetTransform())
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
                Debug.LogWarning($"[SetTransformReference] No TransformReference assigned on {gameObject.name}");
                return;
            }

            Transform transformToSet = GetTargetTransform();

            if (transformToSet == null)
            {
                Debug.LogWarning($"[SetTransformReference] No Transform to set on {gameObject.name}");
                return;
            }

            transformReference.Value = transformToSet;
            Debug.Log($"[SetTransformReference] Set '{transformToSet.name}' to TransformReference '{transformReference.name}'");
        }

        /// <summary>
        /// Clear the Transform reference in the ScriptableObject
        /// </summary>
        public void ClearReference()
        {
            if (transformReference != null)
            {
                transformReference.Clear();
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
