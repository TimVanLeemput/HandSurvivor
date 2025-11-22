using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages reservation of positions on a ring around the Nexus so that enemies do not stack.
/// </summary>
public class NexusRingPositionsManager : MonoBehaviour
{
    public static NexusRingPositionsManager Instance { get; private set; }

    [Tooltip("Number of discrete slots on the ring.")] [SerializeField]
    private int slotsCount = 32;

    [Tooltip("Max distance used by NavMesh.SamplePosition when validating slot positions.")] [SerializeField]
    private float navMeshSampleMaxDistance = 1.5f;

    [Header("Auto-spacing")] [SerializeField]
    private bool computeSlotsFromMinDistance = true;

    [SerializeField, Min(0.1f)] private float minDistanceBetweenSlots = 2.0f;
    [SerializeField] private int minSlots = 4;
    [SerializeField] private int maxSlots = 128;

    private Transform _nexusTransform;
    private bool[] _occupied; // true if reserved

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _nexusTransform = Nexus.Instance != null ? Nexus.Instance.transform : null;

        if (!computeSlotsFromMinDistance)
        {
            int initialCount = Mathf.Clamp(slotsCount, minSlots, maxSlots);
            initialCount = Mathf.Max(initialCount, 4);
            _occupied = new bool[initialCount];
        }
        else
        {
            // Lazy-initialize using the first requested radius in AcquireNearestPosition
            _occupied = null;
        }
    }

    /// <summary>
    /// Attempts to acquire the nearest free slot on a ring of given radius around the Nexus.
    /// </summary>
    /// <param name="requesterPosition">World position of the requester to find nearest slot.</param>
    /// <param name="radius">Ring radius in world units.</param>
    /// <param name="slotIndex">Returned slot index if acquired, otherwise -1.</param>
    /// <param name="position">Returned world position on/near the NavMesh.</param>
    /// <returns>True if a slot was successfully reserved and a position returned.</returns>
    public bool AcquireNearestPosition(Vector3 requesterPosition, float radius, out int slotIndex, out Vector3 position)
    {
        slotIndex = -1;
        position = Vector3.zero;
        if (_nexusTransform == null)
            return false;

        // Lazy initialize slots based on requested radius if enabled
        if (_occupied == null)
        {
            int countComputed;
            if (computeSlotsFromMinDistance)
            {
                float circumference = Mathf.Max(0.001f, 2f * Mathf.PI * Mathf.Max(0.001f, radius));
                countComputed = Mathf.FloorToInt(circumference / Mathf.Max(0.1f, minDistanceBetweenSlots));
                countComputed = Mathf.Clamp(countComputed, Mathf.Max(4, minSlots),
                    Mathf.Max(Mathf.Max(4, minSlots), maxSlots));
            }
            else
            {
                countComputed = Mathf.Clamp(slotsCount, Mathf.Max(4, minSlots),
                    Mathf.Max(Mathf.Max(4, minSlots), maxSlots));
            }

            _occupied = new bool[countComputed];
        }

        int bestIndex = -1;
        float bestScore = float.PositiveInfinity;
        Vector3 nexusPos = _nexusTransform.position;
        Vector3 toRequester = requesterPosition - nexusPos;
        if (toRequester.sqrMagnitude < 0.0001f)
        {
            // Avoid undefined angle: use forward
            toRequester = Vector3.forward;
        }

        float baseAngle = Mathf.Atan2(toRequester.z, toRequester.x); // x-z plane

        // Evaluate all slots: choose the free one with smallest angular distance to requester direction
        int count = _occupied.Length;
        for (int i = 0; i < count; i++)
        {
            if (_occupied[i]) continue;
            float angle = IndexToAngle(i, count);
            float d = Mathf.DeltaAngle(baseAngle * Mathf.Rad2Deg, angle * Mathf.Rad2Deg);
            float score = Mathf.Abs(d);
            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        if (bestIndex == -1)
        {
            // No free slots
            return false;
        }

        Vector3 rawPos = AngleToWorldPos(bestIndex, radius, nexusPos);
        Vector3 navPos = rawPos;
        if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, navMeshSampleMaxDistance, NavMesh.AllAreas))
        {
            navPos = hit.position;
        }

        _occupied[bestIndex] = true;
        slotIndex = bestIndex;
        position = navPos;
        return true;
    }

    /// <summary>
    /// Releases a previously acquired slot.
    /// </summary>
    public void ReleasePosition(int slotIndex)
    {
        if (_occupied == null) return;
        if (slotIndex < 0 || slotIndex >= _occupied.Length) return;
        _occupied[slotIndex] = false;
    }

    private static float IndexToAngle(int index, int count)
    {
        // Angle in radians around x-z plane (0 along +x, increasing counter-clockwise)
        return (index / (float)count) * Mathf.PI * 2f;
    }

    private Vector3 AngleToWorldPos(int index, float radius, Vector3 center)
    {
        int count = _occupied != null && _occupied.Length > 0 ? _occupied.Length : Mathf.Max(4, slotsCount);
        float angle = IndexToAngle(index, count);
        return AngleToWorldPos(angle, radius, center);
    }

    private static Vector3 AngleToWorldPos(float angle, float radius, Vector3 center)
    {
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        return new Vector3(center.x + x, center.y, center.z + z);
    }

    private static Vector3 AngleToWorldPos(int index, int count, float radius, Vector3 center)
    {
        float angle = IndexToAngle(index, count);
        return AngleToWorldPos(angle, radius, center);
    }
}