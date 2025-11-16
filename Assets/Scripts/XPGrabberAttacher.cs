using UnityEngine;
using HandSurvivor.Utilities;

/// <summary>
/// Attaches the XP Grabber prefab to the off-hand at runtime
/// Place this component on the VR rig prefab
/// </summary>
public class XPGrabberAttacher : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("XP Grabber Setup")]
    [SerializeField] private GameObject xpGrabberObject;
    [SerializeField] private OVRSkeleton.BoneId attachBoneId = OVRSkeleton.BoneId.Hand_WristRoot;
    [SerializeField] private float attachDelay = 0.5f;

    private void Start()
    {
        if (xpGrabberObject == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogError("[XPGrabberAttacher] XP Grabber object not assigned!");
            return;
        }

        Invoke(nameof(AttachXPGrabber), attachDelay);
    }

    private void AttachXPGrabber()
    {
        TargetHandFinder.HandComponents offHand = TargetHandFinder.FindOffHand();

        if (!offHand.IsValid)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogError("[XPGrabberAttacher] Could not find off-hand! Retrying in 1 second...");
            Invoke(nameof(AttachXPGrabber), 1f);
            return;
        }

        // Get bone transform from OVRSkeleton by searching for matching BoneId
        Transform targetTransform = null;
        foreach (OVRBone bone in offHand.Skeleton.Bones)
        {
            if (bone.Id == attachBoneId)
            {
                targetTransform = bone.Transform;
                break;
            }
        }

        if (targetTransform == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogWarning($"[XPGrabberAttacher] Bone {attachBoneId} not found in skeleton, falling back to hand root");
            targetTransform = offHand.Hand.transform;
        }

        // Attach existing grabber to the target transform
        xpGrabberObject.transform.SetParent(targetTransform);
        xpGrabberObject.transform.localPosition = Vector3.zero;
        xpGrabberObject.transform.localRotation = Quaternion.identity;

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


            Debug.Log($"[XPGrabberAttacher] Successfully attached XP Grabber to off-hand ({offHand.HandType}) bone: {attachBoneId}", xpGrabberObject);
    }
}
