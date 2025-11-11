using System;
using System.Collections.Generic;
using HandSurvivor.PowerUps;
using HandSurvivor.Utilities;
using UnityEngine;

public class LightningHandPowerUp : PowerUpBase
{
    [Header("Hand Tracking")]
    [SerializeField] private bool useOffHand = true;
    private OVRHand targetHand;
    private OVRSkeleton targetSkeleton;
    [SerializeField] private OVRSkeleton.BoneId thumbFingerTip = OVRSkeleton.BoneId.Hand_ThumbTip;
    [SerializeField] private OVRSkeleton.BoneId indexFingerTip = OVRSkeleton.BoneId.Hand_IndexTip;
    [SerializeField] private OVRSkeleton.BoneId middleFingerTip = OVRSkeleton.BoneId.Hand_MiddleTip;
    [SerializeField] private OVRSkeleton.BoneId ringFingerTip = OVRSkeleton.BoneId.Hand_RingTip;
    [SerializeField] private OVRSkeleton.BoneId pinkyFingerTip = OVRSkeleton.BoneId.Hand_PinkyTip;

    public FollowTransform ThumbTarget = null;
    public FollowTransform IndexTarget = null;
    public FollowTransform MiddleTarget = null;
    public FollowTransform RingTarget = null;
    public FollowTransform PinkyTarget = null;

    public List<FollowTransform> AllLightningTargets =>
        new List<FollowTransform>
        {
            ThumbTarget,
            IndexTarget,
            MiddleTarget,
            RingTarget,
            PinkyTarget
        };


    protected void Start()
    {
        // Find the target hand and skeleton
        FindTargetHand();
    }

    private void FindTargetHand()
    {
        TargetHandFinder.HandComponents handComponents = TargetHandFinder.FindHand(useOffHand);

        if (handComponents.IsValid)
        {
            targetHand = handComponents.Hand;
            targetSkeleton = handComponents.Skeleton;
        }
    }

    protected override void OnActivated() 
    {
    }

    protected override void OnDeactivated()
    {
    }
}
