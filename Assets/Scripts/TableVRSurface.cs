using UnityEngine;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;

namespace HandSurvivor
{
    /// <summary>
    /// Creates a VR-interactable surface on the calibrated table.
    /// Prevents finger pass-through and provides physical feedback in VR.
    /// Allows users without physical tables to use virtual table boundaries.
    /// </summary>
    public class TableVRSurface : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TableCalibrationManager tableCalibration;

        [Header("Surface Settings")]
        [SerializeField, Tooltip("Make the surface visible (for debugging)")]
        private bool showVisualSurface = true;

        [SerializeField, Tooltip("Material for the table surface visualization")]
        private Material surfaceMaterial;

        [SerializeField, Tooltip("Surface opacity (0 = invisible, 1 = fully visible)")]
        [Range(0f, 1f)]
        private float surfaceOpacity = 0.3f;

        [SerializeField, Tooltip("Thickness of the collision surface")]
        private float surfaceThickness = 0.02f;

        [Header("Interaction Settings")]
        [SerializeField, Tooltip("Enable poke interaction (prevents fingers from passing through)")]
        private bool enablePokeInteraction = true;

        [SerializeField, Tooltip("Enable ray interaction (allows pointing at the table)")]
        private bool enableRayInteraction = true;

        [SerializeField, Tooltip("Physics layer for the table surface")]
        private LayerMask surfaceLayer = 1 << 0; // Default layer

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        private GameObject tableSurfaceObject;
        private MeshRenderer surfaceRenderer;
        private BoxCollider surfaceCollider;
        private PokeInteractable pokeInteractable;
        private RayInteractable rayInteractable;

        public GameObject TableSurfaceObject => tableSurfaceObject;
        public bool IsCreated => tableSurfaceObject != null;

        private void Start()
        {
            if (tableCalibration == null)
            {
                tableCalibration = FindFirstObjectByType<TableCalibrationManager>();
            }

            if (tableCalibration == null)
            {
                Debug.LogError("[TableVRSurface] TableCalibrationManager not found!");
                return;
            }

            // Subscribe to calibration event
            tableCalibration.OnTableCalibrated.AddListener(OnTableCalibrated);

            // If already calibrated, create surface immediately
            if (tableCalibration.IsCalibrated)
            {
                CreateTableSurface(tableCalibration.CalibratedTable);
            }
        }

        private void OnDestroy()
        {
            if (tableCalibration != null)
            {
                tableCalibration.OnTableCalibrated.RemoveListener(OnTableCalibrated);
            }

            DestroyTableSurface();
        }

        private void OnTableCalibrated(MRUKAnchor table)
        {
            Debug.Log($"[TableVRSurface] Table calibrated, creating VR surface");
            CreateTableSurface(table);
        }

        /// <summary>
        /// Creates the interactable table surface
        /// Uses local space parenting for proper OpenXR rotation handling
        /// </summary>
        private void CreateTableSurface(MRUKAnchor tableAnchor)
        {
            if (tableAnchor == null)
            {
                Debug.LogError("[TableVRSurface] Cannot create surface - table anchor is null!");
                return;
            }

            if (!tableAnchor.PlaneRect.HasValue)
            {
                Debug.LogError("[TableVRSurface] Table anchor has no PlaneRect!");
                return;
            }

            // Clean up existing surface if any
            DestroyTableSurface();

            // Get table dimensions from PlaneRect (local space)
            Rect planeRect = tableAnchor.PlaneRect.Value;
            float width = planeRect.width;
            float height = planeRect.height;

            // Create the surface GameObject and parent it to the table anchor
            // This ensures it inherits the correct transform with OpenXR
            tableSurfaceObject = new GameObject("TableVRSurface");
            tableSurfaceObject.transform.SetParent(tableAnchor.transform, false);
            tableSurfaceObject.transform.localPosition = Vector3.zero; // Center on table
            tableSurfaceObject.transform.localRotation = Quaternion.identity;
            tableSurfaceObject.layer = GetLayerFromMask(surfaceLayer);

            // Add mesh components for visualization
            MeshFilter meshFilter = tableSurfaceObject.AddComponent<MeshFilter>();
            surfaceRenderer = tableSurfaceObject.AddComponent<MeshRenderer>();

            // Create a quad mesh matching the table dimensions
            Mesh surfaceMesh = CreateTableMesh(width, height);
            meshFilter.mesh = surfaceMesh;

            // Apply material
            if (surfaceMaterial != null)
            {
                surfaceRenderer.material = new Material(surfaceMaterial);
                SetSurfaceOpacity(surfaceOpacity);
            }
            else
            {
                // Create default semi-transparent material
                Material defaultMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                defaultMat.color = new Color(0.2f, 0.5f, 1f, surfaceOpacity);
                SetMaterialTransparent(defaultMat);
                surfaceRenderer.material = defaultMat;
            }

            surfaceRenderer.enabled = showVisualSurface;

            // Add box collider for physics interaction
            surfaceCollider = tableSurfaceObject.AddComponent<BoxCollider>();
            surfaceCollider.size = new Vector3(width, surfaceThickness, height);
            surfaceCollider.isTrigger = false; // Solid collider to prevent pass-through

            // Add Meta SDK interaction components
            if (enablePokeInteraction)
            {
                AddPokeInteraction();
            }

            if (enableRayInteraction)
            {
                AddRayInteraction();
            }

            Debug.Log($"[TableVRSurface] Created VR surface parented to {tableAnchor.name} with size {width}x{height}m");
            Debug.Log($"[TableVRSurface] Poke: {enablePokeInteraction}, Ray: {enableRayInteraction}, Visible: {showVisualSurface}");
        }

