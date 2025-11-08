using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Meta.XR.MRUtilityKit;

namespace HandSurvivor
{
    /// <summary>
    /// Safely manages table calibration using MRUK on Meta Quest.
    /// Fully crash-proof: waits for MRUK, room, and anchors before calibration.
    /// </summary>
    public class TableCalibrationManager : MonoBehaviour
    {
        [Header("Table Detection Settings")]
        [SerializeField] private float minTableHeight = 0.5f;
        [SerializeField] private float maxTableHeight = 1.2f;
        [SerializeField] private float minTableArea = 0.3f;

        [Header("Calibration Mode")]
        [SerializeField] private bool autoCalibrate = true;
        [SerializeField] private bool allowManualSelection = true;

        [Header("Visual Feedback")]
        [SerializeField] private Material tableSurfaceMaterial;
        [SerializeField] private bool showDebugGizmos = true;

        [Header("Events")]
        public UnityEvent<MRUKAnchor> OnTableCalibrated;
        public UnityEvent OnCalibrationFailed;

        private MRUKAnchor calibratedTable;
        private MRUKRoom currentRoom;
        private List<MRUKAnchor> tableCandidates = new List<MRUKAnchor>();

        public MRUKAnchor CalibratedTable => calibratedTable;
        public bool IsCalibrated => calibratedTable != null;

#if UNITY_EDITOR
        private void OnEnable()
        {
            Debug.LogWarning("[TableCalibration] MRUK only works on Quest. Skipping initialization in Editor.");
        }
#else
        private void OnEnable()
        {
            StartCoroutine(SafeMRUKInitialization());
        }
#endif

        private void OnDisable()
        {
            // Only unsubscribe if MRUK still exists
            if (MRUK.Instance != null)
            {
                UnsubscribeFromMRUKEvents();
            }

            // isInitialized = false;
        }

        private void OnDestroy()
        {
            if (MRUK.Instance != null)
            {
                UnsubscribeFromMRUKEvents();
            }
        }

        /// <summary>
        /// Fully safe MRUK initialization coroutine
        /// </summary>
        private IEnumerator SafeMRUKInitialization()
        {
            // Wait for MRUK.Instance
            while (MRUK.Instance == null)
                yield return null;

            Debug.Log("[TableCalibration] MRUK.Instance is ready. Subscribing to events...");
            SubscribeToMRUKEvents();

            // Wait for the first room to be created
            while (currentRoom == null)
                yield return null;

            Debug.Log($"[TableCalibration] Room created: {currentRoom.name}. Waiting for anchors...");

            // Wait until room anchors are populated
            while (currentRoom.Anchors == null || currentRoom.Anchors.Count == 0)
                yield return null;

            Debug.Log("[TableCalibration] Room anchors populated. Detecting table...");

            if (autoCalibrate)
                DetectAndCalibrateTable();
        }


        private void SubscribeToMRUKEvents()
        {
            if (MRUK.Instance == null) return;

            try
            {
                if (MRUK.Instance.RoomCreatedEvent != null)
                    MRUK.Instance.RoomCreatedEvent.AddListener(OnRoomCreated);

                if (MRUK.Instance.RoomUpdatedEvent != null)
                    MRUK.Instance.RoomUpdatedEvent.AddListener(OnRoomUpdated);
            }
            catch (Exception e)
            {
                Debug.LogError($"[TableCalibration] Failed to subscribe to MRUK events: {e.Message}");
            }
        }

        private void UnsubscribeFromMRUKEvents()
        {
            if (MRUK.Instance == null) return;

            try
            {
                MRUK.Instance.RoomCreatedEvent?.RemoveListener(OnRoomCreated);
                MRUK.Instance.RoomUpdatedEvent?.RemoveListener(OnRoomUpdated);
            }
            catch (Exception e)
            {
                Debug.LogError($"[TableCalibration] Failed to unsubscribe MRUK events: {e.Message}");
            }
        }

        private void OnRoomCreated(MRUKRoom room)
        {
            if (room == null) return;
            currentRoom = room;
            Debug.Log($"[TableCalibration] Room created: {room.name}");
        }

        private void OnRoomUpdated(MRUKRoom room)
        {
            if (room == null) return;
            currentRoom = room;

            if (!IsCalibrated && autoCalibrate)
                DetectAndCalibrateTable();
        }

        /// <summary>
        /// Detect table surfaces and select the best candidate
        /// </summary>
        public void DetectAndCalibrateTable()
        {
            if (MRUK.Instance == null || currentRoom == null || currentRoom.Anchors == null)
            {
                Debug.LogWarning("[TableCalibration] Cannot calibrate: MRUK or room anchors not ready.");
                OnCalibrationFailed?.Invoke();
                return;
            }

            tableCandidates.Clear();

            foreach (var anchor in currentRoom.Anchors)
            {
                if (IsValidTableSurface(anchor))
                    tableCandidates.Add(anchor);
            }

            if (tableCandidates.Count == 0)
            {
                Debug.LogWarning("[TableCalibration] No valid table surfaces found.");
                OnCalibrationFailed?.Invoke();
                return;
            }

            MRUKAnchor bestTable = SelectBestTable(tableCandidates);
            if (bestTable != null)
                CalibrateToTable(bestTable);
            else
                OnCalibrationFailed?.Invoke();
        }

