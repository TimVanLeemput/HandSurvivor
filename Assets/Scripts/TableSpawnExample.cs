using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace HandSurvivor
{
    /// <summary>
    /// Example script demonstrating how to use TableCalibrationManager for gameplay.
    /// This shows how to spawn objects on the calibrated table surface.
    /// Use this as a reference for implementing wave-based enemy spawning (HAN-23).
    /// </summary>
    public class TableSpawnExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private TableCalibrationManager tableCalibration;

        [SerializeField]
        [Tooltip("Example prefab to spawn on the table (use enemy prefab later)")]
        private GameObject spawnPrefab;

        [Header("Spawn Settings")]
        [SerializeField]
        private int numberOfObjects = 5;

        [SerializeField]
        [Tooltip("Height offset above table surface")]
        private float spawnHeight = 0.05f;

        [SerializeField]
        [Tooltip("Minimum distance from table edge (in meters)")]
        private float edgeMargin = 0.1f;

        private bool hasSpawned = false;

        private void Start()
        {
            // Find TableCalibrationManager if not assigned
            if (tableCalibration == null)
            {
                tableCalibration = FindObjectOfType<TableCalibrationManager>();
            }

            if (tableCalibration == null)
            {
                Debug.LogError("[TableSpawnExample] TableCalibrationManager not found!");
                return;
            }

            // Subscribe to calibration event
            tableCalibration.OnTableCalibrated.AddListener(OnTableCalibrated);

            // If already calibrated, spawn immediately
            if (tableCalibration.IsCalibrated && !hasSpawned)
            {
                SpawnObjectsOnTable();
            }
        }

        private void OnDestroy()
        {
            if (tableCalibration != null)
            {
                tableCalibration.OnTableCalibrated.RemoveListener(OnTableCalibrated);
            }
        }

        /// <summary>
        /// Called when table calibration is complete
        /// </summary>
        private void OnTableCalibrated(MRUKAnchor table)
        {
            Debug.Log($"[TableSpawnExample] Table calibrated: {table.name}");

            if (!hasSpawned)
            {
                SpawnObjectsOnTable();
            }
        }

        /// <summary>
        /// Spawns objects randomly on the calibrated table surface
        /// </summary>
        private void SpawnObjectsOnTable()
        {
            if (!tableCalibration.IsCalibrated)
            {
                Debug.LogWarning("[TableSpawnExample] Cannot spawn - table not calibrated!");
                return;
            }

            if (spawnPrefab == null)
            {
                Debug.LogWarning("[TableSpawnExample] Spawn prefab not assigned!");
                return;
            }

            hasSpawned = true;

            Bounds tableBounds = tableCalibration.GetTableBounds();

            for (int i = 0; i < numberOfObjects; i++)
            {
                Vector3 spawnPosition = GetRandomPositionOnTable(tableBounds);
                Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                GameObject spawnedObject = Instantiate(spawnPrefab, spawnPosition, spawnRotation);
                spawnedObject.name = $"{spawnPrefab.name}_{i}";

                Debug.Log($"[TableSpawnExample] Spawned {spawnedObject.name} at {spawnPosition}");
            }
        }

        /// <summary>
        /// Gets a random position on the table within bounds, respecting edge margins
        /// </summary>
        private Vector3 GetRandomPositionOnTable(Bounds bounds)
        {
            // Calculate safe spawn area (avoid edges)
            float minX = bounds.min.x + edgeMargin;
            float maxX = bounds.max.x - edgeMargin;
            float minZ = bounds.min.z + edgeMargin;
            float maxZ = bounds.max.z - edgeMargin;

            // Clamp to ensure valid range
            minX = Mathf.Min(minX, bounds.center.x);
            maxX = Mathf.Max(maxX, bounds.center.x);
            minZ = Mathf.Min(minZ, bounds.center.z);
            maxZ = Mathf.Max(maxZ, bounds.center.z);

            Vector3 randomPosition = new Vector3(
                Random.Range(minX, maxX),
                bounds.center.y + spawnHeight,
                Random.Range(minZ, maxZ)
            );

            return randomPosition;
        }

        /// <summary>
        /// Example: Spawn object at table center (useful for boss spawns)
        /// </summary>
        public void SpawnAtTableCenter()
        {
            if (!tableCalibration.IsCalibrated)
            {
                Debug.LogWarning("[TableSpawnExample] Cannot spawn - table not calibrated!");
                return;
            }

            Vector3 centerPosition = tableCalibration.GetTableCenter();
            centerPosition.y += spawnHeight;

            GameObject centerObject = Instantiate(spawnPrefab, centerPosition, Quaternion.identity);
            centerObject.name = $"{spawnPrefab.name}_Center";

            Debug.Log($"[TableSpawnExample] Spawned at center: {centerPosition}");
        }

        /// <summary>
        /// Example: Spawn objects in a circle pattern (useful for wave spawning)
        /// </summary>
        public void SpawnInCircle(int count, float radius)
        {
            if (!tableCalibration.IsCalibrated)
            {
                Debug.LogWarning("[TableSpawnExample] Cannot spawn - table not calibrated!");
                return;
            }

            Vector3 center = tableCalibration.GetTableCenter();
            Bounds tableBounds = tableCalibration.GetTableBounds();

            // Clamp radius to table size
            float maxRadius = Mathf.Min(tableBounds.extents.x, tableBounds.extents.z) - edgeMargin;
            radius = Mathf.Min(radius, maxRadius);

            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    spawnHeight,
                    Mathf.Sin(angle) * radius
                );

                Vector3 spawnPosition = center + offset;
                GameObject circleObject = Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);
                circleObject.name = $"{spawnPrefab.name}_Circle_{i}";
            }

            Debug.Log($"[TableSpawnExample] Spawned {count} objects in circle with radius {radius}m");
        }

        /// <summary>
        /// Example: Spawn in a grid pattern (useful for organized waves)
        /// </summary>
        public void SpawnInGrid(int rows, int columns, float spacing)
        {
            if (!tableCalibration.IsCalibrated)
            {
                Debug.LogWarning("[TableSpawnExample] Cannot spawn - table not calibrated!");
                return;
            }

            Vector3 center = tableCalibration.GetTableCenter();
            Bounds tableBounds = tableCalibration.GetTableBounds();

            // Calculate grid dimensions
            float gridWidth = (columns - 1) * spacing;
            float gridDepth = (rows - 1) * spacing;

            // Ensure grid fits on table
            if (gridWidth > tableBounds.size.x - edgeMargin * 2 ||
                gridDepth > tableBounds.size.z - edgeMargin * 2)
            {
                Debug.LogWarning("[TableSpawnExample] Grid too large for table!");
                return;
            }

            // Calculate starting position (bottom-left of grid)
            Vector3 startPosition = center - new Vector3(gridWidth / 2f, -spawnHeight, gridDepth / 2f);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Vector3 offset = new Vector3(col * spacing, 0, row * spacing);
                    Vector3 spawnPosition = startPosition + offset;

                    GameObject gridObject = Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);
                    gridObject.name = $"{spawnPrefab.name}_Grid_R{row}_C{col}";
                }
            }

            Debug.Log($"[TableSpawnExample] Spawned {rows * columns} objects in {rows}x{columns} grid");
        }

        /// <summary>
        /// Example: Check if a position is within table bounds (useful for validation)
        /// </summary>
        public bool IsPositionOnTable(Vector3 position)
        {
            if (!tableCalibration.IsCalibrated)
            {
                return false;
            }

            Bounds tableBounds = tableCalibration.GetTableBounds();
            return tableBounds.Contains(position);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor button to test spawning
        /// </summary>
        [ContextMenu("Test Spawn Objects")]
        private void TestSpawn()
        {
            if (Application.isPlaying)
            {
                SpawnObjectsOnTable();
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to test spawning");
            }
        }

        [ContextMenu("Test Spawn Circle")]
        private void TestSpawnCircle()
        {
            if (Application.isPlaying)
            {
                SpawnInCircle(8, 0.3f);
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to test spawning");
            }
        }

        [ContextMenu("Test Spawn Grid")]
        private void TestSpawnGrid()
        {
            if (Application.isPlaying)
            {
                SpawnInGrid(3, 3, 0.15f);
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to test spawning");
            }
        }
#endif
    }
}
