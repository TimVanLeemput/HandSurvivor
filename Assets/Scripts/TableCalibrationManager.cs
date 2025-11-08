using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Meta.XR.MRUtilityKit;

namespace HandSurvivor
{
    /// <summary>
    /// Manages the detection and calibration of table/desk surfaces for AR gameplay.
    /// Uses Meta's MR Utility Kit to identify horizontal surfaces at table height.
    /// </summary>
    public class TableCalibrationManager : MonoBehaviour
    {
        [Header("Table Detection Settings")]
        [SerializeField]
        [Tooltip("Minimum height (in meters) for a surface to be considered a table")]
        private float minTableHeight = 0.2f;

        [SerializeField]
        [Tooltip("Maximum height (in meters) for a surface to be considered a table")]
        private float maxTableHeight = 1.3f;

        [SerializeField]
        [Tooltip("Minimum surface area (in square meters) for a valid table")]
        private float minTableArea = 0.15f;

        [Header("Calibration Mode")]
        [SerializeField]
        [Tooltip("Automatically select the best table candidate on scene load")]
        private bool autoCalibrate = true;

        [SerializeField]
        [Tooltip("Allow manual table selection if multiple candidates exist")]
        private bool allowManualSelection = true;

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("Material to apply to the selected table surface")]
        private Material tableSurfaceMaterial;

        [SerializeField]
        [Tooltip("Show debug gizmos for detected surfaces")]
        private bool showDebugGizmos = true;

        [Header("Events")]
        public UnityEvent<MRUKAnchor> OnTableCalibrated;
        public UnityEvent OnCalibrationFailed;

        // Current calibrated table
        private MRUKAnchor calibratedTable;
        private MRUKRoom currentRoom;
        private List<MRUKAnchor> tableCandidates = new List<MRUKAnchor>();

        public MRUKAnchor CalibratedTable => calibratedTable;
        public bool IsCalibrated => calibratedTable != null;

