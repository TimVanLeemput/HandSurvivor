using System.Collections;
using UnityEngine;
using HandSurvivor.Core.Passive;
using HandSurvivor.Upgrades;
using MyBox;

public class XPGrabber : MonoBehaviour, IUpgradeable
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Base Properties")]
    [SerializeField] private float baseRadius = 1f;
    [SerializeField] private float collectDuration = 0.4f;
    [SerializeField] private LayerMask collisionLayer = -1;

    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugSphere = false;
    [SerializeField] private Transform debugSphereTransform;

    [Header("Upgrade Tracking")]
    [SerializeField,ReadOnly]private float rangeMultiplier = 1f;
    private SphereCollider sphereCollider;
    private MeshRenderer debugMeshRenderer;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            // Store initial radius as base
            baseRadius = sphereCollider.radius;
        }

        // Get debug visualization components from child object
        if (debugSphereTransform != null)
        {
            debugMeshRenderer = debugSphereTransform.GetComponent<MeshRenderer>();
        }

        UpdateDebugVisualization();
    }

    private void OnValidate()
    {
        // UpdateDebugVisualization();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object's layer is in the collision layer mask
        if (((1 << other.gameObject.layer) & collisionLayer) == 0)
        {
            return;
        }

        StartCoroutine(CollectCoroutine(other));
    }
    
    private IEnumerator CollectCoroutine(Collider other)
    {
        Transform dropletTransform = other.transform;
        XPDroplet droplet = other.GetComponent<XPDroplet>();
        
        other.GetComponent<Collider>().enabled = false;
        float elapsed = 0f;

        while (elapsed < collectDuration && dropletTransform != null)
        {
            elapsed += Time.deltaTime;

            float remaining = Mathf.Max(collectDuration - elapsed, 0.0001f);

            // On recalcule la direction vers le XPGrabber à chaque frame
            Vector3 toTarget = transform.position - dropletTransform.position;

            // Vitesse nécessaire pour atteindre la cible dans le temps restant
            Vector3 velocity = toTarget / remaining;

            dropletTransform.position += velocity * Time.deltaTime;

            yield return null;
        }

        // On s’assure que le droplet finit exactement sur le XPGrabber
        if (dropletTransform != null)
            dropletTransform.position = transform.position;

        // Callback de collecte + destruction
        droplet.OnDropLetCollected();
        yield return null;
        Destroy(droplet.gameObject);
    }

    #region IUpgradeable Implementation

    public string GetUpgradeableId()
    {
        return "xp_grabber";
    }

    public void ApplyPassiveUpgrade(PassiveUpgradeData upgrade)
    {
        if (upgrade.type == PassiveType.RangeIncrease)
        {
            float increasePercent = upgrade.value / 100f;
            rangeMultiplier += increasePercent;
            UpdateRadius();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


                Debug.Log($"[XPGrabber] Range increased by {upgrade.value}%. New multiplier: {rangeMultiplier:F2}, New radius: {sphereCollider.radius:F2}");
        }
        else
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogWarning($"[XPGrabber] Passive type '{upgrade.type}' not supported. Only RangeIncrease is valid.");
        }
    }

    private void UpdateRadius()
    {
        if (sphereCollider != null)
        {
            sphereCollider.radius = baseRadius * rangeMultiplier;
        }

        UpdateDebugVisualization();
    }

    private void UpdateDebugVisualization()
    {
        if (debugMeshRenderer != null)
        {
            debugMeshRenderer.enabled = showDebugSphere;
        }

        if (showDebugSphere && debugSphereTransform != null)
        {
            // Unity sphere primitive has diameter of 1, so to match collider radius R,
            // we need child scale = R * 2 (to get diameter)
            float currentRadius = baseRadius * rangeMultiplier;
            float diameter = currentRadius * 2f;

            debugSphereTransform.localScale = new Vector3(diameter, diameter, diameter);
        }
    }


    #endregion
}
