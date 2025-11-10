using System;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceWorldManually : MonoBehaviour
{
    public GameObject BottomLeft = null;
    public GameObject TopRight = null;
    public float WorldScaleMultiplier = 100f;
    public float PlaneUpdateSpeed = 0.05f;
    [ReadOnly] public Vector3 minScale = Vector3.zero;
    private Vector3 initialScale;
    [SerializeField] private bool canUpdatePlane = true;
    private void Start()
    {
        initialScale = transform.localScale;
        minScale = initialScale;

        InitializeCornerPositions();

        InvokeRepeating(nameof(UpdatePlane), 0f, PlaneUpdateSpeed);
    }

    private void Update()
    {
        ConstrainCornerPositions();

        if (transform.localScale.x < minScale.x)
        {
            transform.localScale = minScale;
        }

        if (transform.localScale.y < minScale.y)
        {
            transform.localScale = minScale;
        }

        if (transform.localScale.z < minScale.z)
        {
            transform.localScale = minScale;
        }
    }

    public void SetCanUpdatePlane(bool canUpdate)
    {
        canUpdatePlane = canUpdate;
    }

    private void InitializeCornerPositions()
    {
        if (BottomLeft == null || TopRight == null) return;

        Renderer planeRenderer = GetComponent<Renderer>();
        if (planeRenderer == null) return;

        Bounds bounds = planeRenderer.bounds;

        BottomLeft.transform.position = bounds.min;
        TopRight.transform.position = bounds.max;
    }

    private void ConstrainCornerPositions()
    {
        if (BottomLeft == null || TopRight == null) return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        Bounds meshBounds = meshFilter.sharedMesh.bounds;

        float minWidth = (minScale.x * meshBounds.size.x) / WorldScaleMultiplier;
        float minHeight = (minScale.z * meshBounds.size.z) / WorldScaleMultiplier;

        Vector3 bottomLeftPos = BottomLeft.transform.position;
        Vector3 topRightPos = TopRight.transform.position;

        float currentWidth = Mathf.Abs(topRightPos.x - bottomLeftPos.x);
        float currentHeight = Mathf.Abs(topRightPos.z - bottomLeftPos.z);

        if (currentWidth < minWidth || currentHeight < minHeight)
        {
            Vector3 center = (bottomLeftPos + topRightPos) / 2f;

            float adjustedWidth = Mathf.Max(currentWidth, minWidth);
            float adjustedHeight = Mathf.Max(currentHeight, minHeight);

            BottomLeft.transform.position = new Vector3(
                center.x - adjustedWidth / 2f,
                bottomLeftPos.y,
                center.z - adjustedHeight / 2f
            );

            TopRight.transform.position = new Vector3(
                center.x + adjustedWidth / 2f,
                topRightPos.y,
                center.z + adjustedHeight / 2f
            );
        }
    }

    private void UpdatePlane()
    {
        if (BottomLeft == null || TopRight == null || !canUpdatePlane) return;

        Vector3 bottomLeftPos = BottomLeft.transform.position;
        Vector3 topRightPos = TopRight.transform.position;

        Vector3 planeCenter = (bottomLeftPos + topRightPos) / 2f;
        float width = Mathf.Abs(topRightPos.x - bottomLeftPos.x);
        float height = Mathf.Abs(topRightPos.z - bottomLeftPos.z);

        transform.position = planeCenter;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds meshBounds = meshFilter.sharedMesh.bounds;

            transform.localScale = new Vector3(
                (width / meshBounds.size.x) * WorldScaleMultiplier,
                initialScale.y * WorldScaleMultiplier,
                (height / meshBounds.size.z) * WorldScaleMultiplier
            );
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdatePlane));
    }
}