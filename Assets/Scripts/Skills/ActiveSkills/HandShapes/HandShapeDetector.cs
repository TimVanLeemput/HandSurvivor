using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.ActiveSkills.HandShapes
{
    public class HandShapeDetector : MonoBehaviour
    {
        [Header("Hand References")]
        [SerializeField] private OVRHand ovrHand;
        [SerializeField] private OVRSkeleton ovrSkeleton;

        [Header("Finger Gun Detection")]
        [Tooltip("Index finger must be this extended (0-1)")]
        [SerializeField] private float indexExtendedThreshold = 0.7f;

        [Tooltip("Other fingers must be this curled (0-1)")]
        [SerializeField] private float fingersCurledThreshold = 0.3f;

        [Header("Detection Settings")]
        [SerializeField] private float detectionInterval = 0.1f;
        [SerializeField] private bool enableDebugLogs = false;

        [Header("Events")]
        public UnityEvent OnFingerGunDetected;
        public UnityEvent OnFingerGunLost;

        private bool isFingerGunActive = false;
        private float lastCheckTime = 0f;

        public bool IsFingerGunActive => isFingerGunActive;

        private void Update()
        {
            if (Time.time - lastCheckTime >= detectionInterval)
            {
                lastCheckTime = Time.time;
                CheckFingerGunPose();
            }
        }

        private void CheckFingerGunPose()
        {
            if (ovrHand == null || ovrSkeleton == null)
            {
                return;
            }

            if (!ovrHand.IsTracked)
            {
                SetFingerGunActive(false);
                return;
            }

            bool isPoseDetected = DetectFingerGun();

            if (isPoseDetected && !isFingerGunActive)
            {
                SetFingerGunActive(true);
            }
            else if (!isPoseDetected && isFingerGunActive)
            {
                SetFingerGunActive(false);
            }
        }

        private bool DetectFingerGun()
        {
            float indexCurl = GetFingerCurl(OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.Hand_Index3, OVRSkeleton.BoneId.Hand_IndexTip);
            float middleCurl = GetFingerCurl(OVRSkeleton.BoneId.Hand_Middle1, OVRSkeleton.BoneId.Hand_Middle2, OVRSkeleton.BoneId.Hand_Middle3, OVRSkeleton.BoneId.Hand_MiddleTip);
            float ringCurl = GetFingerCurl(OVRSkeleton.BoneId.Hand_Ring1, OVRSkeleton.BoneId.Hand_Ring2, OVRSkeleton.BoneId.Hand_Ring3, OVRSkeleton.BoneId.Hand_RingTip);
            float pinkyCurl = GetFingerCurl(OVRSkeleton.BoneId.Hand_Pinky1, OVRSkeleton.BoneId.Hand_Pinky2, OVRSkeleton.BoneId.Hand_Pinky3, OVRSkeleton.BoneId.Hand_PinkyTip);

            float indexExtension = 1f - indexCurl;

            bool indexExtended = indexExtension >= indexExtendedThreshold;
            bool othersCurled = middleCurl <= fingersCurledThreshold &&
                               ringCurl <= fingersCurledThreshold &&
                               pinkyCurl <= fingersCurledThreshold;

            if (enableDebugLogs)
            {
                Debug.Log($"[HandShapeDetector] Index: {indexExtension:F2}, Middle: {middleCurl:F2}, Ring: {ringCurl:F2}, Pinky: {pinkyCurl:F2} | Detected: {indexExtended && othersCurled}");
            }

            return indexExtended && othersCurled;
        }

        private float GetFingerCurl(OVRSkeleton.BoneId bone1, OVRSkeleton.BoneId bone2, OVRSkeleton.BoneId bone3, OVRSkeleton.BoneId tip)
        {
            Transform bone1Transform = GetBoneTransform(bone1);
            Transform bone2Transform = GetBoneTransform(bone2);
            Transform bone3Transform = GetBoneTransform(bone3);
            Transform tipTransform = GetBoneTransform(tip);

            if (bone1Transform == null || bone2Transform == null || bone3Transform == null || tipTransform == null)
            {
                return 0f;
            }

            Vector3 bone1Forward = bone1Transform.forward;
            Vector3 bone2Forward = bone2Transform.forward;
            Vector3 bone3Forward = bone3Transform.forward;

            float angle1 = Vector3.Angle(bone1Forward, bone2Forward);
            float angle2 = Vector3.Angle(bone2Forward, bone3Forward);

            float totalAngle = angle1 + angle2;
            float maxAngle = 180f;

            float curlAmount = Mathf.Clamp01(totalAngle / maxAngle);

            return curlAmount;
        }

        private Transform GetBoneTransform(OVRSkeleton.BoneId boneId)
        {
            if (ovrSkeleton == null || ovrSkeleton.Bones == null)
            {
                return null;
            }

            foreach (OVRBone bone in ovrSkeleton.Bones)
            {
                if (bone.Id == boneId)
                {
                    return bone.Transform;
                }
            }

            return null;
        }

        private void SetFingerGunActive(bool active)
        {
            if (isFingerGunActive != active)
            {
                isFingerGunActive = active;

                if (active)
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log("[HandShapeDetector] Finger gun pose DETECTED");
                    }
                    OnFingerGunDetected?.Invoke();
                }
                else
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log("[HandShapeDetector] Finger gun pose LOST");
                    }
                    OnFingerGunLost?.Invoke();
                }
            }
        }

        public void SetHand(OVRHand hand, OVRSkeleton skeleton)
        {
            ovrHand = hand;
            ovrSkeleton = skeleton;
        }

        public void SetDebugMode(bool enabled)
        {
            enableDebugLogs = enabled;
        }
    }
}