        private bool IsValidTableSurface(MRUKAnchor anchor)
        {
            if (anchor == null || anchor.transform == null)
                return false;

            try
            {
                if (anchor.Label != MRUKAnchor.SceneLabels.TABLE &&
                    anchor.Label != MRUKAnchor.SceneLabels.COUCH &&
                    anchor.Label != MRUKAnchor.SceneLabels.OTHER)
                    return false;

                float height = anchor.transform.position.y;
                if (height < minTableHeight || height > maxTableHeight)
                    return false;

                if (anchor.PlaneRect.HasValue)
                {
                    Rect rect = anchor.PlaneRect.Value;
                    if (rect.width * rect.height < minTableArea)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private MRUKAnchor SelectBestTable(List<MRUKAnchor> candidates)
        {
            const float idealHeight = 0.75f;
            MRUKAnchor best = candidates[0];
            float bestScore = CalculateTableScore(best, idealHeight);

            for (int i = 1; i < candidates.Count; i++)
            {
                if (candidates[i] == null) continue;
                float score = CalculateTableScore(candidates[i], idealHeight);
                if (score > bestScore)
                {
                    best = candidates[i];
                    bestScore = score;
                }
            }

            return best;
        }

        private float CalculateTableScore(MRUKAnchor anchor, float idealHeight)
        {
            if (anchor == null || anchor.transform == null) return 0f;

            float score = 0f;
            float heightDiff = Mathf.Abs(anchor.transform.position.y - idealHeight);
            score += (1f - Mathf.Clamp01(heightDiff / 0.5f)) * 50f;

            if (anchor.PlaneRect.HasValue)
            {
                Rect rect = anchor.PlaneRect.Value;
                float areaScore = Mathf.Clamp01(rect.width * rect.height / 2f);
                score += areaScore * 50f;
            }

            return score;
        }

        public void CalibrateToTable(MRUKAnchor table)
        {
            if (table == null) return;
            calibratedTable = table;

            Debug.Log($"[TableCalibration] Calibrated to table: {table.name}");

            if (tableSurfaceMaterial != null)
            {
                MeshRenderer renderer = table.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                    renderer.material = tableSurfaceMaterial;
            }

            OnTableCalibrated?.Invoke(calibratedTable);
        }
        
        /// <summary>
        /// Returns the bounds of the calibrated table. Always safe to call.
        /// </summary>
        public Bounds GetTableBounds()
        {
            if (!IsCalibrated)
            {
                Debug.LogWarning("[TableCalibration] GetTableBounds called but table is not calibrated!");
                return new Bounds(Vector3.zero, Vector3.one);
            }

            // If the anchor has a PlaneRect, use it
            if (calibratedTable.PlaneRect.HasValue)
            {
                Rect rect = calibratedTable.PlaneRect.Value;
                // PlaneRect is local 2D rectangle on the table plane
                Vector3 size = new Vector3(rect.width, 0.05f, rect.height); // small height for bounds
                return new Bounds(calibratedTable.transform.position, size);
            }

            // Fallback if PlaneRect is null: assume 1x1m centered on anchor
            return new Bounds(calibratedTable.transform.position, new Vector3(1f, 0.05f, 1f));
        }
        
        /// <summary>
        /// Returns the world center position of the calibrated table. Always safe to call.
        /// </summary>
        public Vector3 GetTableCenter()
        {
            if (!IsCalibrated)
            {
                Debug.LogWarning("[TableCalibration] GetTableCenter called but table is not calibrated!");
                return Vector3.zero;
            }

            // If PlaneRect exists, calculate center offset
            if (calibratedTable.PlaneRect.HasValue)
            {
                Rect rect = calibratedTable.PlaneRect.Value;
                // PlaneRect is local to anchor, so center is at half width/height
                Vector3 localCenter = new Vector3(rect.width / 2f, 0, rect.height / 2f);
                return calibratedTable.transform.TransformPoint(localCenter);
            }

            // Fallback: use anchor position
            return calibratedTable.transform.position;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || calibratedTable == null) return;

            try
            {
                Gizmos.color = Color.green;
                DrawTableGizmo(calibratedTable);

                Gizmos.color = Color.yellow;
                foreach (var candidate in tableCandidates)
                {
                    if (candidate != null && candidate != calibratedTable)
                        DrawTableGizmo(candidate);
                }
            }
            catch { }
        }

        private void DrawTableGizmo(MRUKAnchor anchor)
        {
            if (anchor == null || !anchor.PlaneRect.HasValue || anchor.transform == null) return;

            Rect rect = anchor.PlaneRect.Value;
            Vector3 size = new Vector3(rect.width, 0.02f, rect.height);
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = anchor.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = old;
        }
#endif
    }
}
