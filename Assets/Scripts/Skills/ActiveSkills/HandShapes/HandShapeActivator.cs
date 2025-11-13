using UnityEngine;
using HandSurvivor.Utilities;

namespace HandSurvivor.ActiveSkills.HandShapes
{
    public class HandShapeActivator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool useOffHand = true;
        [SerializeField] private bool autoFindHand = true;

        [Header("References")]
        [SerializeField] private HandShapeDetector detector;

        private OVRHand targetHand;
        private OVRSkeleton targetSkeleton;

        public bool IsPoseActive => detector != null && detector.IsFingerGunActive;
        public HandShapeDetector Detector => detector;

        private void Start()
        {
            if (detector == null)
            {
                detector = GetComponent<HandShapeDetector>();

                if (detector == null)
                {
                    detector = gameObject.AddComponent<HandShapeDetector>();
                    Debug.Log("[HandShapeActivator] Created HandShapeDetector component");
                }
            }

            if (autoFindHand)
            {
                FindTargetHand();
            }
        }

        public void FindTargetHand()
        {
            TargetHandFinder.HandComponents handComponents = TargetHandFinder.FindHand(useOffHand);

            if (handComponents.IsValid)
            {
                targetHand = handComponents.Hand;
                targetSkeleton = handComponents.Skeleton;

                if (detector != null)
                {
                    detector.SetHand(targetHand, targetSkeleton);
                    Debug.Log($"[HandShapeActivator] Configured detector for {(useOffHand ? "off" : "main")} hand");
                }
            }
            else
            {
                Debug.LogWarning($"[HandShapeActivator] Could not find {(useOffHand ? "off" : "main")} hand!");
            }
        }

        public void SetTargetHand(OVRHand hand, OVRSkeleton skeleton)
        {
            targetHand = hand;
            targetSkeleton = skeleton;

            if (detector != null)
            {
                detector.SetHand(hand, skeleton);
            }
        }

        public void SetUseOffHand(bool offHand)
        {
            useOffHand = offHand;
            if (autoFindHand)
            {
                FindTargetHand();
            }
        }
    }
}
