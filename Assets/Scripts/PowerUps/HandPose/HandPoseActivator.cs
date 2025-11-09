using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Implements IPowerUpActivator using hand pose detection
    /// Integrates HandPoseDetector with the power-up system
    /// </summary>
    public class HandPoseActivator : MonoBehaviour, IPowerUpActivator
    {
        [Header("Hand Selection")]
        [SerializeField] private bool useMainHand = false;
        [Tooltip("If true, uses MainHand/OffHand from HandSelectionManager. If false, uses specified hand.")]
        [SerializeField] private bool autoDetectHand = true;
        [SerializeField] private HandPreference.HandType manualHandType = HandPreference.HandType.Right;

        [Header("Pose Detection")]
        [SerializeField] private HandPoseType poseType = HandPoseType.FingerGun;
        [SerializeField] private HandPoseDefinition customPose;

        [Header("References (Auto-assigned if empty)")]
        [SerializeField] private HandPoseDetector poseDetector;
        [SerializeField] private GameObject handPoseDetectorPrefab;

        private UnityEvent onActivationTriggered = new UnityEvent();
        private UnityEvent onDeactivationTriggered = new UnityEvent();
        private bool isInitialized = false;

        public UnityEvent OnActivationTriggered => onActivationTriggered;
        public UnityEvent OnDeactivationTriggered => onDeactivationTriggered;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Determine which hand to use
            HandPreference.HandType targetHand = DetermineTargetHand();

            // Find or create hand pose detector
            if (poseDetector == null)
            {
                poseDetector = FindHandPoseDetector(targetHand);
            }

            if (poseDetector == null)
            {
                Debug.LogWarning($"[HandPoseActivator] No HandPoseDetector found for {targetHand} hand. Cannot initialize.");
                return;
            }

            // Configure pose type
            poseDetector.SetPoseType(poseType);
            if (poseType == HandPoseType.Custom && customPose != null)
            {
                poseDetector.SetCustomPose(customPose);
            }

            // Wire up events
            poseDetector.OnPoseDetected.AddListener(OnPoseDetectedHandler);
            poseDetector.OnPoseLost.AddListener(OnPoseLostHandler);

            isInitialized = true;
            Debug.Log($"[HandPoseActivator] Initialized with {targetHand} hand, pose: {poseType}");
        }

        public void Enable()
        {
            if (poseDetector != null)
            {
                poseDetector.enabled = true;
            }
        }

        public void Disable()
        {
            if (poseDetector != null)
            {
                poseDetector.enabled = false;
            }
        }

        public void Cleanup()
        {
            if (poseDetector != null)
            {
                poseDetector.OnPoseDetected.RemoveListener(OnPoseDetectedHandler);
                poseDetector.OnPoseLost.RemoveListener(OnPoseLostHandler);
            }

            isInitialized = false;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private HandPreference.HandType DetermineTargetHand()
        {
            if (!autoDetectHand)
            {
                return manualHandType;
            }

            // Use HandSelectionManager if available
            if (HandSelectionManager.Instance != null)
            {
                return useMainHand ? HandSelectionManager.GetMainHand() : HandSelectionManager.GetOffHand();
            }

            return manualHandType;
        }

        private HandPoseDetector FindHandPoseDetector(HandPreference.HandType handType)
        {
            // Find all OVRHand components in scene
            OVRHand[] hands = FindObjectsByType<OVRHand>(FindObjectsSortMode.None);

            foreach (OVRHand hand in hands)
            {
                // Check if this is the correct hand
                bool isRightHand = hand.HandType == OVRHand.Hand.HandRight;
                bool isTargetHand = (handType == HandPreference.HandType.Right && isRightHand) ||
                                   (handType == HandPreference.HandType.Left && !isRightHand);

                if (isTargetHand)
                {
                    // Check if HandPoseDetector already exists
                    HandPoseDetector detector = hand.GetComponent<HandPoseDetector>();
                    if (detector == null)
                    {
                        // Add HandPoseDetector component
                        detector = hand.gameObject.AddComponent<HandPoseDetector>();
                        Debug.Log($"[HandPoseActivator] Added HandPoseDetector to {handType} hand");
                    }

                    return detector;
                }
            }

            Debug.LogWarning($"[HandPoseActivator] Could not find OVRHand for {handType} hand in scene");
            return null;
        }

        private void OnPoseDetectedHandler()
        {
            onActivationTriggered?.Invoke();
        }

        private void OnPoseLostHandler()
        {
            onDeactivationTriggered?.Invoke();
        }
    }
}
