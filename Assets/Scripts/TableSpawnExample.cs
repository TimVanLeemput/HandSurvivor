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
        /// Spawn objects randomly on table surface
        /// </summary>
        public void SpawnObjectsOnTable()
        {
            if (!ValidateSpawn()) return;

            hasSpawned = true;
            Bounds bounds = tableCalibration.GetTableBounds();

            for (int i = 0; i < numberOfObjects; i++)
            {
                Vector3 pos = GetRandomPosition(bounds);
                Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                GameObject obj = Instantiate(spawnPrefab, pos, rot);
                obj.name = $"{spawnPrefab.name}_{i}";
                Debug.Log($"[TableSpawnExample] Spawned {obj.name} at {pos}");
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
