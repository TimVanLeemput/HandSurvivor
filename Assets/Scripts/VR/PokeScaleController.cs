using System.Linq;
using UnityEngine;
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

    private Vector3 initialScale;
    private bool wasSelecting = false;

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

        bool isSelecting = pokeInteractable.SelectingInteractors.Count > 0;

        if (isSelecting)
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
        PokeInteractor interactor = pokeInteractable.SelectingInteractors.FirstOrDefault();
        if (interactor == null || pokeInteractable.SurfacePatch == null)
        {
            return;
        }

        float pokeDepth = 0f;

        Vector3 interactorPosition = interactor.Origin;

        if (pokeInteractable.SurfacePatch.ClosestSurfacePoint(interactorPosition, out SurfaceHit hit))
        {
            Vector3 surfaceNormal = hit.Normal;
            Vector3 toInteractor = interactorPosition - hit.Point;

            pokeDepth = Mathf.Max(0, -Vector3.Dot(toInteractor, surfaceNormal));
        }

        float pokeProgress = Mathf.Clamp01(pokeDepth / maxPokeDistance);
        float targetScale = Mathf.Lerp(maxScale, minScale, pokeProgress);

        ApplyScale(targetScale);
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
    }
}
