using UnityEngine;
using UnityEngine.Animations;

namespace HandSurvivor
{
    /// <summary>
    /// Dynamically constrains this GameObject to a Transform stored in a TransformReference ScriptableObject using ParentConstraint
    /// </summary>
    [RequireComponent(typeof(ParentConstraint))]
    public class TransformConstraint : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Transform Reference")]
        [SerializeField] private TransformReference targetTransformReference;

        [Header("Constraint Settings")]
        [SerializeField] private bool applyOnStart = true;

        [Header("Wait for Reference")]
        [SerializeField] private bool waitForReference = true;
        [SerializeField] private float checkInterval = 0.1f;
        [SerializeField] private float timeout = 5f;

        [Header("Constraint Axes")]
        [SerializeField] private bool constrainPosition = false;
        [SerializeField] private bool constrainRotation = true;

        [Header("Optional Offset")]
        [SerializeField] private Vector3 positionOffset = Vector3.zero;
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;

        private ParentConstraint parentConstraint;
        private Transform cachedTargetTransform;
        private bool constraintApplied = false;

        private void Awake()
        {
            parentConstraint = GetComponent<ParentConstraint>();
            if (parentConstraint == null)
            {
                parentConstraint = gameObject.AddComponent<ParentConstraint>();
            }
        }

        private void Start()
        {
            if (applyOnStart)
            {
                if (waitForReference)
                {
                    StartCoroutine(WaitAndApplyConstraint());
                }
                else
                {
                    ApplyConstraint();
                }
            }
        }

        private System.Collections.IEnumerator WaitAndApplyConstraint()
        {
            float elapsedTime = 0f;

            while (targetTransformReference == null || targetTransformReference.Value == null)
            {
                if (elapsedTime >= timeout)
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                        Debug.LogWarning($"[TransformConstraint] Timeout waiting for TransformReference on {gameObject.name}");
                    yield break;
                }

                yield return new WaitForSeconds(checkInterval);
                elapsedTime += checkInterval;
            }

            ApplyConstraint();
        }

        /// <summary>
        /// Apply the parent constraint using the Transform from the ScriptableObject
        /// </summary>
        public void ApplyConstraint()
        {
            if (targetTransformReference == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning($"[TransformConstraint] No TransformReference assigned on {gameObject.name}");
                return;
            }

            if (targetTransformReference.Value == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning($"[TransformConstraint] TransformReference '{targetTransformReference.name}' has no Transform set");
                return;
            }

            cachedTargetTransform = targetTransformReference.Value;

            // Deactivate constraint while configuring
            parentConstraint.constraintActive = false;

            // Clear existing sources
            while (parentConstraint.sourceCount > 0)
            {
                parentConstraint.RemoveSource(0);
            }

            // Configure constraint axes
            if (constrainPosition)
            {
                parentConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            }
            else
            {
                parentConstraint.translationAxis = Axis.None;
            }

            if (constrainRotation)
            {
                parentConstraint.rotationAxis = Axis.X | Axis.Y | Axis.Z;
            }
            else
            {
                parentConstraint.rotationAxis = Axis.None;
            }

            // Add new constraint source
            ConstraintSource constraintSource = new ConstraintSource
            {
                sourceTransform = cachedTargetTransform,
                weight = 1f
            };

            int sourceIndex = parentConstraint.AddSource(constraintSource);

            // Set offsets to maintain current world position/rotation
            Vector3 currentPosition = transform.position;
            Quaternion currentRotation = transform.rotation;

            // Calculate offset needed to maintain current position
            Vector3 translationOffset = cachedTargetTransform.InverseTransformPoint(currentPosition) + positionOffset;
            Vector3 rotationOffset = (Quaternion.Inverse(cachedTargetTransform.rotation) * currentRotation).eulerAngles + this.rotationOffset;

            parentConstraint.SetTranslationOffset(sourceIndex, translationOffset);
            parentConstraint.SetRotationOffset(sourceIndex, rotationOffset);

            // Activate and lock constraint
            parentConstraint.constraintActive = true;
            parentConstraint.locked = true;

            constraintApplied = true;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[TransformConstraint] '{gameObject.name}' constrained to '{cachedTargetTransform.name}'");
        }

        /// <summary>
        /// Remove the parent constraint
        /// </summary>
        public void RemoveConstraint()
        {
            parentConstraint.constraintActive = false;

            while (parentConstraint.sourceCount > 0)
            {
                parentConstraint.RemoveSource(0);
            }

            cachedTargetTransform = null;
            constraintApplied = false;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[TransformConstraint] '{gameObject.name}' constraint removed");
        }

        /// <summary>
        /// Set a new TransformReference at runtime
        /// </summary>
        public void SetTransformReference(TransformReference newReference)
        {
            targetTransformReference = newReference;
            ApplyConstraint();
        }

        [ContextMenu("Apply Constraint")]
        private void ApplyConstraintContextMenu()
        {
            ApplyConstraint();
        }

        [ContextMenu("Remove Constraint")]
        private void RemoveConstraintContextMenu()
        {
            RemoveConstraint();
        }
    }
}
