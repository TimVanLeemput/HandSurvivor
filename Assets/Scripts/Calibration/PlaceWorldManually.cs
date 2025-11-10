using System;
using MyBox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlaceWorldManually : MonoBehaviour
{
    public GameObject BottomLeft = null;
    public GameObject TopRight = null;
    public float WorldScaleMultiplier = 100f;
    public float PlaneUpdateSpeed = 0.05f;
    [ReadOnly] public Vector3 minScale = Vector3.zero;
    [SerializeField] private bool lockScale = false;
    [SerializeField] private bool keepAspectRatio = false;
    [SerializeField] private AspectRatioScaleLock aspectRatioScaleLock = AspectRatioScaleLock.UseMinimum;
    private Vector3 initialScale;
    [SerializeField] private bool canUpdatePlane = true;
    private bool _wasDistanceRespected = true;

    public enum AspectRatioScaleLock
    {
        UseMinimum,
        UseMaximum,
        LockToWidth,
        LockToHeight
    }

    [Header("Events")]
    public UnityEvent minDistanceBetweenGrabbableAnchorsNotRespected;
    public UnityEvent minDistanceBetweenGrabbableAnchorsRespected;
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

        bool isDistanceRespected = currentWidth >= minWidth && currentHeight >= minHeight;

        if (isDistanceRespected != _wasDistanceRespected)
        {
            if (isDistanceRespected)
            {
                minDistanceBetweenGrabbableAnchorsRespected?.Invoke();
            }
            else
            {
                minDistanceBetweenGrabbableAnchorsNotRespected?.Invoke();
            }

            _wasDistanceRespected = isDistanceRespected;
        }
    }

    private void UpdatePlane()
    {
        if (BottomLeft == null || !canUpdatePlane) return;

        if (lockScale)
        {
            MeshFilter _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter != null && _meshFilter.sharedMesh != null)
            {
                Bounds meshBounds = _meshFilter.sharedMesh.bounds;
                Vector3 offset = meshBounds.center - meshBounds.min;
                transform.position = BottomLeft.transform.position + offset;
            }
            return;
        }

        if (TopRight == null) return;

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

            float scaleX = (width / meshBounds.size.x) * WorldScaleMultiplier;
            float scaleZ = (height / meshBounds.size.z) * WorldScaleMultiplier;

            if (keepAspectRatio)
            {
                float uniformScale = aspectRatioScaleLock switch
                {
                    AspectRatioScaleLock.UseMinimum => Mathf.Min(scaleX, scaleZ),
                    AspectRatioScaleLock.UseMaximum => Mathf.Max(scaleX, scaleZ),
                    AspectRatioScaleLock.LockToWidth => scaleX,
                    AspectRatioScaleLock.LockToHeight => scaleZ,
                    _ => Mathf.Min(scaleX, scaleZ)
                };

                transform.localScale = new Vector3(
                    uniformScale,
                    initialScale.y * WorldScaleMultiplier,
                    uniformScale
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    scaleX,
                    initialScale.y * WorldScaleMultiplier,
                    scaleZ
                );
            }
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdatePlane));
    }
}