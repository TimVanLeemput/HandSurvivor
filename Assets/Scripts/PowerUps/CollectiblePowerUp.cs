using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Collectible power-up that can be picked up in the world
    /// Supports both collision-based and grab-based pickup
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectiblePowerUp : MonoBehaviour
    {
        [Header("Power-Up Configuration")] [SerializeField]
        private PowerUpData powerUpData;

        [SerializeField] private GameObject powerUpPrefab;

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
            Debug.Log($"[CollectiblePowerUp] OnTriggerEnter - Object: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}", other.gameObject);

            if (!autoPickupOnCollision || hasBeenCollected)
            {
                Debug.Log($"[CollectiblePowerUp] Skipped - autoPickup: {autoPickupOnCollision}, collected: {hasBeenCollected}");
                return;
            }

            // Check if collider is on player layer
            bool isPlayer = ((1 << other.gameObject.layer) & layers) != 0;
            Debug.Log($"[CollectiblePowerUp] Layer check - isPlayer: {isPlayer}, layers mask: {layers.value}");
            if (!isPlayer)
            {
                return;
            }

            // Determine if this hand is main or off hand
            bool isMainHand = HandSelectionManager.IsMainHandObject(other.gameObject);
            bool isOffHand = HandSelectionManager.IsOffHandObject(other.gameObject);
            Debug.Log($"[CollectiblePowerUp] isMainHand: {isMainHand}, isOffHand: {isOffHand}");
            Debug.Log($"[CollectiblePowerUp] Pickup permissions - mainHandCanPickup: {mainHandCanPickup}, offHandCanPickup: {offHandCanPickup}");

            // Check if this hand is allowed to pick up
            if ((isMainHand && mainHandCanPickup) || (isOffHand && offHandCanPickup))
            {
                Debug.Log($"[CollectiblePowerUp] ✓ Pickup allowed! Calling Pickup()");
                Pickup();
                Debug.Log(
                    $"  [CollectiblePowerUp] Picked up: {powerUpData.displayName}, collider hit {other.gameObject.name}",
                    other.gameObject);
            }
            else
            {
                Debug.Log($"[CollectiblePowerUp] ✗ Pickup denied - hand not allowed");
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
            if (powerUpData == null)
            {
                Debug.LogError("[CollectiblePowerUp] No PowerUpData assigned!");
                Destroy(gameObject);
                return;
            }

            if (powerUpPrefab == null)
            {
                Debug.LogError("[CollectiblePowerUp] No PowerUp prefab assigned!");
                Destroy(gameObject);
                return;
            }

            // Check if inventory exists
            if (PowerUpInventory.Instance == null)
            {
                Debug.LogError("[CollectiblePowerUp] No PowerUpInventory found in scene!");
                Destroy(gameObject);
                return;
            }

            // Check if inventory is full
            // if (PowerUpInventory.Instance.is)
            // {
            //     Debug.LogWarning($"[CollectiblePowerUp] Inventory full! Cannot pickup {powerUpData.displayName}");
            //     hasBeenCollected = false; // Allow retry
            //     return;
            // }

            // Instantiate the power-up
            GameObject powerUpObj = Instantiate(powerUpPrefab);
            PowerUpBase powerUp = powerUpObj.GetComponent<PowerUpBase>();

            if (powerUp == null)
            {
                Debug.LogError($"[CollectiblePowerUp] PowerUp prefab does not have a PowerUpBase component!");
                Destroy(powerUpObj);
                Destroy(gameObject);
                return;
            }

            // Trigger pickup on the power-up
            powerUp.Pickup();

            // Fire events
            OnPickedUp?.Invoke();

            // Play pickup effects (from PowerUpData)
            if (powerUpData.pickupVFX != null)
            {
                Instantiate(powerUpData.pickupVFX, transform.position, Quaternion.identity);
            }

            if (powerUpData.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(powerUpData.pickupSound, transform.position);
            }

            Debug.Log($"[CollectiblePowerUp] Picked up: {powerUpData.displayName}");

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