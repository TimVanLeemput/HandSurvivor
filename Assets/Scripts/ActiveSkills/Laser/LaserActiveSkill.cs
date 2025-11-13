using MyBox;
using UnityEngine;
using HandSurvivor.Utilities;
using HandSurvivor.ActiveSkills.HandShapes;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Laser active skill that shoots a beam from the index finger
    /// Fires during cooldown window when finger gun pose is detected
    /// </summary>
    public class LaserActiveSkill : ActiveSkillBase
    {
        [Header("Laser Configuration")]
        [SerializeField] private GameObject laserBeamPrefab;
        [SerializeField] private float laserDamage = 15f;
        [SerializeField] private float laserRange = 50f;
        [SerializeField] private Color laserColor = Color.red;

        [Header("Hand Tracking")]
        [SerializeField] private bool useOffHand = true;
        [Tooltip("Which finger tip to shoot from")]
        [SerializeField] private OVRSkeleton.BoneId fingerTipBone = OVRSkeleton.BoneId.Hand_IndexTip;

        [Header("Hand Shape Control")]
        [SerializeField] private HandShapeActivator handShapeActivator;
        [SerializeField] private bool requirePoseForFiring = true;

        private LaserBeam laserBeam;
        private Transform fingerTipTransform;
        private OVRHand targetHand;
        private OVRSkeleton targetSkeleton;
        private bool wasLaserFiring = false;

        protected void Awake()
        {
            // Create laser beam instance
            if (laserBeamPrefab != null)
            {
                GameObject laserObj = Instantiate(laserBeamPrefab, transform);
                laserBeam = laserObj.GetComponent<LaserBeam>();

                if (laserBeam == null)
                {
                    laserBeam = laserObj.AddComponent<LaserBeam>();
                }

                // Configure laser
                laserBeam.SetDamage(laserDamage);
                laserBeam.SetBeamColor(laserColor);
            }
            else
            {
                // Create basic laser beam if no prefab provided
                GameObject laserObj = new GameObject("LaserBeam");
                laserObj.transform.SetParent(transform);
                laserObj.AddComponent<LineRenderer>();
                laserBeam = laserObj.AddComponent<LaserBeam>();
                laserBeam.SetDamage(laserDamage);
                laserBeam.SetBeamColor(laserColor);
            }
        }

        protected void Start()
        {
            FindTargetHand();

            if (handShapeActivator == null)
            {
                handShapeActivator = GetComponent<HandShapeActivator>();

                if (handShapeActivator == null && requirePoseForFiring)
                {
                    handShapeActivator = gameObject.AddComponent<HandShapeActivator>();
                    Debug.Log("[LaserActiveSkill] Created HandShapeActivator component");
                }
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

        protected override void Update()
        {
            base.Update();

            if (!isActive)
            {
                return;
            }

            UpdateFingerTipTransform();

            if (requirePoseForFiring && handShapeActivator != null)
            {
                bool isPoseActive = handShapeActivator.IsPoseActive;

                if (isPoseActive && !wasLaserFiring)
                {
                    StartFiring();
                }
                else if (!isPoseActive && wasLaserFiring)
                {
                    StopFiring();
                }
            }
            else
            {
                if (!wasLaserFiring)
                {
                    StartFiring();
                }
            }
        }

        protected override void OnActivated()
        {
            Debug.Log("[LaserActiveSkill] Laser window started!");

            if (laserBeam == null)
            {
                Debug.LogError("[LaserActiveSkill] No LaserBeam component found!");
                return;
            }

            if (targetHand == null || targetSkeleton == null)
            {
                Debug.LogWarning("[LaserActiveSkill] Target hand not found, searching again...");
                FindTargetHand();

                if (targetHand == null || targetSkeleton == null)
                {
                    Debug.LogError("[LaserActiveSkill] Still cannot find target hand! Aborting.");
                    return;
                }
            }

            fingerTipTransform = GetFingerTipTransform();

            if (fingerTipTransform == null)
            {
                Debug.LogError("[LaserActiveSkill] Could not find finger tip transform!");
                return;
            }

            wasLaserFiring = false;
        }

        protected override void OnDeactivated()
        {
            Debug.Log("[LaserActiveSkill] Laser window ended");

            StopFiring();
            fingerTipTransform = null;
        }

        private void StartFiring()
        {
            if (laserBeam != null && fingerTipTransform != null)
            {
                float modifiedDamage = GetModifiedDamage(laserDamage);
                laserBeam.SetDamage(modifiedDamage);
                laserBeam.StartLaser(fingerTipTransform);
                wasLaserFiring = true;
                Debug.Log($"[LaserActiveSkill] Laser firing (Damage: {modifiedDamage})");
            }
        }

        private void StopFiring()
        {
            if (laserBeam != null && wasLaserFiring)
            {
                laserBeam.StopLaser();
                wasLaserFiring = false;
                Debug.Log("[LaserActiveSkill] Laser stopped");
            }
        }

        [ButtonMethod]
        private void stoplaser()
        {
            laserBeam.StopLaser();

        }

        private Transform GetFingerTipTransform()
        {
            if (targetSkeleton == null)
            {
                return null;
            }

            // Wait for skeleton to be initialized
            if (targetSkeleton.Bones == null || targetSkeleton.Bones.Count == 0)
            {
                Debug.LogWarning("[LaserActiveSkill] Skeleton not initialized yet");
                return null;
            }

            // Find the finger tip bone
            foreach (OVRBone bone in targetSkeleton.Bones)
            {
                if (bone.Id == fingerTipBone)
                {
                    return bone.Transform;
                }
            }

            Debug.LogWarning($"[LaserActiveSkill] Could not find bone: {fingerTipBone}");
            return null;
        }

        private void UpdateFingerTipTransform()
        {
            if (fingerTipTransform == null && targetSkeleton != null)
            {
                fingerTipTransform = GetFingerTipTransform();
            }
        }

        private void OnDrawGizmos()
        {
            // Draw debug visualization of finger tip in editor
            if (isActive && fingerTipTransform != null)
            {
                Gizmos.color = laserColor;
                Gizmos.DrawWireSphere(fingerTipTransform.position, 0.02f);
                Gizmos.DrawRay(fingerTipTransform.position, fingerTipTransform.forward * 1f);
            }
        }
    }
}
