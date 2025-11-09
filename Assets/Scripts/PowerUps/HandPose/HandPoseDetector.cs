using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Detects hand poses using Meta Quest hand tracking
    /// Works with OVRHand and OVRSkeleton components
    /// </summary>
    [RequireComponent(typeof(OVRHand))]
    public class HandPoseDetector : MonoBehaviour
    {
        [Header("Hand Reference")]
        [SerializeField] private OVRHand ovrHand;
        [SerializeField] private OVRSkeleton ovrSkeleton;

        [Header("Pose Configuration")]
        [SerializeField] private HandPoseType poseType = HandPoseType.FingerGun;
        [SerializeField] private HandPoseDefinition customPose;

        [Header("Detection Settings")]
        [SerializeField] private float detectionThreshold = 0.8f;
        [Tooltip("How long pose must be held before triggering (seconds)")]
        [SerializeField] private float holdDuration = 0.3f;
        [Tooltip("Check pose every N frames (1 = every frame, 2 = every other frame)")]
        [SerializeField] private int checkInterval = 2;

        [Header("Events")]
        public UnityEvent OnPoseDetected;
        public UnityEvent OnPoseLost;

        private HandPoseDefinition activePose;
        private bool isPoseDetected = false;
        private float poseHoldTime = 0f;
        private int frameCounter = 0;

        public bool IsPoseActive => isPoseDetected;

        private void Awake()
        {
            if (ovrHand == null)
            {
                ovrHand = GetComponent<OVRHand>();
            }

            if (ovrSkeleton == null)
            {
                ovrSkeleton = GetComponent<OVRSkeleton>();
            }

            // Set active pose based on type
            activePose = GetPoseDefinition(poseType);
        }

        private void Update()
        {
            // Skip frames for performance
            frameCounter++;
            if (frameCounter % checkInterval != 0)
            {
                return;
            }

            // Only check if hand tracking is active
            if (!ovrHand.IsTracked || !ovrHand.IsDataValid)
            {
                if (isPoseDetected)
                {
                    OnPoseLost?.Invoke();
                    isPoseDetected = false;
                    poseHoldTime = 0f;
                }
                return;
            }

            bool poseMatches = CheckPose();

            if (poseMatches)
            {
                poseHoldTime += Time.deltaTime * checkInterval;

                if (poseHoldTime >= holdDuration && !isPoseDetected)
                {
                    isPoseDetected = true;
                    OnPoseDetected?.Invoke();
                    Debug.Log($"[HandPoseDetector] Pose detected: {poseType}");
                }
            }
            else
            {
                if (isPoseDetected)
                {
                    OnPoseLost?.Invoke();
                    Debug.Log($"[HandPoseDetector] Pose lost: {poseType}");
                }

                isPoseDetected = false;
                poseHoldTime = 0f;
            }
        }

        /// <summary>
        /// Check if current hand pose matches the target pose
        /// </summary>
        private bool CheckPose()
        {
            if (activePose == null)
            {
                return false;
            }

            // Check each finger against the pose definition
            bool thumbMatch = CheckFinger(OVRHand.HandFinger.Thumb, activePose.thumb, activePose.curlTolerance);
            bool indexMatch = CheckFinger(OVRHand.HandFinger.Index, activePose.index, activePose.curlTolerance);
            bool middleMatch = CheckFinger(OVRHand.HandFinger.Middle, activePose.middle, activePose.curlTolerance);
            bool ringMatch = CheckFinger(OVRHand.HandFinger.Ring, activePose.ring, activePose.curlTolerance);
            bool pinkyMatch = CheckFinger(OVRHand.HandFinger.Pinky, activePose.pinky, activePose.curlTolerance);

            return thumbMatch && indexMatch && middleMatch && ringMatch && pinkyMatch;
        }

        /// <summary>
        /// Check if a specific finger matches the required state
        /// </summary>
        private bool CheckFinger(OVRHand.HandFinger finger, FingerState requiredState, float tolerance)
        {
            if (requiredState == FingerState.Any)
            {
                return true;
            }

            float pinchStrength = ovrHand.GetFingerPinchStrength(finger);

            // Pinch strength is 0 when extended, 1 when curled
            if (requiredState == FingerState.Extended)
            {
                return pinchStrength < tolerance;
            }
            else // FingerState.Curled
            {
                return pinchStrength > (1f - tolerance);
            }
        }

        /// <summary>
        /// Get pose definition for a given pose type
        /// </summary>
        private HandPoseDefinition GetPoseDefinition(HandPoseType type)
        {
            switch (type)
            {
                case HandPoseType.FingerGun:
                    return HandPoseDefinition.FingerGun;
                case HandPoseType.OpenPalm:
                    return HandPoseDefinition.OpenPalm;
                case HandPoseType.Fist:
                    return HandPoseDefinition.Fist;
                case HandPoseType.TwoFingers:
                    return HandPoseDefinition.TwoFingers;
                case HandPoseType.Custom:
                    return customPose;
                default:
                    return HandPoseDefinition.FingerGun;
            }
        }

        /// <summary>
        /// Change the pose being detected at runtime
        /// </summary>
        public void SetPoseType(HandPoseType newPoseType)
        {
            poseType = newPoseType;
            activePose = GetPoseDefinition(newPoseType);
            isPoseDetected = false;
            poseHoldTime = 0f;
        }

        /// <summary>
        /// Set a custom pose definition
        /// </summary>
        public void SetCustomPose(HandPoseDefinition pose)
        {
            customPose = pose;
            if (poseType == HandPoseType.Custom)
            {
                activePose = pose;
                isPoseDetected = false;
                poseHoldTime = 0f;
            }
        }
    }
}
