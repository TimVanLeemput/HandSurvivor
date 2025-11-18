using MyBox;
using UnityEngine;
using HandSurvivor.Utilities;
using Oculus.Interaction.PoseDetection;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Laser active skill that shoots a beam from the index finger
    /// Fires during cooldown window when finger gun pose is detected
    /// </summary>
    public class UFOActiveSkill : ActiveSkillBase
    {

        [Header("Laser Configuration")] [SerializeField]
        private GameObject laserBeamPrefab;

        [Header("Hand Tracking")] [SerializeField]
        private bool useOffHand = true;

        [Tooltip("Which finger tip to shoot from")] [SerializeField]
        private OVRSkeleton.BoneId fingerTipBone = OVRSkeleton.BoneId.Hand_IndexTip;

        [Header("Primed State")] [SerializeField]
        private GameObject primedParticlesPrefab;

        private LaserBeam laserBeam;
        private Transform fingerTipTransform;
        private OVRHand targetHand;
        private OVRSkeleton targetSkeleton;
        private bool wasLaserFiring = false;

        private GameObject primedParticlesInstance;

        protected new void Awake()
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
            }
            else
            {
                // Create basic laser beam if no prefab provided
                GameObject laserObj = new GameObject("LaserBeam");
                laserObj.transform.SetParent(transform);
                laserObj.AddComponent<LineRenderer>();
                laserBeam = laserObj.AddComponent<LaserBeam>();
            }
        }

        protected new void Start()
        {
            FindTargetHand();
            SetupHandShapeListener();
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

        private void SetupHandShapeListener()
        {
            if (HandShapeManager.Instance == null)
            {
                Debug.LogWarning("[LaserActiveSkill] No HandShapeManager assigned!");
                return;
            }
        }


      

        protected override void Activate()
        {
            isActive = true;
            activationTime = Time.time;

            OnActivate?.Invoke();
            OnActivated();
        }

        public override bool CanActivate()
        {
            if (isOnCooldown)
            {
                return false;
            }

            return true;
        }

        protected override void OnActivated()
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[LaserActiveSkill] Entering Primed state!");

            if (laserBeam == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError("[LaserActiveSkill] No LaserBeam component found!");
                return;
            }

            if (targetHand == null || targetSkeleton == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[LaserActiveSkill] Target hand not found, searching again...");
                FindTargetHand();

                if (targetHand == null || targetSkeleton == null)
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.LogError("[LaserActiveSkill] Still cannot find target hand! Aborting.");
                    return;
                }
            }

            fingerTipTransform = GetFingerTipTransform();

            if (fingerTipTransform == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError("[LaserActiveSkill] Could not find finger tip transform!");
                return;
            }

            wasLaserFiring = false;
            EnterPrimedState();
        }

        protected override void OnDeactivated()
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[LaserActiveSkill] Laser duration expired");

            // When duration expires, enter cooling down state
            EnterCoolingDownState();
            fingerTipTransform = null;
        }

        private void StartFiring()
        {
            if (laserBeam != null && fingerTipTransform != null)
            {
                float modifiedDamage = GetModifiedDamage(data.damage);
                float modifiedDuration = GetModifiedDuration();
                laserBeam.SetDamage(modifiedDamage);
                laserBeam.SetSkillId(data.activeSkillId);
                laserBeam.StartLaser(fingerTipTransform, modifiedDuration);
                wasLaserFiring = true;
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[LaserActiveSkill] Laser firing (Damage: {modifiedDamage}, Duration: {modifiedDuration})");
            }
        }

        private void StopFiring()
        {
            if (laserBeam != null && wasLaserFiring)
            {
                laserBeam.StopLaser();
                wasLaserFiring = false;
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
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
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
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

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
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
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(fingerTipTransform.position, 0.02f);
                Gizmos.DrawRay(fingerTipTransform.position, fingerTipTransform.forward * 1f);
            }
        }

        #region State Management

        private void EnterPrimedState()
        {
            SpawnPrimedParticles();
            PlayActivationEffects(); // Play primed sound when entering primed state
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[LaserActiveSkill] Entered PRIMED state - sparkles active");
        }

        private void ExitPrimedState()
        {
            DestroyPrimedParticles();
        }

        private void EnterFiringState()
        {
            ExitPrimedState(); // Remove sparkles FIRST

            // Set activation time for duration tracking
            activationTime = Time.time;

            StartFiring();
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[LaserActiveSkill] Entered FIRING state - laser active");
        }

        private void EnterCoolingDownState()
        {
            StopFiring();

            // Start cooldown NOW (when entering cooling down)
            if (data != null && data.cooldown > 0f)
            {
                isOnCooldown = true;
                cooldownEndTime = Time.time + GetModifiedCooldown();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log(
                        $"[LaserActiveSkill Instance {GetInstanceID()}] Cooldown started: isOnCooldown={isOnCooldown}, cooldownEndTime={cooldownEndTime}, modified cooldown={GetModifiedCooldown()}s");
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning($"[LaserActiveSkill] Cooldown NOT started - data={data}, cooldown={data?.cooldown}");
            }

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log(
                    $"[LaserActiveSkill Instance {GetInstanceID()}] Entered COOLING_DOWN state - waiting for cooldown");
        }

        #endregion

        #region Primed Particles

        private void SpawnPrimedParticles()
        {
            if (primedParticlesPrefab == null)
            {
                Debug.LogWarning("[LaserActiveSkill] No primed particles prefab assigned");
                return;
            }

            if (fingerTipTransform == null)
            {
                Debug.LogWarning("[LaserActiveSkill] No finger tip transform for particles");
                return;
            }

            // Reuse existing instance if available
            if (primedParticlesInstance == null)
            {
                primedParticlesInstance = Instantiate(primedParticlesPrefab, fingerTipTransform);
                primedParticlesInstance.transform.localPosition = Vector3.zero;
                Debug.Log("[LaserActiveSkill] Primed particles created");
            }
            else
            {
                // Reactivate cached instance
                primedParticlesInstance.SetActive(true);
                Debug.Log("[LaserActiveSkill] Primed particles reactivated");
            }
        }

        private void DestroyPrimedParticles()
        {
            if (primedParticlesInstance != null)
            {
                // Deactivate instead of destroying for reuse
                primedParticlesInstance.SetActive(false);
                Debug.Log("[LaserActiveSkill] Primed particles deactivated");
            }
        }

        #endregion
    }
}