        /// <summary>
        /// Adds poke interaction component (prevents finger pass-through)
        /// </summary>
        private void AddPokeInteraction()
        {
            if (tableSurfaceObject == null) return;

            // Add PokeInteractable component from Meta SDK
            pokeInteractable = tableSurfaceObject.AddComponent<PokeInteractable>();

            // Configure poke settings
            if (surfaceCollider != null)
            {
                // The PokeInteractable will use the BoxCollider automatically
                Debug.Log("[TableVRSurface] Poke interaction enabled - fingers will not pass through table");
            }
        }

        /// <summary>
        /// Adds ray interaction component (allows pointing at table)
        /// </summary>
        private void AddRayInteraction()
        {
            if (tableSurfaceObject == null) return;

            // Add RayInteractable component from Meta SDK
            rayInteractable = tableSurfaceObject.AddComponent<RayInteractable>();

            Debug.Log("[TableVRSurface] Ray interaction enabled - table can be pointed at");
        }

        /// <summary>
        /// Creates a quad mesh for the table surface
        /// </summary>
        private Mesh CreateTableMesh(float width, float depth)
        {
            Mesh mesh = new Mesh();
            mesh.name = "TableSurfaceMesh";

            float halfWidth = width * 0.5f;
            float halfDepth = depth * 0.5f;

            // Vertices (quad facing up)
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-halfWidth, 0, -halfDepth), // Bottom-left
                new Vector3(halfWidth, 0, -halfDepth),  // Bottom-right
                new Vector3(-halfWidth, 0, halfDepth),  // Top-left
                new Vector3(halfWidth, 0, halfDepth)    // Top-right
            };

            // Triangles (two triangles forming a quad)
            int[] triangles = new int[]
            {
                0, 2, 1, // First triangle
                2, 3, 1  // Second triangle
            };

            // UVs
            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            // Normals (pointing up)
            Vector3[] normals = new Vector3[]
            {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.normals = normals;

            return mesh;
        }

        /// <summary>
        /// Sets the material to transparent rendering mode
        /// </summary>
        private void SetMaterialTransparent(Material mat)
        {
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha blend
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        /// <summary>
        /// Updates the surface opacity
        /// </summary>
        public void SetSurfaceOpacity(float opacity)
        {
            surfaceOpacity = Mathf.Clamp01(opacity);

            if (surfaceRenderer != null && surfaceRenderer.material != null)
            {
                Color color = surfaceRenderer.material.color;
                color.a = surfaceOpacity;
                surfaceRenderer.material.color = color;
            }
        }

        /// <summary>
        /// Toggle surface visibility
        /// </summary>
        public void SetSurfaceVisible(bool visible)
        {
            showVisualSurface = visible;
            if (surfaceRenderer != null)
            {
                surfaceRenderer.enabled = visible;
            }
        }

        /// <summary>
        /// Destroys the table surface
        /// </summary>
        public void DestroyTableSurface()
        {
            if (tableSurfaceObject != null)
            {
                Destroy(tableSurfaceObject);
                tableSurfaceObject = null;
                surfaceRenderer = null;
                surfaceCollider = null;
                pokeInteractable = null;
                rayInteractable = null;

                Debug.Log("[TableVRSurface] Table surface destroyed");
            }
        }

        /// <summary>
        /// Manually recreate the surface (useful for runtime updates)
        /// </summary>
        [ContextMenu("Recreate Surface")]
        public void RecreateTableSurface()
        {
            if (tableCalibration != null && tableCalibration.IsCalibrated)
            {
                CreateTableSurface(tableCalibration.CalibratedTable);
            }
            else
            {
                Debug.LogWarning("[TableVRSurface] Cannot recreate surface - table not calibrated!");
            }
        }

        /// <summary>
        /// Converts LayerMask to layer index
        /// </summary>
        private int GetLayerFromMask(LayerMask mask)
        {
            int layerNumber = 0;
            int layer = mask.value;
            while (layer > 1)
            {
                layer >>= 1;
                layerNumber++;
            }
            return layerNumber;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || tableSurfaceObject == null) return;

            // Draw the surface bounds
            Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.5f);
            Gizmos.matrix = tableSurfaceObject.transform.localToWorldMatrix;

            if (surfaceCollider != null)
            {
                Gizmos.DrawWireCube(Vector3.zero, surfaceCollider.size);
            }
        }
#endif
    }
}
