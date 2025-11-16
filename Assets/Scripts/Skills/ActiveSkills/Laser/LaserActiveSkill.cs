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
    public class LaserActiveSkill : ActiveSkillBase
    {
        private enum LaserState
        {
            Idle, // Not slotted or inactive
            Primed, // Slotted, ready to fire (sparkles active)
            Firing, // Laser active (pose detected)
            CoolingDown // Post-fire cooldown (no sparkles, can't fire)
        }

        [Header("Laser Configuration")] [SerializeField]
        private GameObject laserBeamPrefab;

        [SerializeField] private float laserDamage = 15f;

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

        private LaserState currentState = LaserState.Idle;
        private GameObject primedParticlesInstance;

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
            }
            else
            {
                // Create basic laser beam if no prefab provided
                GameObject laserObj = new GameObject("LaserBeam");
                laserObj.transform.SetParent(transform);
                laserObj.AddComponent<LineRenderer>();
                laserBeam = laserObj.AddComponent<LaserBeam>();
                laserBeam.SetDamage(laserDamage);
            }
        }

        protected void Start()
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

            HandShapeManager.Instance.OnFingerGun.AddListener(OnFingerGunDetected);
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[LaserActiveSkill] Listening to HandShapeManager OnFingerGun event");
        }

        protected override void Update()
        {
            // Handle our custom cooldown logic separately from base
            // Check cooldown expiry for CoolingDown state
            if (currentState == LaserState.CoolingDown && isOnCooldown)
            {
                if (Time.time >= cooldownEndTime)
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log($"[LaserActiveSkill] Cooldown expired at {Time.time} (end time was {cooldownEndTime})");
                    isOnCooldown = false;
                    EnterPrimedState(); // Return to primed state when cooldown completes
                }
            }

            // Duration tracking for Firing state only (how long laser can fire)
            if (currentState == LaserState.Firing && Time.time >= activationTime + GetModifiedDuration())
            {
                // Laser duration expired while firing - transition to cooling down
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[LaserActiveSkill] Duration expired at {Time.time}");
                OnDeactivate?.Invoke();
                EnterCoolingDownState();
                OnExpired();
            }

            UpdateFingerTipTransform();
        }

        public void OnFingerGunDetected()
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[LaserActiveSkill] OnFingerGunDetected - currentState={currentState}");

            // Fire if in Primed state
            if (currentState == LaserState.Primed)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[LaserActiveSkill] Primed state detected - calling EnterFiringState");
                EnterFiringState();
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[LaserActiveSkill] Cannot fire - not in Primed state");
            }
        }

        protected override void Activate()
        {
            // If already primed or in any state other than Idle/CoolingDown, do nothing
            if (currentState == LaserState.Primed || currentState == LaserState.Firing)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[LaserActiveSkill] Activate() blocked - already in {currentState} state");
                return;
            }

            // Only allow activation from CoolingDown if cooldown has completed
            if (currentState == LaserState.CoolingDown && isOnCooldown)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[LaserActiveSkill] Activate() blocked - still on cooldown");
                return;
            }

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[LaserActiveSkill] Activate() called from {currentState} state");

            // Override base activation to NOT start cooldown
            // Instead, enter Primed state
            isActive = true;
            activationTime = Time.time;

            OnActivate?.Invoke();
            // Don't play activation sound here - only play when primed first time (in Pickup) or when laser fires
            OnActivated();
        }

        public override bool CanActivate()
        {
            // Prevent re-activation if already primed or firing
            if (currentState == LaserState.Primed || currentState == LaserState.Firing)
            {
                return false;
            }

            // Prevent activation during cooldown
            if (isOnCooldown)
            {
                return false;
            }

            // Idle or CoolingDown with cooldown complete can activate
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
                float modifiedDamage = GetModifiedDamage(laserDamage);
                float modifiedDuration = GetModifiedDuration();
                laserBeam.SetDamage(modifiedDamage);
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
            // Prevent re-entering if already primed
            if (currentState == LaserState.Primed)
            {
                return;
            }

            currentState = LaserState.Primed;
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
            currentState = LaserState.Firing;

            // Set activation time for duration tracking
            activationTime = Time.time;

            StartFiring();
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[LaserActiveSkill] Entered FIRING state - laser active");
        }

        private void EnterCoolingDownState()
        {
            currentState = LaserState.CoolingDown;
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