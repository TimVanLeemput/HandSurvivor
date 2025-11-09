using UnityEngine;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Visual laser beam with damage dealing capabilities
    /// Uses LineRenderer for beam visualization and raycasting for hit detection
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
        [Header("Beam Settings")]
        [SerializeField] private float maxRange = 50f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float damageInterval = 0.1f;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Visual")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float beamWidth = 0.05f;
        [SerializeField] private Color beamColor = Color.red;
        [SerializeField] private Material beamMaterial;

        [Header("Effects")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject muzzleEffectPrefab;
        [SerializeField] private AudioClip beamSound;
        [SerializeField] private bool loopBeamSound = true;

        private Transform origin;
        private AudioSource audioSource;
        private float lastDamageTime = 0f;
        private GameObject currentHitEffect;
        private GameObject currentMuzzleEffect;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            SetupLineRenderer();
            lineRenderer.enabled = false;
        }

        private void SetupLineRenderer()
        {
            lineRenderer.startWidth = beamWidth;
            lineRenderer.endWidth = beamWidth;
            lineRenderer.positionCount = 2;

            if (beamMaterial != null)
            {
                lineRenderer.material = beamMaterial;
            }

            lineRenderer.startColor = beamColor;
            lineRenderer.endColor = beamColor;

            // Use world space for positions
            lineRenderer.useWorldSpace = true;
        }

        /// <summary>
        /// Start firing the laser from the specified origin point
        /// </summary>
        public void StartLaser(Transform originTransform)
        {
            if (IsActive)
            {
                return;
            }

            origin = originTransform;
            IsActive = true;
            lineRenderer.enabled = true;

            // Spawn muzzle effect
            if (muzzleEffectPrefab != null && origin != null)
            {
                currentMuzzleEffect = Instantiate(muzzleEffectPrefab, origin.position, origin.rotation, origin);
            }

            // Play beam sound
            if (beamSound != null)
            {
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                audioSource.clip = beamSound;
                audioSource.loop = loopBeamSound;
                audioSource.Play();
            }

            Debug.Log("[LaserBeam] Laser started");
        }

        /// <summary>
        /// Stop firing the laser
        /// </summary>
        public void StopLaser()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            lineRenderer.enabled = false;

            // Stop sound
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            // Destroy effects
            if (currentHitEffect != null)
            {
                Destroy(currentHitEffect);
                currentHitEffect = null;
            }

            if (currentMuzzleEffect != null)
            {
                Destroy(currentMuzzleEffect);
                currentMuzzleEffect = null;
            }

            Debug.Log("[LaserBeam] Laser stopped");
        }

        private void Update()
        {
            if (!IsActive || origin == null)
            {
                return;
            }

            UpdateBeam();
        }

        private void UpdateBeam()
        {
            Vector3 startPos = origin.position;
            Vector3 direction = origin.forward;

            // Raycast to find hit point
            RaycastHit hit;
            Vector3 endPos;
            bool didHit = Physics.Raycast(startPos, direction, out hit, maxRange, hitLayers);

            if (didHit)
            {
                endPos = hit.point;

                // Update or spawn hit effect
                if (hitEffectPrefab != null)
                {
                    if (currentHitEffect == null)
                    {
                        currentHitEffect = Instantiate(hitEffectPrefab);
                    }

                    currentHitEffect.transform.position = hit.point;
                    currentHitEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
                }

                // Deal damage
                if (Time.time >= lastDamageTime + damageInterval)
                {
                    DealDamage(hit);
                    lastDamageTime = Time.time;
                }
            }
            else
            {
                endPos = startPos + direction * maxRange;

                // Destroy hit effect if no longer hitting
                if (currentHitEffect != null)
                {
                    Destroy(currentHitEffect);
                    currentHitEffect = null;
                }
            }

            // Update line renderer positions
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }

        private void DealDamage(RaycastHit hit)
        {
            // Try to find Enemy component
            Ennemy enemy = hit.collider.GetComponent<Ennemy>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
                Debug.Log($"[LaserBeam] Damaged enemy: {damage} damage");
                return;
            }

            // Try to find Nexus component (player base)
            Nexus nexus = hit.collider.GetComponent<Nexus>();
            if (nexus != null)
            {
                // Don't damage the player's own nexus!
                return;
            }

            // Add more damage receiver types here as needed
        }

        private void OnDestroy()
        {
            if (currentHitEffect != null)
            {
                Destroy(currentHitEffect);
            }

            if (currentMuzzleEffect != null)
            {
                Destroy(currentMuzzleEffect);
            }
        }

        /// <summary>
        /// Set beam color at runtime
        /// </summary>
        public void SetBeamColor(Color color)
        {
            beamColor = color;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        /// <summary>
        /// Set beam width at runtime
        /// </summary>
        public void SetBeamWidth(float width)
        {
            beamWidth = width;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }

        /// <summary>
        /// Set damage per tick
        /// </summary>
        public void SetDamage(float dmg)
        {
            damage = dmg;
        }
    }
}
