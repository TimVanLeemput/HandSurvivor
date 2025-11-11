using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;

public class PokeScaleController : MonoBehaviour
{
    public enum ScaleAxis
    {
        X,
        Y,
        Z
    }

    [Header("References")]
    [SerializeField] private PokeInteractable pokeInteractable;
    [SerializeField] private Transform targetTransform;

    [Header("Settings")]
    [SerializeField] private ScaleAxis scaleAxis = ScaleAxis.Z;
    [SerializeField] private float maxPokeDistance = 0.1f;
    [SerializeField] private float minScale = 0f;
    [SerializeField] private float maxScale = 1f;

    [Header("Events")]
    [SerializeField] private UnityEvent onMinScaleReached;

    private Vector3 initialScale;
    private bool wasSelecting = false;
    private bool hasReachedMinScale = false;

    private void Start()
    {
        if (targetTransform == null)
        {
            targetTransform = transform;
        }

        initialScale = targetTransform.localScale;
    }

    private void Update()
    {
        if (pokeInteractable == null)
        {
            return;
        }

        bool isInteracting = pokeInteractable.InteractorViews.Count() > 0;

        if (isInteracting)
        {
            UpdateScale();
            wasSelecting = true;
        }
        else if (wasSelecting)
        {
            ResetScale();
            wasSelecting = false;
        }
    }

    private void UpdateScale()
    {
        PokeInteractor interactor = pokeInteractable.InteractorViews.FirstOrDefault() as PokeInteractor;
        if (interactor == null || pokeInteractable.SurfacePatch == null)
        {
            return;
        }

        Vector3 interactorPosition = interactor.Origin;

        if (pokeInteractable.SurfacePatch.ClosestSurfacePoint(interactorPosition, out SurfaceHit hit))
        {
            Vector3 surfaceNormal = hit.Normal;
            Vector3 toInteractor = interactorPosition - hit.Point;

            // Distance from surface (positive = in front, negative = past surface)
            float distanceFromSurface = Vector3.Dot(toInteractor, surfaceNormal);

            // Calculate travel distance from hover start position
            // When at maxPokeDistance away: travelDistance = 0 → scale = maxScale
            // When at surface: travelDistance = maxPokeDistance → scale = minScale
            float travelDistance = maxPokeDistance - distanceFromSurface;
            float pokeProgress = Mathf.Clamp01(travelDistance / maxPokeDistance);
            float targetScale = Mathf.Lerp(maxScale, minScale, pokeProgress);

            ApplyScale(targetScale);

            // Check if min scale reached
            if (Mathf.Approximately(pokeProgress, 1f) && !hasReachedMinScale)
            {
                hasReachedMinScale = true;
                onMinScaleReached?.Invoke();
            }
            else if (!Mathf.Approximately(pokeProgress, 1f))
            {
                hasReachedMinScale = false;
            }
        }
    }

    private void ApplyScale(float scaleValue)
    {
        Vector3 newScale = targetTransform.localScale;

        switch (scaleAxis)
        {
            case ScaleAxis.X:
                newScale.x = initialScale.x * scaleValue;
                break;
            case ScaleAxis.Y:
                newScale.y = initialScale.y * scaleValue;
                break;
            case ScaleAxis.Z:
                newScale.z = initialScale.z * scaleValue;
                break;
        }

        targetTransform.localScale = newScale;
    }

    private void ResetScale()
    {
        targetTransform.localScale = initialScale;
        hasReachedMinScale = false;
    }
}
