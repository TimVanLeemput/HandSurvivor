using MyBox;
using Unity.VisualScripting;
using UnityEngine;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Visual laser beam with damage dealing capabilities
    /// Uses LineRenderer for beam visualization and raycasting for hit detection
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
        [Header("Beam Settings")] [SerializeField]
        private float maxRange = 50f;

        [SerializeField] private float damage = 10f;
        [SerializeField] private float damageInterval = 0.1f;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Visual")] [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float beamWidth = 0.05f;
        [SerializeField] private Color beamColor = Color.red;
        [SerializeField] private Material beamMaterial;

        [Header("Effects")] [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject muzzleEffectPrefab;
        [SerializeField] private AudioClip beamSound;
        [SerializeField] private bool loopBeamSound = true;

        private Transform origin;
        private AudioSource audioSource;
        private float lastDamageTime = 0f;
        private float lastDamageNumberTime = 0f;
        private float damageNumberInterval = 0.3f; // Show damage number every 0.3s instead of every tick
        private GameObject currentHitEffect;
        private GameObject currentMuzzleEffect;
        private GameObject debugBoxVisual;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            SetupLineRenderer();
            lineRenderer.enabled = false;
            CreateDebugBoxVisual();
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
        [ButtonMethod]
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
        [ButtonMethod]
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
            if (origin == null || lineRenderer == null) return;

            Vector3 startPos = origin.position;
            Vector3 direction = origin.forward;
            Vector3 endPos;

            // Use BoxCast with size matching the line renderer width
            Vector3 boxHalfExtents = new Vector3(beamWidth * 0.5f, beamWidth * 0.5f, 0.01f);
            RaycastHit hit;
            bool didHit = Physics.BoxCast(startPos, boxHalfExtents, direction, out hit, origin.rotation, maxRange, hitLayers);

            // Debug visualization
            float debugDistance = didHit ? hit.distance : maxRange;
            DebugDrawBoxCast(startPos, boxHalfExtents, direction, origin.rotation, debugDistance, didHit ? Color.red : Color.green);
            UpdateDebugBoxVisual(startPos, boxHalfExtents, direction, origin.rotation, debugDistance);

            if (didHit)
            {
                // Check if hit object has the correct tag
                endPos = hit.point;

                // Spawn/update hit effect
                if (hitEffectPrefab != null)
                {
                    if (currentHitEffect == null)
                        currentHitEffect = Instantiate(hitEffectPrefab);

                    currentHitEffect.transform.position = hit.point;
                    currentHitEffect.transform.eulerAngles =
                        new Vector3(0f, currentHitEffect.transform.eulerAngles.y, 0f);
                }

                // Deal damage with timing check
                if (Time.time >= lastDamageTime + damageInterval)
                {
                    DealDamage(hit);
                    lastDamageTime = Time.time;
                }
            }
            else
            {
                endPos = startPos + direction * maxRange;

                if (currentHitEffect != null)
                {
                    Destroy(currentHitEffect);
                    currentHitEffect = null;
                }
            }

            // Update line renderer
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }

        private void DealDamage(RaycastHit hit)
        {
            Debug.LogWarning($"[LaserBeam] Enemy {hit.transform.gameObject.name} trying to deal damage!");
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                RagdollController ragdoll = enemy.GetComponent<RagdollController>();
                if (ragdoll != null)
                {
                    ragdoll.SetRagdoll(true);
                }
                else
                {
                    Debug.LogWarning($"[LaserBeam] Enemy '{enemy.name}' has no RagdollController component!");
                }

                Debug.Log($"[LaserBeam] Damaged enemy: {enemy.name}");
                enemy.TakeDamage((int)damage);

                // Show damage number (throttled to avoid spam)
                if (Time.time >= lastDamageNumberTime + damageNumberInterval)
                {
                    if (DamageNumberManager.Instance != null)
                    {
                        Vector3 damageNumberPosition = hit.point + Vector3.up * 0.3f;
                        DamageNumberManager.Instance.SpawnDamageNumber((int)damage, damageNumberPosition);
                        lastDamageNumberTime = Time.time;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[LaserBeam] Hit object '{hit.collider.name}' has no Enemy component");
            }
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

        private void CreateDebugBoxVisual()
        {
            debugBoxVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugBoxVisual.name = "LaserBoxCastDebug";
            debugBoxVisual.transform.SetParent(transform);

            // Make it semi-transparent
            Renderer renderer = debugBoxVisual.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0f, 1f, 0f, 0.3f); // Semi-transparent green
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha blend
            mat.renderQueue = 3000;
            renderer.material = mat;

            // Remove collider
            Destroy(debugBoxVisual.GetComponent<Collider>());

            debugBoxVisual.SetActive(false);
        }

        private void UpdateDebugBoxVisual(Vector3 startPos, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float distance)
        {
            if (debugBoxVisual == null) return;

            // Position box at midpoint of cast
            Vector3 centerPoint = startPos + direction * (distance * 0.5f);
            debugBoxVisual.transform.position = centerPoint;
            debugBoxVisual.transform.rotation = orientation;

            // Scale to match box dimensions (cube is 1x1x1, so scale = size)
            Vector3 fullExtents = halfExtents * 2f;
            fullExtents.z = distance; // Length of the cast
            debugBoxVisual.transform.localScale = fullExtents;

            debugBoxVisual.SetActive(IsActive);
        }

        /// <summary>
        /// Debug visualization for BoxCast
        /// </summary>
        private void DebugDrawBoxCast(Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float distance, Color color)
        {
            Vector3 endPoint = origin + direction * distance;

            // Draw box at start
            DrawDebugBox(origin, halfExtents, orientation, color);

            // Draw box at end
            DrawDebugBox(endPoint, halfExtents, orientation, color);

            // Draw lines connecting the boxes
            Vector3[] startCorners = GetBoxCorners(origin, halfExtents, orientation);
            Vector3[] endCorners = GetBoxCorners(endPoint, halfExtents, orientation);

            for (int i = 0; i < 8; i++)
            {
                Debug.DrawLine(startCorners[i], endCorners[i], color);
            }
        }

        private void DrawDebugBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, Color color)
        {
            Vector3[] corners = GetBoxCorners(center, halfExtents, orientation);

            // Draw bottom face
            Debug.DrawLine(corners[0], corners[1], color);
            Debug.DrawLine(corners[1], corners[2], color);
            Debug.DrawLine(corners[2], corners[3], color);
            Debug.DrawLine(corners[3], corners[0], color);

            // Draw top face
            Debug.DrawLine(corners[4], corners[5], color);
            Debug.DrawLine(corners[5], corners[6], color);
            Debug.DrawLine(corners[6], corners[7], color);
            Debug.DrawLine(corners[7], corners[4], color);

            // Draw vertical edges
            Debug.DrawLine(corners[0], corners[4], color);
            Debug.DrawLine(corners[1], corners[5], color);
            Debug.DrawLine(corners[2], corners[6], color);
            Debug.DrawLine(corners[3], corners[7], color);
        }

        private Vector3[] GetBoxCorners(Vector3 center, Vector3 halfExtents, Quaternion orientation)
        {
            Vector3[] corners = new Vector3[8];

            Vector3 right = orientation * Vector3.right * halfExtents.x;
            Vector3 up = orientation * Vector3.up * halfExtents.y;
            Vector3 forward = orientation * Vector3.forward * halfExtents.z;

            corners[0] = center - right - up - forward; // Bottom front left
            corners[1] = center + right - up - forward; // Bottom front right
            corners[2] = center + right - up + forward; // Bottom back right
            corners[3] = center - right - up + forward; // Bottom back left
            corners[4] = center - right + up - forward; // Top front left
            corners[5] = center + right + up - forward; // Top front right
            corners[6] = center + right + up + forward; // Top back right
            corners[7] = center - right + up + forward; // Top back left

            return corners;
        }
    }
}