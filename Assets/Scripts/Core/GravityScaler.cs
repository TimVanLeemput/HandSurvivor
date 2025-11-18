using UnityEngine;
using HandSurvivor.Interfaces;

namespace HandSurvivor.Core
{
    /// <summary>
    /// Component that can be added to any GameObject with a Rigidbody to apply scaled gravity.
    /// Useful for throwable objects in reduced gravity environments.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GravityScaler : MonoBehaviour, IGravityScalable
    {
        [Header("Gravity Settings")]
        [Tooltip("Gravity multiplier. Use 100 if world gravity is divided by 100 to get normal gravity feel.")]
        [SerializeField] private float gravityMultiplier = 100f;

        [Tooltip("Only apply scaled gravity when not being grabbed (requires OVRGrabbable)")]
        [SerializeField] private bool disableWhenGrabbed = true;

        public float GravityMultiplier => gravityMultiplier;

        private Rigidbody _rigidbody;
        private OVRGrabbable _grabbable;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _grabbable = GetComponent<OVRGrabbable>();
        }

        private void FixedUpdate()
        {
            ApplyScaledGravity();
        }

        public void ApplyScaledGravity()
        {
            if (_rigidbody == null)
                return;

            // Check if we should skip when grabbed
            if (disableWhenGrabbed && _grabbable != null && _grabbable.isGrabbed)
                return;

            // Apply additional gravity to compensate for reduced world gravity
            _rigidbody.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }
        
        public void SetDisableWhenGrabbed(bool value)
        {
            disableWhenGrabbed = value;
        }
        
        public void SetGravityMultiplier(float value)
        {
            gravityMultiplier = value;
        }
    }
}
