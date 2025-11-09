using MyBox;
using UnityEngine;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Laser power-up that shoots a beam from the index finger
    /// Activated by making a finger gun hand pose with the off-hand
    /// </summary>
    public class LaserPowerUp : PowerUpBase
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

        private LaserBeam laserBeam;
        private Transform fingerTipTransform;
        private OVRHand targetHand;
        private OVRSkeleton targetSkeleton;

        protected override void Awake()
        {
            base.Awake();

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

        protected override void Start()
        {
            base.Start();

            // Find the target hand and skeleton
            FindTargetHand();
        }

        private void FindTargetHand()
        {
            // Determine which hand to use based on HandSelectionManager
            HandType targetHandType = useOffHand
                ? HandSelectionManager.GetOffHand()
                : HandSelectionManager.GetMainHand();

            Debug.Log($"[LaserPowerUp] Looking for {(useOffHand ? "OFF" : "MAIN")} hand, which is: {targetHandType}");

            // Find all OVRHand components in scene
            OVRHand[] hands = FindObjectsByType<OVRHand>(FindObjectsSortMode.None);
            Debug.Log($"[LaserPowerUp] Found {hands.Length} OVRHand(s) in scene");

            foreach (OVRHand hand in hands)
            {
                // Get skeleton to check hand type
                OVRSkeleton skeleton = hand.GetComponent<OVRSkeleton>();
                if (skeleton == null)
                {
                    Debug.LogWarning($"[LaserPowerUp] OVRHand found but no OVRSkeleton attached!");
                    continue;
                }

                // Check skeleton type - support both OVR and OpenXR skeleton types
                OVRSkeleton.SkeletonType skelType = skeleton.GetSkeletonType();
                bool isRightHand = (skelType == OVRSkeleton.SkeletonType.HandRight ||
                                   skelType == OVRSkeleton.SkeletonType.XRHandRight);
                bool isLeftHand = (skelType == OVRSkeleton.SkeletonType.HandLeft ||
                                  skelType == OVRSkeleton.SkeletonType.XRHandLeft);

                Debug.Log($"[LaserPowerUp] Checking hand - SkeletonType: {skelType}, IsRight: {isRightHand}, IsLeft: {isLeftHand}");

                // Match against our target hand type
                bool isMatch = (targetHandType == HandType.Right && isRightHand) ||
                              (targetHandType == HandType.Left && isLeftHand);

                if (isMatch)
                {
                    targetHand = hand;
                    targetSkeleton = skeleton;
                    Debug.Log($"[LaserPowerUp] ✓ MATCHED! Using {targetHandType} hand for laser");
                    return;
                }
            }

            Debug.LogError($"[LaserPowerUp] Could not find {targetHandType} hand ({(useOffHand ? "OFF-hand" : "MAIN-hand")}) OVRHand in scene!");
        }

        protected override void Update()
        {
            base.Update();

            // Update finger tip position if laser is active
            if (isActive && laserBeam != null && laserBeam.IsActive)
            {
                UpdateFingerTipTransform();
            }
        }

        protected override void OnActivated()
        {
            Debug.Log("[LaserPowerUp] Laser power-up activated!");

            if (laserBeam == null)
            {
                Debug.LogError("[LaserPowerUp] No LaserBeam component found!");
                return;
            }

            if (targetHand == null || targetSkeleton == null)
            {
                Debug.LogWarning("[LaserPowerUp] Target hand not found, searching again...");
                FindTargetHand();

                if (targetHand == null || targetSkeleton == null)
                {
                    Debug.LogError("[LaserPowerUp] Still cannot find target hand! Aborting.");
                    return;
                }
            }

            // Get finger tip transform
            fingerTipTransform = GetFingerTipTransform();

            if (fingerTipTransform == null)
            {
                Debug.LogError("[LaserPowerUp] Could not find finger tip transform!");
                return;
            }

            // Start laser beam
            laserBeam.StartLaser(fingerTipTransform);
        }

        protected override void OnDeactivated()
        {
            Debug.Log("[LaserPowerUp] Laser power-up deactivated");

            if (laserBeam != null)
            {
                laserBeam.StopLaser();
            }

            fingerTipTransform = null;
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
                Debug.LogWarning("[LaserPowerUp] Skeleton not initialized yet");
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

            Debug.LogWarning($"[LaserPowerUp] Could not find bone: {fingerTipBone}");
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
