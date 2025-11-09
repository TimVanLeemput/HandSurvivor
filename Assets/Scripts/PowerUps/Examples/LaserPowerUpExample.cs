using UnityEngine;

namespace HandSurvivor.PowerUps.Examples
{
    /// <summary>
    /// Example script showing how to use the Laser Power-Up system
    ///
    /// SETUP INSTRUCTIONS:
    ///
    /// 1. CREATE POWER-UP DATA ASSET:
    ///    - Right-click in Project: Create > HandSurvivor > PowerUp Data
    ///    - Name it "LaserPowerUpData"
    ///    - Configure:
    ///      * powerUpId: "laser"
    ///      * displayName: "Laser Beam"
    ///      * description: "Shoot a laser from your index finger!"
    ///      * powerUpType: Duration
    ///      * duration: 3.0 seconds
    ///
    /// 2. CREATE LASER POWER-UP PREFAB:
    ///    - Create empty GameObject, name it "LaserPowerUp"
    ///    - Add LaserPowerUp component
    ///    - Assign the PowerUpData asset to the 'data' field
    ///    - Add HandPoseActivator component
    ///    - Assign HandPoseActivator to LaserPowerUp's 'activatorComponent' field
    ///    - Configure HandPoseActivator:
    ///      * posePrefab: Assign RockPose prefab from Meta SDK
    ///        Location: Packages/Meta XR Interaction SDK/Runtime/Sample/Prefabs/HandPose/RockPose.prefab
    ///    - Save as prefab in Assets/Prefabs/PowerUps/
    ///
    /// 3. CREATE COLLECTIBLE PREFAB:
    ///    - Create GameObject with Sphere mesh (or custom model)
    ///    - Add SphereCollider, set as trigger
    ///    - Add CollectiblePowerUp component
    ///    - Assign:
    ///      * powerUpData: LaserPowerUpData asset
    ///      * powerUpPrefab: LaserPowerUp prefab from step 2
    ///    - Adjust visual settings (rotation, bobbing)
    ///    - Save as prefab in Assets/Prefabs/Collectibles/
    ///
    /// 4. SETUP SCENE:
    ///    - Ensure PowerUpInventory exists in scene (or create empty GameObject with component)
    ///    - Place collectible prefab(s) in the world
    ///    - Ensure OVRCameraRig exists with hand tracking enabled
    ///    - Configure OVRCameraRig:
    ///      * Hand Tracking Support: Controllers And Hands
    ///      * OVRHand components on LeftHandAnchor and RightHandAnchor
    ///      * OVRSkeleton components on both hands
    ///
    /// 5. TEST IN VR:
    ///    - Put on Quest headset
    ///    - Walk to collectible and touch it to pick up
    ///    - Make rock gesture with your hand (🤘 - pinky & index extended, thumb out)
    ///    - Hold gesture for ~0.3 seconds
    ///    - Laser should shoot from index finger for duration specified in PowerUpData
    ///
    /// TROUBLESHOOTING:
    ///
    /// - "No PowerUpInventory found":
    ///   Add PowerUpInventory component to any GameObject in the scene
    ///
    /// - "Could not find OVRHand":
    ///   Make sure OVRHand and OVRSkeleton are on LeftHandAnchor/RightHandAnchor
    ///   Enable hand tracking in OVRCameraRig settings
    ///
    /// - "Skeleton not initialized":
    ///   OVRSkeleton needs a frame to initialize. Try waiting ~1 second after scene load.
    ///
    /// - "Pose not detected":
    ///   Ensure the RockPose prefab is properly assigned and active
    ///   Check Debug logs for HandPoseDetector messages
    ///   Ensure hand tracking is working in Quest settings
    ///   Try different Meta SDK pose prefabs (ThumbsUpPose, ScissorsPose, etc.)
    ///
    /// - "No IActiveState found":
    ///   Make sure the pose prefab from Meta SDK is assigned to HandPoseActivator
    ///   Verify the prefab contains a ShapeRecognizerActiveState component
    ///
    /// EXTENDING THE SYSTEM:
    ///
    /// To create new power-ups:
    ///
    /// 1. Create new class extending PowerUpBase
    /// 2. Override OnActivated() and OnDeactivated()
    /// 3. Implement your ability logic
    /// 4. Create PowerUpData asset
    /// 5. Create prefab with your component + activator
    /// 6. Test!
    ///
    /// Example power-up ideas:
    /// - Shield (duration-based protection)
    /// - Teleport (one-time use instant movement)
    /// - Speed Boost (duration-based movement modifier)
    /// - Freeze (one-time freeze all enemies)
    /// - Lightning (activated by open palm gesture)
    ///
    /// </summary>
    public class LaserPowerUpExample : MonoBehaviour
    {
        [Header("Example Configuration")]
        [SerializeField] private GameObject laserCollectiblePrefab;
        [SerializeField] private float spawnHeight = 1.5f;
        [SerializeField] private float spawnDistance = 2f;

        [MyBox.ButtonMethod]
        private void SpawnLaserPowerUp()
        {
            if (laserCollectiblePrefab == null)
            {
                Debug.LogWarning("[LaserPowerUpExample] No collectible prefab assigned!");
                return;
            }

            // Spawn in front of camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogWarning("[LaserPowerUpExample] No main camera found!");
                return;
            }

            Vector3 spawnPos = mainCam.transform.position +
                              mainCam.transform.forward * spawnDistance;
            spawnPos.y = spawnHeight;

            GameObject collectible = Instantiate(laserCollectiblePrefab, spawnPos, Quaternion.identity);
            Debug.Log($"[LaserPowerUpExample] Spawned laser power-up at {spawnPos}");
        }

        [MyBox.ButtonMethod]
        private void PrintInventoryStatus()
        {
            if (PowerUpInventory.Instance == null)
            {
                Debug.LogWarning("[LaserPowerUpExample] No PowerUpInventory in scene!");
                return;
            }

            Debug.Log($"[LaserPowerUpExample] Inventory: {PowerUpInventory.Instance.Count} power-ups");

            foreach (PowerUpBase powerUp in PowerUpInventory.Instance.PowerUps)
            {
                Debug.Log($"  - {powerUp.Data.displayName} (Active: {powerUp.IsActive}, Cooldown: {powerUp.IsOnCooldown})");
            }
        }

        [MyBox.ButtonMethod]
        private void ClearInventory()
        {
            if (PowerUpInventory.Instance != null)
            {
                PowerUpInventory.Instance.ClearInventory();
                Debug.Log("[LaserPowerUpExample] Inventory cleared");
            }
        }
    }
}
