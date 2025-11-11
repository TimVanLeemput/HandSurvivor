using UnityEngine;

namespace HandSurvivor.Utilities
{
    /// <summary>
    /// Static utility for finding and retrieving OVRHand and OVRSkeleton components
    /// based on HandSelectionManager preferences
    /// </summary>
    public static class TargetHandFinder
    {
        public struct HandComponents
        {
            public OVRHand Hand;
            public OVRSkeleton Skeleton;
            public HandType HandType;
            public bool IsValid => Hand != null && Skeleton != null;
        }

        /// <summary>
        /// Finds the main hand (dominant hand)
        /// </summary>
        public static HandComponents FindMainHand()
        {
            return FindHand(false);
        }

        /// <summary>
        /// Finds the off hand (non-dominant hand)
        /// </summary>
        public static HandComponents FindOffHand()
        {
            return FindHand(true);
        }

        /// <summary>
        /// Finds either main or off hand based on parameter
        /// </summary>
        /// <param name="useOffHand">True for off-hand, false for main hand</param>
        public static HandComponents FindHand(bool useOffHand)
        {
            HandType targetHandType = useOffHand
                ? HandSelectionManager.GetOffHand()
                : HandSelectionManager.GetMainHand();

            Debug.Log($"[TargetHandFinder] Looking for {(useOffHand ? "OFF" : "MAIN")} hand, which is: {targetHandType}");

            OVRHand[] hands = Object.FindObjectsByType<OVRHand>(FindObjectsSortMode.None);
            Debug.Log($"[TargetHandFinder] Found {hands.Length} OVRHand(s) in scene");

            foreach (OVRHand hand in hands)
            {
                OVRSkeleton skeleton = hand.GetComponent<OVRSkeleton>();
                if (skeleton == null)
                {
                    Debug.LogWarning($"[TargetHandFinder] OVRHand found but no OVRSkeleton attached!");
                    continue;
                }

                OVRSkeleton.SkeletonType skelType = skeleton.GetSkeletonType();
                bool isRightHand = (skelType == OVRSkeleton.SkeletonType.HandRight ||
                                   skelType == OVRSkeleton.SkeletonType.XRHandRight);
                bool isLeftHand = (skelType == OVRSkeleton.SkeletonType.HandLeft ||
                                  skelType == OVRSkeleton.SkeletonType.XRHandLeft);

                Debug.Log($"[TargetHandFinder] Checking hand - SkeletonType: {skelType}, IsRight: {isRightHand}, IsLeft: {isLeftHand}");

                bool isMatch = (targetHandType == HandType.Right && isRightHand) ||
                              (targetHandType == HandType.Left && isLeftHand);

                if (isMatch)
                {
                    Debug.Log($"[TargetHandFinder] ✓ MATCHED! Using {targetHandType} hand");
                    return new HandComponents
                    {
                        Hand = hand,
                        Skeleton = skeleton,
                        HandType = targetHandType
                    };
                }
            }

            Debug.LogError($"[TargetHandFinder] Could not find {targetHandType} hand ({(useOffHand ? "OFF-hand" : "MAIN-hand")}) in scene!");
            return new HandComponents();
        }

        /// <summary>
        /// Finds a specific hand by HandType (bypasses HandSelectionManager)
        /// </summary>
        public static HandComponents FindSpecificHand(HandType handType)
        {
            OVRHand[] hands = Object.FindObjectsByType<OVRHand>(FindObjectsSortMode.None);

            foreach (OVRHand hand in hands)
            {
                OVRSkeleton skeleton = hand.GetComponent<OVRSkeleton>();
                if (skeleton == null) continue;

                OVRSkeleton.SkeletonType skelType = skeleton.GetSkeletonType();
                bool isRightHand = (skelType == OVRSkeleton.SkeletonType.HandRight ||
                                   skelType == OVRSkeleton.SkeletonType.XRHandRight);
                bool isLeftHand = (skelType == OVRSkeleton.SkeletonType.HandLeft ||
                                  skelType == OVRSkeleton.SkeletonType.XRHandLeft);

                bool isMatch = (handType == HandType.Right && isRightHand) ||
                              (handType == HandType.Left && isLeftHand);

                if (isMatch)
                {
                    return new HandComponents
                    {
                        Hand = hand,
                        Skeleton = skeleton,
                        HandType = handType
                    };
                }
            }

            return new HandComponents();
        }
    }
}
