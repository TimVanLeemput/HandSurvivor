using System;
using UnityEngine;

/// <summary>
/// Represents the root of a game world/level that needs to be positioned and scaled.
/// Used by SceneLoader to fit the world onto the calibrated table surface.
/// </summary>
public class WorldReference : MonoBehaviour
{
    public Renderer WorldPlaneRenderer = null;
    public Vector3 TopLeftCorner = Vector3.zero;
    public Vector3 TopRightCorner = Vector3.zero;
    public Vector3 BottomLeftCorner = Vector3.zero;
    public Vector3 BottomRightCorner = Vector3.zero;

    private void Start()
    {
        GetWorldPlaneBounds();
    }

    public void GetWorldPlaneBounds()
    {
        if (WorldPlaneRenderer != null)
        {
            Bounds bounds = WorldPlaneRenderer.bounds;
            TopLeftCorner = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z); // Top-left
            TopRightCorner = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z); // Top-right
            BottomLeftCorner = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z); // Bottom-left
            BottomRightCorner = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z); // Bottom-right
        }
    }
}