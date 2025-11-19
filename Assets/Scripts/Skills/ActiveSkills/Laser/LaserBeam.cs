using MyBox;
using Unity.VisualScripting;
using UnityEngine;
using HandSurvivor.Stats;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Visual laser beam with damage dealing capabilities
    /// Uses LineRenderer for beam visualization and raycasting for hit detection
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Beam Settings")] [SerializeField]
        private float maxRange = 50f;

        [Tooltip("Damage value set at runtime by LaserActiveSkill from ActiveSkillData")]
        private float damage = 0f; // This value is set by ActiveSkillData
        private string skillId = ""; // Set at runtime for stats tracking
        [SerializeField] private float damageInterval = 0.1f;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Visual")] [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float beamWidth = 0.05f;

        [Header("Raycast Box")]
        [SerializeField] private Vector3 raycastBoxHalfExtents = new Vector3(0.025f, 0.025f, 0.01f);
        [Tooltip("Auto-sync raycast box width with beam width")]
        [SerializeField] private bool autoSyncBoxWithBeamWidth = true;

        [Header("Surface Contact")]
        [Tooltip("Layers that block the beam visually (e.g., Environment, ground)")]
        [SerializeField] private LayerMask blockingSurfacesLayerMask = 0;
        [Tooltip("Event emitted continuously while laser contacts blocking surface (for VFX trail)")]
        [SerializeField] private UnityEngine.Events.UnityEvent<RaycastHit> onSurfaceContact;
        [Tooltip("Interval between surface contact event emissions (seconds)")]
        [SerializeField] private float surfaceContactEventInterval = 0.05f;

        [Header("Retraction")]
        [SerializeField] private float retractionDuration = 0.25f;

        [Header("Effects")] [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject muzzleEffectPrefab;
        [SerializeField] private GameObject beamOriginParticlesPrefab;
        [SerializeField] private AudioClip[] beamSounds;
        [SerializeField] private bool loopBeamSound = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugBox = false;
        [SerializeField] private bool showDebugBoxInVR = false;
        [SerializeField] private bool showGizmoBoxInEditor = true;

        private Transform origin;
        private AudioSource audioSource;
        private float lastDamageTime = 0f;
        private GameObject currentHitEffect;
        private GameObject currentMuzzleEffect;
        private GameObject currentBeamOriginParticles;
        private ParticleSystem beamOriginParticleSystem;
        private GameObject debugBoxVisual;
        private float laserStartTime;
        private float laserDuration;
        private ParticleSystem.MainModule particlesMainModule;
        private float lastSurfaceContactEventTime = 0f;

        private RaycastHit[] cachedHits;
        private int frameSkipCounter = 0;
        private const int RAYCAST_FRAME_SKIP = 2;

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

            // Use world space for positions
            lineRenderer.useWorldSpace = true;
        }

        /// <summary>
        /// Start firing the laser from the specified origin point
        /// </summary>
        [ButtonMethod]
        public void StartLaser(Transform originTransform, float duration = 2f)
        {
            if (IsActive)
            {
                return;
            }

            origin = originTransform;
            IsActive = true;
            lineRenderer.enabled = true;
            laserStartTime = Time.time;
            laserDuration = duration;

            // Spawn beam origin particles
            if (beamOriginParticlesPrefab != null && origin != null)
            {
                currentBeamOriginParticles = Instantiate(beamOriginParticlesPrefab, origin.position, origin.rotation, origin);
                beamOriginParticleSystem = currentBeamOriginParticles.GetComponent<ParticleSystem>();
                if (beamOriginParticleSystem != null)
                {
                    particlesMainModule = beamOriginParticleSystem.main;
                }
            }

            // Spawn muzzle effect
            if (muzzleEffectPrefab != null && origin != null)
            {
                currentMuzzleEffect = Instantiate(muzzleEffectPrefab, origin.position, origin.rotation, origin);
            }

            // Play beam sound
            if (beamSounds != null && beamSounds.Length > 0)
            {
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                audioSource.loop = loopBeamSound;
                audioSource.PlayRandomClipWithPitch(beamSounds);
            }

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
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

            // Destroy beam origin particles
            if (currentBeamOriginParticles != null)
            {
                Destroy(currentBeamOriginParticles);
                currentBeamOriginParticles = null;
                beamOriginParticleSystem = null;
            }

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

            if (currentBeamOriginParticles != null)
            {
                Destroy(currentBeamOriginParticles);
                currentBeamOriginParticles = null;
                beamOriginParticleSystem = null;
            }

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
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

            // Calculate retraction factor (1.0 = full length, 0.0 = fully retracted)
            float timeSinceStart = Time.time - laserStartTime;
            float retractionFactor = 1f;
            float particleAlpha = 1f;

            // Start retraction during the last retractionDuration seconds
            float retractionStartTime = laserDuration - retractionDuration;
            if (timeSinceStart >= retractionStartTime)
            {
                float retractionTime = timeSinceStart - retractionStartTime;
                retractionFactor = 1f - Mathf.Clamp01(retractionTime / retractionDuration);
                particleAlpha = retractionFactor;
            }

            // Update beam origin particles alpha and scale (position/rotation inherited from parent)
            if (beamOriginParticleSystem != null)
            {
                currentBeamOriginParticles.transform.localScale = Vector3.one * particleAlpha;

                Color startColor = particlesMainModule.startColor.color;
                startColor.a = particleAlpha;
                particlesMainModule.startColor = startColor;
            }

            // Apply retraction factor to max range
            float effectiveRange = maxRange * retractionFactor;

            // Use BoxCastAll with size matching the line renderer width to hit multiple targets
            Vector3 boxHalfExtents = autoSyncBoxWithBeamWidth
                ? new Vector3(beamWidth * 0.5f, beamWidth * 0.5f, raycastBoxHalfExtents.z)
                : raycastBoxHalfExtents;

            RaycastHit[] hits;
            if (frameSkipCounter <= 0)
            {
                hits = Physics.BoxCastAll(startPos, boxHalfExtents, direction, origin.rotation, effectiveRange, hitLayers);
                cachedHits = hits;
                frameSkipCounter = RAYCAST_FRAME_SKIP;
            }
            else
            {
                hits = cachedHits ?? new RaycastHit[0];
                frameSkipCounter--;
            }

            // Separate blocking surfaces from damage targets
            RaycastHit closestBlockingSurface = default;
            float blockingDistance = effectiveRange;
            bool hasBlockingSurface = false;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
            {
                Debug.Log($"[LaserBeam] BoxCast detected {hits.Length} total hits. blockingSurfacesLayerMask={blockingSurfacesLayerMask.value}");
            }

            // Find closest blocking surface
            for (int i = 0; i < hits.Length; i++)
            {
                int hitLayer = hits[i].collider.gameObject.layer;
                int hitLayerMask = 1 << hitLayer;
                bool isBlockingSurface = (hitLayerMask & blockingSurfacesLayerMask) != 0;

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                {
                    Debug.Log($"[LaserBeam] Hit '{hits[i].collider.name}' on layer {hitLayer} ({LayerMask.LayerToName(hitLayer)}), distance={hits[i].distance:F2}, isBlockingSurface={isBlockingSurface}");
                }

                if (isBlockingSurface)
                {
                    if (!hasBlockingSurface || hits[i].distance < blockingDistance)
                    {
                        closestBlockingSurface = hits[i];
                        blockingDistance = hits[i].distance;
                        hasBlockingSurface = true;

                        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        {
                            Debug.Log($"[LaserBeam] New closest blocking surface: '{hits[i].collider.name}' at {hits[i].distance:F2}m");
                        }
                    }
                }
            }

            // Use blocking surface to limit effective range
            float visualRange = hasBlockingSurface ? blockingDistance : effectiveRange;

            // Find closest hit for visual feedback (legacy, now uses blocking surface)
            RaycastHit closestHit = hasBlockingSurface ? closestBlockingSurface : default;
            float closestDistance = visualRange;
            bool didHit = hasBlockingSurface;

            // If no blocking surface, check for any hit for legacy hit effect
            if (!hasBlockingSurface && hits.Length > 0)
            {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                closestHit = hits[0];
                closestDistance = closestHit.distance;
                didHit = true;
            }

            // Debug visualization
            if (showDebugBox)
            {
                DebugDrawBoxCast(startPos, boxHalfExtents, direction, origin.rotation, closestDistance, didHit ? Color.red : Color.green);
            }
            if (showDebugBoxInVR)
            {
                UpdateDebugBoxVisual(startPos, boxHalfExtents, direction, origin.rotation, closestDistance);
            }

            if (didHit)
            {
                // Use closest hit for beam endpoint
                endPos = closestHit.point;

                // Spawn/update hit effect at closest hit
                if (hitEffectPrefab != null)
                {
                    if (currentHitEffect == null)
                        currentHitEffect = Instantiate(hitEffectPrefab);

                    currentHitEffect.transform.position = closestHit.point;
                    currentHitEffect.transform.eulerAngles =
                        new Vector3(0f, currentHitEffect.transform.eulerAngles.y, 0f);
                }

                // Emit surface contact event for VFX trail (only for blocking surfaces)
                if (hasBlockingSurface && onSurfaceContact != null)
                {
                    if (Time.time >= lastSurfaceContactEventTime + surfaceContactEventInterval)
                    {
                        onSurfaceContact.Invoke(closestBlockingSurface);
                        lastSurfaceContactEventTime = Time.time;

                        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        {
                            Debug.Log($"[LaserBeam] Surface contact event emitted for '{closestBlockingSurface.collider.name}' at {closestBlockingSurface.point}");
                        }
                    }
                }

                // Deal damage to ALL hits with timing check (excludes blocking surfaces)
                if (Time.time >= lastDamageTime + damageInterval)
                {
                    foreach (RaycastHit hit in hits)
                    {
                        // Skip blocking surfaces for damage
                        bool isBlockingSurface = ((1 << hit.collider.gameObject.layer) & blockingSurfacesLayerMask) != 0;
                        if (!isBlockingSurface)
                        {
                            DealDamage(hit);
                        }
                    }
                    lastDamageTime = Time.time;
                }
            }
            else
            {
                endPos = startPos + direction * effectiveRange;

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
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
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
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning($"[LaserBeam] Enemy '{enemy.name}' has no RagdollController component!");
                }

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[LaserBeam] Damaged enemy: {enemy.name}");
                enemy.TakeDamage((int)damage);

                // Track damage for stats
                if (PlayerStatsManager.Instance != null && !string.IsNullOrEmpty(skillId))
                    PlayerStatsManager.Instance.RecordDamage(skillId, damage, enemy.name);

                // Show damage number for each enemy hit
                if (DamageNumberManager.Instance != null)
                {
                    DamageNumberManager.Instance.SpawnDamageNumber((int)damage, hit.point);
                }
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

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

        public void SetSkillId(string id)
        {
            skillId = id;
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

            if (!showDebugBoxInVR || !IsActive)
            {
                debugBoxVisual.SetActive(false);
                return;
            }

            // Position box at midpoint of cast
            Vector3 centerPoint = startPos + direction * (distance * 0.5f);
            debugBoxVisual.transform.position = centerPoint;
            debugBoxVisual.transform.rotation = orientation;

            // Scale to match box dimensions (cube is 1x1x1, so scale = size)
            Vector3 fullExtents = halfExtents * 2f;
            fullExtents.z = distance; // Length of the cast
            debugBoxVisual.transform.localScale = fullExtents;

            debugBoxVisual.SetActive(true);
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

        private void OnDrawGizmosSelected()
        {
            if (!showGizmoBoxInEditor) return;

            // Show box at origin when active
            if (IsActive && origin != null)
            {
                Vector3 boxHalfExtents = autoSyncBoxWithBeamWidth
                    ? new Vector3(beamWidth * 0.5f, beamWidth * 0.5f, raycastBoxHalfExtents.z)
                    : raycastBoxHalfExtents;

                Gizmos.color = Color.yellow;
                Gizmos.matrix = Matrix4x4.TRS(origin.position, origin.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
            }
            // Show preview at transform when inactive
            else if (transform != null)
            {
                Vector3 boxHalfExtents = autoSyncBoxWithBeamWidth
                    ? new Vector3(beamWidth * 0.5f, beamWidth * 0.5f, raycastBoxHalfExtents.z)
                    : raycastBoxHalfExtents;

                Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // Semi-transparent yellow
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
            }
        }
    }
}