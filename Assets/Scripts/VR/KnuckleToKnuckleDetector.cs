using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.VR
{
    /// <summary>
    /// Continuously detects when both hands' knuckles are pointing toward each other
    /// Assign the knuckle transforms manually in the inspector
    /// </summary>
    public class KnuckleToKnuckleDetector : MonoBehaviour
    {
        [Header("Hand Knuckle Transforms")]
        [SerializeField] private Transform leftKnuckle;
        [SerializeField] private Transform rightKnuckle;
        [SerializeField] private Transform leftWrist;
        [SerializeField] private Transform rightWrist;

        [Header("Tolerance Settings")]
        [SerializeField] [Range(0f, 90f)] private float angleTolerance = 30f;
        [SerializeField] private float maxDistance = 0.5f; // meters

        [Header("Detection Settings")]
        [SerializeField] private float detectionCooldown = 0.5f; // prevent rapid firing

        [Header("Events")]
        public UnityEvent OnKnucklesPointing;
        public UnityEvent OnKnucklesStopPointing;

        private bool wasPointing = false;
        private float lastDetectionTime = -999f;

        private void Update()
        {
            if (!AreTransformsAssigned())
                return;

            bool isPointing = AreKnucklesPointingAtEachOther();

            // Rising edge detection with cooldown
            if (isPointing && !wasPointing && Time.time - lastDetectionTime > detectionCooldown)
            {
                OnKnucklesPointing?.Invoke();
                lastDetectionTime = Time.time;
            }

            // Falling edge detection - only fires if we were previously pointing
            if (!isPointing && wasPointing)
            {
                OnKnucklesStopPointing?.Invoke();
            }

            wasPointing = isPointing;
        }

        private bool AreTransformsAssigned()
        {
            return leftKnuckle != null && rightKnuckle != null &&
                   leftWrist != null && rightWrist != null;
        }

        private bool AreKnucklesPointingAtEachOther()
        {
            // Get knuckle positions
            Vector3 leftKnucklePos = leftKnuckle.position;
            Vector3 rightKnucklePos = rightKnuckle.position;

            // Distance check
            float distance = Vector3.Distance(leftKnucklePos, rightKnucklePos);
            if (distance > maxDistance)
                return false;

            // Direction vectors from wrist to knuckle (palm/finger direction)
            Vector3 leftDirection = (leftKnucklePos - leftWrist.position).normalized;
            Vector3 rightDirection = (rightKnucklePos - rightWrist.position).normalized;

            // Direction from left to right knuckle
            Vector3 leftToRight = (rightKnucklePos - leftKnucklePos).normalized;
            Vector3 rightToLeft = -leftToRight;

            // Check if left knuckle direction points toward right knuckle
            float leftAngle = Vector3.Angle(leftDirection, leftToRight);

            // Check if right knuckle direction points toward left knuckle
            float rightAngle = Vector3.Angle(rightDirection, rightToLeft);

            return leftAngle < angleTolerance && rightAngle < angleTolerance;
        }

        public bool IsCurrentlyPointing()
        {
            return wasPointing;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !AreTransformsAssigned())
                return;

            Vector3 leftPos = leftKnuckle.position;
            Vector3 rightPos = rightKnuckle.position;
            Vector3 leftDirection = (leftPos - leftWrist.position).normalized;
            Vector3 rightDirection = (rightPos - rightWrist.position).normalized;

            // Draw knuckle positions
            Gizmos.color = wasPointing ? Color.green : Color.red;
            Gizmos.DrawWireSphere(leftPos, 0.02f);
            Gizmos.DrawWireSphere(rightPos, 0.02f);

            // Draw direction vectors
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(leftPos, leftDirection * 0.1f);
            Gizmos.DrawRay(rightPos, rightDirection * 0.1f);

            // Draw connection line
            Gizmos.color = wasPointing ? Color.green : Color.yellow;
            Gizmos.DrawLine(leftPos, rightPos);
        }
    }
}
