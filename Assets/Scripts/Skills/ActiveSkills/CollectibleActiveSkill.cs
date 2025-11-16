using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Collectible active skill that can be picked up in the world
    /// Supports both collision-based and grab-based pickup
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectibleActiveSkill : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Active Skill Configuration")] [SerializeField]
        private ActiveSkillData activeSkillData;

        [SerializeField] private GameObject activeSkillPrefab;

        [Header("Pickup Settings")]
        [Tooltip("Automatically pickup on collision (true) or require grab interaction (false)")]
        [SerializeField]
        private bool autoPickupOnCollision = true;

        [Header("Which layers can pick up?")] 
        [SerializeField] private LayerMask layers;

        [Header("Which hand can pick up?")] [SerializeField]
        private bool mainHandCanPickup = true;

        [SerializeField] private bool offHandCanPickup = true;

        [Header("Visual")] [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private bool shouldBobUpAndDown = true;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobAmount = 0.2f;
        [SerializeField] private GameObject visualMesh;

        [Header("Effects")] [SerializeField] private GameObject auraEffectPrefab;
        [SerializeField] private AudioClip ambientSound;

        [Header("Events")] public UnityEvent OnPickedUp;

        private Vector3 startPosition;
        private float bobOffset;
        private GameObject auraEffect;
        private AudioSource audioSource;
        private bool hasBeenCollected = false;

        private void Awake()
        {
            // Ensure collider is trigger
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;

            // Store start position for bobbing
            startPosition = transform.position;
            bobOffset = Random.Range(0f, Mathf.PI * 2f); // Random phase for variety

            // Spawn aura effect if provided
            if (auraEffectPrefab != null)
            {
                auraEffect = Instantiate(auraEffectPrefab, transform.position, Quaternion.identity, transform);
            }

            // Setup ambient sound
            if (ambientSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = ambientSound;
                audioSource.loop = true;
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.Play();
            }
        }

        private void Update()
        {
            if (hasBeenCollected)
            {
                return;
            }

            // Rotate the power-up
            if (visualMesh != null)
            {
                visualMesh.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }

            // Bob up and down
            if (shouldBobUpAndDown)
            {
                float newY = startPosition.y + Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobAmount;
                Vector3 newPos = transform.position;
                newPos.y = newY;
                transform.position = newPos;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[CollectibleActiveSkill] OnTriggerEnter - Object: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}", other.gameObject);

            if (!autoPickupOnCollision || hasBeenCollected)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[CollectibleActiveSkill] Skipped - autoPickup: {autoPickupOnCollision}, collected: {hasBeenCollected}");
                return;
            }

            // Check if collider is on player layer
            bool isPlayer = ((1 << other.gameObject.layer) & layers) != 0;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[CollectibleActiveSkill] Layer check - isPlayer: {isPlayer}, layers mask: {layers.value}");
            if (!isPlayer)
            {
                return;
            }

            // Determine if this hand is main or off hand
            bool isMainHand = HandSelectionManager.IsMainHandObject(other.gameObject);
            bool isOffHand = HandSelectionManager.IsOffHandObject(other.gameObject);
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[CollectibleActiveSkill] isMainHand: {isMainHand}, isOffHand: {isOffHand}");
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[CollectibleActiveSkill] Pickup permissions - mainHandCanPickup: {mainHandCanPickup}, offHandCanPickup: {offHandCanPickup}");

            // Check if this hand is allowed to pick up
            if ((isMainHand && mainHandCanPickup) || (isOffHand && offHandCanPickup))
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[CollectibleActiveSkill] ✓ Pickup allowed! Calling Pickup()");
                Pickup();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log(
                    $"  [CollectibleActiveSkill] Picked up: {activeSkillData.displayName}, collider hit {other.gameObject.name}",
                    other.gameObject);
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[CollectibleActiveSkill] ✗ Pickup denied - hand not allowed");
            }
        }

        /// <summary>
        /// Manually trigger pickup (for grab interactions)
        /// </summary>
        [ButtonMethod]
        public void Pickup()
        {
            if (hasBeenCollected)
            {
                return;
            }

            hasBeenCollected = true;

            // Validate configuration
            if (activeSkillData == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogError("[CollectibleActiveSkill] No ActiveSkillData assigned!");
                Destroy(gameObject);
                return;
            }

            if (activeSkillPrefab == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogError("[CollectibleActiveSkill] No ActiveSkill prefab assigned!");
                Destroy(gameObject);
                return;
            }

            // Check if inventory exists
            if (ActiveSkillInventory.Instance == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogError("[CollectibleActiveSkill] No ActiveSkillInventory found in scene!");
                Destroy(gameObject);
                return;
            }

            // Check if inventory is full
            // if (ActiveSkillInventory.Instance.is)
            // {
            //     if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
         Debug.LogWarning($"[CollectibleActiveSkill] Inventory full! Cannot pickup {activeSkillData.displayName}");
            //     hasBeenCollected = false; // Allow retry
            //     return;
            // }

            // Instantiate the active skill
            GameObject activeSkillObj = Instantiate(activeSkillPrefab);
            ActiveSkillBase activeSkill = activeSkillObj.GetComponent<ActiveSkillBase>();

            if (activeSkill == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogError($"[CollectibleActiveSkill] ActiveSkill prefab does not have an ActiveSkillBase component!");
                Destroy(activeSkillObj);
                Destroy(gameObject);
                return;
            }

            // Trigger pickup on the active skill
            activeSkill.Pickup();

            // Fire events
            OnPickedUp?.Invoke();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


                Debug.Log($"[CollectibleActiveSkill] Picked up: {activeSkillData.displayName}");

            // Destroy the collectible
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (auraEffect != null)
            {
                Destroy(auraEffect);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw pickup radius
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }
}