        private void Start()
        {
            // Subscribe to MRUK room creation events
            if (MRUK.Instance != null)
            {
                MRUK.Instance.RoomCreatedEvent.AddListener(OnRoomCreated);
                MRUK.Instance.RoomUpdatedEvent.AddListener(OnRoomUpdated);
            }
            else
            {
                Debug.LogError("[TableCalibration] MRUK instance not found in scene! Please add MRUK component.");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (MRUK.Instance != null)
            {
                MRUK.Instance.RoomCreatedEvent.RemoveListener(OnRoomCreated);
                MRUK.Instance.RoomUpdatedEvent.RemoveListener(OnRoomUpdated);
            }
        }

        /// <summary>
        /// Called when MRUK detects or creates a room
        /// </summary>
        private void OnRoomCreated(MRUKRoom room)
        {
            Debug.Log($"[TableCalibration] Room created: {room.name}");
            currentRoom = room;

            if (autoCalibrate)
            {
                DetectAndCalibrateTable();
            }
        }

        /// <summary>
        /// Called when room data is updated
        /// </summary>
        private void OnRoomUpdated(MRUKRoom room)
        {
            Debug.Log($"[TableCalibration] Room updated: {room.name}");

            // Re-calibrate if we don't have a table or if user requests it
            if (!IsCalibrated && autoCalibrate)
            {
                DetectAndCalibrateTable();
            }
        }

        /// <summary>
        /// Detects all potential table surfaces and selects the best candidate
        /// </summary>
        public void DetectAndCalibrateTable()
        {
            tableCandidates.Clear();

            if (MRUK.Instance == null || currentRoom == null)
            {
                Debug.LogWarning("[TableCalibration] Cannot calibrate: MRUK or room not available");
                OnCalibrationFailed?.Invoke();
                return;
            }

            // Get all anchors in the current room
            List<MRUKAnchor> roomAnchors = currentRoom.Anchors;

            Debug.Log($"[TableCalibration] ===== SCANNING ROOM =====");
            Debug.Log($"[TableCalibration] Total anchors in room: {roomAnchors.Count}");

            // First, find the floor to calculate relative heights
            MRUKAnchor floorAnchor = null;
            foreach (MRUKAnchor anchor in roomAnchors)
            {
                if (anchor != null && anchor.Label == MRUKAnchor.SceneLabels.FLOOR)
                {
                    floorAnchor = anchor;
                    Debug.Log($"[TableCalibration] Found floor at Y={anchor.transform.position.y}m");
                    break;
                }
            }

            float floorHeight = floorAnchor != null ? floorAnchor.transform.position.y : 0f;

            foreach (MRUKAnchor anchor in roomAnchors)
            {
                if (anchor == null)
                {
                    Debug.LogWarning("[TableCalibration] Found null anchor in room!");
                    continue;
                }

                float absoluteHeight = anchor.transform.position.y;
                float relativeHeight = absoluteHeight - floorHeight;

                Debug.Log($"[TableCalibration] Checking anchor: {anchor.name}, Label: {anchor.Label}, Absolute Y: {absoluteHeight:F2}m, Height from floor: {relativeHeight:F2}m");

                if (IsValidTableSurface(anchor, floorHeight))
                {
                    tableCandidates.Add(anchor);
                    Debug.Log($"[TableCalibration] ✓ ACCEPTED as table candidate: {anchor.name}");
                }
            }

            Debug.Log($"[TableCalibration] ===== SCAN COMPLETE =====");
            Debug.Log($"[TableCalibration] Total table candidates found: {tableCandidates.Count}");

            if (tableCandidates.Count == 0)
            {
                Debug.LogWarning("[TableCalibration] No valid table surfaces found!");
                OnCalibrationFailed?.Invoke();
                return;
            }

            // Select the best table (prefer larger surfaces closer to standard desk height ~0.75m)
            MRUKAnchor bestTable = SelectBestTable(tableCandidates, floorHeight);
            CalibrateToTable(bestTable);
        }

        /// <summary>
        /// Checks if an anchor represents a valid table surface
        /// </summary>
        private bool IsValidTableSurface(MRUKAnchor anchor, float floorHeight)
        {
            // Check if it's a horizontal plane/volume (table, desk, etc.)
            if (anchor.Label != MRUKAnchor.SceneLabels.TABLE &&
                anchor.Label != MRUKAnchor.SceneLabels.COUCH &&
                anchor.Label != MRUKAnchor.SceneLabels.OTHER)
            {
                Debug.LogWarning($"[TableCalibration] ✗ {anchor.name} REJECTED: Wrong label '{anchor.Label}' (need TABLE/COUCH/OTHER)");
                return false;
            }

            // Calculate height relative to floor (actual table height)
            float absoluteHeight = anchor.transform.position.y;
            float relativeHeight = absoluteHeight - floorHeight;

            if (relativeHeight < minTableHeight || relativeHeight > maxTableHeight)
            {
                Debug.LogWarning($"[TableCalibration] ✗ {anchor.name} REJECTED: Height from floor {relativeHeight:F2}m outside range [{minTableHeight}-{maxTableHeight}m]");
                return false;
            }

            // Check surface area if it's a plane
            if (anchor.PlaneRect.HasValue)
            {
                Rect planeRect = anchor.PlaneRect.Value;
                float area = planeRect.width * planeRect.height;

                if (area < minTableArea)
                {
                    Debug.LogWarning($"[TableCalibration] ✗ {anchor.name} REJECTED: Area {area:F2}m² < minimum {minTableArea}m²");
                    return false;
                }

                Debug.LogWarning($"[TableCalibration] {anchor.name} passed all checks: Label={anchor.Label}, Height from floor={relativeHeight:F2}m, Area={area:F2}m²");
            }
            else
            {
                Debug.LogWarning($"[TableCalibration] {anchor.name} passed checks but has no PlaneRect: Label={anchor.Label}, Height from floor={relativeHeight:F2}m");
            }

            return true;
        }

        /// <summary>
        /// Selects the best table from candidates based on size and height
        /// </summary>
        private MRUKAnchor SelectBestTable(List<MRUKAnchor> candidates, float floorHeight)
        {
            const float idealTableHeight = 0.75f; // Standard desk height from floor

            MRUKAnchor bestCandidate = candidates[0];
            float bestScore = CalculateTableScore(bestCandidate, idealTableHeight, floorHeight);

            for (int i = 1; i < candidates.Count; i++)
            {
                float score = CalculateTableScore(candidates[i], idealTableHeight, floorHeight);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidates[i];
                }
            }

            return bestCandidate;
        }

