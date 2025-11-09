using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace HandSurvivor
{
    /// <summary>
    /// Safe table spawn manager. Spawns objects on a calibrated table using TableCalibrationManager.
    /// Supports random, circle, and grid patterns. Fully safe for delayed calibration.
    /// </summary>
    public class TableSpawnExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TableCalibrationManager tableCalibration;
        [SerializeField, Tooltip("Prefab to spawn on the table")] private GameObject spawnPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private int numberOfObjects = 5;
        [SerializeField, Tooltip("Height above table surface")] private float spawnHeight = 0.05f;
        [SerializeField, Tooltip("Minimum distance from table edge")] private float edgeMargin = 0.1f;

        private bool hasSpawned = false;

        private void Start()
        {
            if (tableCalibration == null)
                tableCalibration = FindFirstObjectByType<TableCalibrationManager>();

            if (tableCalibration == null)
            {
                Debug.LogError("[TableSpawnExample] TableCalibrationManager not found!");
                return;
            }

            // Subscribe safely
            tableCalibration.OnTableCalibrated.AddListener(OnTableCalibrated);

            // If already calibrated at start, spawn immediately
            if (tableCalibration.IsCalibrated && !hasSpawned)
            {
                SpawnObjectsOnTable();
            }
        }

        private void OnDestroy()
        {
            if (tableCalibration != null)
                tableCalibration.OnTableCalibrated.RemoveListener(OnTableCalibrated);
        }

        private void OnTableCalibrated(MRUKAnchor table)
        {
            Debug.Log($"[TableSpawnExample] Table calibrated: {table.name}");
            if (!hasSpawned)
                SpawnObjectsOnTable();
        }

        /// <summary>
        /// Spawn objects at table corners and center (for debugging)
        /// Spawns exactly 5 objects: 4 corners + 1 center, no offset
        /// Uses local space transform to properly handle OpenXR rotation
        /// </summary>
        public void SpawnObjectsOnTable()
        {
            if (!ValidateSpawn()) return;

            hasSpawned = true;

            MRUKAnchor tableAnchor = tableCalibration.CalibratedTable;
            if (tableAnchor == null || !tableAnchor.PlaneRect.HasValue)
            {
                Debug.LogError("[TableSpawnExample] Table anchor or PlaneRect not available!");
                return;
            }

            // Get table dimensions from PlaneRect (local space)
            Rect planeRect = tableAnchor.PlaneRect.Value;
            float halfWidth = planeRect.width * 0.5f;
            float halfHeight = planeRect.height * 0.5f;

            // Define the 5 spawn positions in LOCAL space (relative to table anchor center)
            // This ensures proper rotation handling with OpenXR
            Vector3[] localPositions = new Vector3[]
            {
                // 4 corners in local space (X, Y-up, Z)
                new Vector3(-halfWidth, spawnHeight, -halfHeight), // Bottom-left corner
                new Vector3(halfWidth, spawnHeight, -halfHeight),  // Bottom-right corner
                new Vector3(-halfWidth, spawnHeight, halfHeight),  // Top-left corner
                new Vector3(halfWidth, spawnHeight, halfHeight),   // Top-right corner

                // Center
                new Vector3(0, spawnHeight, 0)                      // Center
            };

            string[] positionNames = { "BottomLeft", "BottomRight", "TopLeft", "TopRight", "Center" };

            // Create transform matrix with +90 X rotation to match MRUK PlaneRect orientation
            Quaternion correctionRotation = Quaternion.Euler(90f, 0f, 0f);
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(correctionRotation);
            Matrix4x4 transformMatrix = tableAnchor.transform.localToWorldMatrix * rotationMatrix;

            // Transform local positions to world space using the corrected matrix
            for (int i = 0; i < localPositions.Length; i++)
            {
                Vector3 worldPosition = transformMatrix.MultiplyPoint3x4(localPositions[i]);
                Quaternion worldRotation = tableAnchor.transform.rotation * correctionRotation;

                GameObject obj = Instantiate(spawnPrefab, worldPosition, worldRotation);
                obj.name = $"{spawnPrefab.name}_{positionNames[i]}";
                Debug.Log($"[TableSpawnExample] Spawned {obj.name} at world {worldPosition} (local {localPositions[i]})");
            }
        }

        private bool ValidateSpawn()
        {
            if (!tableCalibration.IsCalibrated)
            {
                Debug.LogWarning("[TableSpawnExample] Cannot spawn - table not calibrated!");
                return false;
            }

            if (spawnPrefab == null)
            {
                Debug.LogWarning("[TableSpawnExample] Spawn prefab not assigned!");
                return false;
            }

            return true;
        }

        private Vector3 GetRandomPosition(Bounds bounds)
        {
            float minX = bounds.min.x + edgeMargin;
            float maxX = bounds.max.x - edgeMargin;
            float minZ = bounds.min.z + edgeMargin;
            float maxZ = bounds.max.z - edgeMargin;

            // Ensure valid ranges
            if (minX > maxX) { float tmp = minX; minX = maxX; maxX = tmp; }
            if (minZ > maxZ) { float tmp = minZ; minZ = maxZ; maxZ = tmp; }

            return new Vector3(
                Random.Range(minX, maxX),
                bounds.center.y + spawnHeight,
                Random.Range(minZ, maxZ)
            );
        }

        /// <summary>
        /// Spawn at table center
        /// </summary>
        public void SpawnAtTableCenter()
        {
            if (!ValidateSpawn()) return;

            Vector3 center = tableCalibration.GetTableCenter();
            center.y += spawnHeight;
            GameObject obj = Instantiate(spawnPrefab, center, Quaternion.identity);
            obj.name = $"{spawnPrefab.name}_Center";
            Debug.Log($"[TableSpawnExample] Spawned at center: {center}");
        }

        /// <summary>
        /// Spawn objects in a circle pattern
        /// </summary>
        public void SpawnInCircle(int count, float radius)
        {
            if (!ValidateSpawn()) return;

            Vector3 center = tableCalibration.GetTableCenter();
            Bounds bounds = tableCalibration.GetTableBounds();
            float maxRadius = Mathf.Min(bounds.extents.x, bounds.extents.z) - edgeMargin;
            radius = Mathf.Min(radius, maxRadius);

            float step = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * step * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, spawnHeight, Mathf.Sin(angle) * radius);
                Vector3 pos = center + offset;
                GameObject obj = Instantiate(spawnPrefab, pos, Quaternion.identity);
                obj.name = $"{spawnPrefab.name}_Circle_{i}";
            }

            Debug.Log($"[TableSpawnExample] Spawned {count} objects in circle (radius {radius})");
        }

        /// <summary>
        /// Spawn objects in a grid pattern
        /// </summary>
        public void SpawnInGrid(int rows, int columns, float spacing)
        {
            if (!ValidateSpawn()) return;

            Vector3 center = tableCalibration.GetTableCenter();
            Bounds bounds = tableCalibration.GetTableBounds();

            float gridWidth = (columns - 1) * spacing;
            float gridDepth = (rows - 1) * spacing;

            if (gridWidth > bounds.size.x - edgeMargin * 2 || gridDepth > bounds.size.z - edgeMargin * 2)
            {
                Debug.LogWarning("[TableSpawnExample] Grid too large for table!");
                return;
            }

            Vector3 start = center - new Vector3(gridWidth / 2f, -spawnHeight, gridDepth / 2f);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Vector3 pos = start + new Vector3(c * spacing, 0, r * spacing);
                    GameObject obj = Instantiate(spawnPrefab, pos, Quaternion.identity);
                    obj.name = $"{spawnPrefab.name}_Grid_R{r}_C{c}";
                }
            }

            Debug.Log($"[TableSpawnExample] Spawned {rows * columns} objects in {rows}x{columns} grid");
        }

        public bool IsPositionOnTable(Vector3 position)
        {
            return tableCalibration.IsCalibrated && tableCalibration.GetTableBounds().Contains(position);
        }

#if UNITY_EDITOR
        [ContextMenu("Test Spawn Random")]
        private void TestSpawnRandom() { if (Application.isPlaying) SpawnObjectsOnTable(); }
        [ContextMenu("Test Spawn Circle")]
        private void TestSpawnCircle() { if (Application.isPlaying) SpawnInCircle(8, 0.3f); }
        [ContextMenu("Test Spawn Grid")]
        private void TestSpawnGrid() { if (Application.isPlaying) SpawnInGrid(3, 3, 0.15f); }
#endif
    }
}
