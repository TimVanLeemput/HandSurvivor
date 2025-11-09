using UnityEngine;

/// <summary>
/// Represents the root of a game world/level that needs to be positioned and scaled.
/// Used by SceneLoader to fit the world onto the calibrated table surface.
/// </summary>
public class WorldReference : MonoBehaviour
{
    [Header("Bounds Settings")]
    [SerializeField]
    [Tooltip("Manually define world bounds (leave at zero to auto-calculate)")]
    private Vector3 manualBounds = Vector3.zero;

    [SerializeField]
    [Tooltip("Include inactive children when calculating bounds")]
    private bool includeInactiveChildren = false;

    [SerializeField]
    [Tooltip("Show debug gizmo for world bounds")]
    private bool showDebugGizmo = true;

    private Bounds cachedBounds;
    private bool boundsCalculated = false;

    /// <summary>
    /// Gets the bounds of this world in local space
    /// </summary>
    public Bounds GetWorldBounds()
    {
        // Use manual bounds if specified
        if (manualBounds != Vector3.zero)
        {
            return new Bounds(Vector3.zero, manualBounds);
        }

        // Return cached bounds if already calculated
        if (boundsCalculated)
        {
            return cachedBounds;
        }

        // Calculate bounds from all child renderers and colliders
        cachedBounds = CalculateBounds();
        boundsCalculated = true;

        return cachedBounds;
    }

    /// <summary>
    /// Recalculates the bounds (useful if world changes at runtime)
    /// </summary>
    public void RecalculateBounds()
    {
        boundsCalculated = false;
        cachedBounds = CalculateBounds();
        boundsCalculated = true;
    }

    /// <summary>
    /// Calculates bounds by combining all child renderers and colliders
    /// </summary>
    private Bounds CalculateBounds()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool boundsInitialized = false;

        // Get all renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);
        foreach (Renderer renderer in renderers)
        {
            if (!boundsInitialized)
            {
                bounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        // Also check colliders if no renderers found
        if (!boundsInitialized)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(includeInactiveChildren);
            foreach (Collider collider in colliders)
            {
                if (!boundsInitialized)
                {
                    bounds = collider.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }
        }

        // If still no bounds, use a default 1x1x1 bounds
        if (!boundsInitialized)
        {
            Debug.LogWarning($"[WorldReference] No renderers or colliders found on {gameObject.name}. Using default bounds.");
            bounds = new Bounds(transform.position, Vector3.one);
        }

        // Convert to local space relative to this transform
        Bounds localBounds = new Bounds(
            transform.InverseTransformPoint(bounds.center),
            bounds.size
        );

        Debug.Log($"[WorldReference] Calculated bounds for {gameObject.name}: Size={localBounds.size}, Center={localBounds.center}");

        return localBounds;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmo) return;

        Bounds bounds = GetWorldBounds();

        // Draw bounds in world space
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
#endif
}