        /// <summary>
        /// Calculates a quality score for a table candidate
        /// </summary>
        private float CalculateTableScore(MRUKAnchor anchor, float idealHeight, float floorHeight)
        {
            float score = 0f;

            // Calculate height relative to floor
            float relativeHeight = anchor.transform.position.y - floorHeight;

            // Prefer surfaces closer to ideal desk height
            float heightDiff = Mathf.Abs(relativeHeight - idealHeight);
            float heightScore = 1f - Mathf.Clamp01(heightDiff / 0.5f); // Max penalty at 0.5m difference
            score += heightScore * 50f;

            // Prefer larger surfaces
            if (anchor.PlaneRect.HasValue)
            {
                Rect planeRect = anchor.PlaneRect.Value;
                float area = planeRect.width * planeRect.height;
                float areaScore = Mathf.Clamp01(area / 2f); // Normalize to ~2 square meters
                score += areaScore * 50f;
            }

            return score;
        }

        /// <summary>
        /// Calibrates the play area to a specific table anchor
        /// </summary>
        public void CalibrateToTable(MRUKAnchor table)
        {
            if (table == null)
            {
                Debug.LogError("[TableCalibration] Cannot calibrate to null table!");
                return;
            }

            calibratedTable = table;

            Debug.LogWarning($"[TableCalibration] Calibrated to table: {table.name} at position {table.transform.position}");

            // Apply visual feedback if material is assigned
            if (tableSurfaceMaterial != null)
            {
                ApplyTableVisualization(table);
            }

            // Invoke calibration success event
            OnTableCalibrated?.Invoke(calibratedTable);
        }

        /// <summary>
        /// Applies visual material to the table surface
        /// </summary>
        private void ApplyTableVisualization(MRUKAnchor table)
        {
            MeshRenderer renderer = table.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = tableSurfaceMaterial;
            }
        }

        /// <summary>
        /// Manually select a specific table from detected candidates
        /// </summary>
        public void SelectTableByIndex(int index)
        {
            if (index < 0 || index >= tableCandidates.Count)
            {
                Debug.LogError($"[TableCalibration] Invalid table index: {index}");
                return;
            }

            CalibrateToTable(tableCandidates[index]);
        }

        /// <summary>
        /// Get world position on the table surface at a given local offset
        /// </summary>
        public Vector3 GetTablePosition(Vector2 localOffset)
        {
            if (!IsCalibrated)
            {
                Debug.LogWarning("[TableCalibration] Cannot get table position - not calibrated!");
                return Vector3.zero;
            }

            // Convert local 2D offset to world position on table surface
            Vector3 worldOffset = calibratedTable.transform.TransformPoint(new Vector3(localOffset.x, 0, localOffset.y));
            return worldOffset;
        }

        /// <summary>
        /// Get the center position of the calibrated table
        /// </summary>
        public Vector3 GetTableCenter()
        {
            if (!IsCalibrated)
            {
                Debug.LogWarning("[TableCalibration] Cannot get table center - not calibrated!");
                return Vector3.zero;
            }

            return calibratedTable.transform.position;
        }

        /// <summary>
        /// Get the bounds of the calibrated table
        /// </summary>
        public Bounds GetTableBounds()
        {
            if (!IsCalibrated)
            {
                Debug.LogWarning("[TableCalibration] Cannot get table bounds - not calibrated!");
                return new Bounds();
            }

            if (calibratedTable.PlaneRect.HasValue)
            {
                Rect rect = calibratedTable.PlaneRect.Value;
                return new Bounds(
                    calibratedTable.transform.position,
                    new Vector3(rect.width, 0.05f, rect.height)
                );
            }

            return new Bounds(calibratedTable.transform.position, Vector3.one);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // Draw calibrated table in green
            if (IsCalibrated && calibratedTable.PlaneRect.HasValue)
            {
                Gizmos.color = Color.green;
                DrawTableGizmo(calibratedTable);
            }

            // Draw table candidates in yellow
            Gizmos.color = Color.yellow;
            foreach (MRUKAnchor candidate in tableCandidates)
            {
                if (candidate != calibratedTable)
                {
                    DrawTableGizmo(candidate);
                }
            }
        }

        private void DrawTableGizmo(MRUKAnchor anchor)
        {
            if (!anchor.PlaneRect.HasValue) return;

            Rect rect = anchor.PlaneRect.Value;
            Vector3 center = anchor.transform.position;
            Vector3 size = new Vector3(rect.width, 0.02f, rect.height);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = anchor.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = oldMatrix;
        }
#endif
    }
}
