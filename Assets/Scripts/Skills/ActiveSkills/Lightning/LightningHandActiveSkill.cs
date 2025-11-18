using System;
using System.Collections.Generic;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Utilities;
using UnityEngine;

public class LightningHandActiveSkill : ActiveSkillBase
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

    public FollowTransform ThumbFollowTransform = null;
    public FollowTransform IndexFollowTransform = null;
    public FollowTransform MiddleFollowTransform = null;
    public FollowTransform RingFollowTransform = null;
    public FollowTransform PinkyFollowTransform = null;

    public List<FollowTransform> AllLightningFollowTransform =>
        new List<FollowTransform>
        {
            ThumbFollowTransform,
            IndexFollowTransform,
            MiddleFollowTransform,
            RingFollowTransform,
            PinkyFollowTransform
        };
    public List<OVRSkeleton.BoneId> AllLightningTargetBoneIds =>
        new List<OVRSkeleton.BoneId>
        {
            thumbFingerTip,
            indexFingerTip,
            middleFingerTip,
            ringFingerTip,
            pinkyFingerTip
        };


    protected new void Start()
    {
        FindTargetHand();
        InitializeAllFollowTransformTargets();
    }

    private void InitializeAllFollowTransformTargets()
    {
        int length = AllLightningFollowTransform.Count;
        for (int i = 0; i < length; i++)
        {
            AllLightningFollowTransform[i].Target = GetFingerTipTransform(AllLightningTargetBoneIds[i]);
        }
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
    
    private Transform GetFingerTipTransform(OVRSkeleton.BoneId fingerTipBone)
    {
        if (targetSkeleton == null)
        {
            return null;
        }

        if (targetSkeleton.Bones == null || targetSkeleton.Bones.Count == 0)
        {
            Debug.LogWarning("[LightningHandActiveSkill] Skeleton not initialized yet");
            return null;
        }

        foreach (OVRBone bone in targetSkeleton.Bones)
        {
            if (bone.Id == fingerTipBone)
            {
                return bone.Transform;
            }
        }

        Debug.LogWarning($"[LightningHandActiveSkill] Could not find bone: {fingerTipBone}");
        return null;
    }
    
    protected override void OnActivated() 
    {
    }

    protected override void OnDeactivated()
    {
    }
}
