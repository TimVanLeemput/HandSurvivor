using HandSurvivor;
using UnityEngine;
using UnityEngine.Events;
using HandSurvivor.Utilities;

[RequireComponent(typeof(Collider))]
public class ColliderTriggerEvents : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Events")]
    public UnityEvent onTriggerEnter;
    public UnityEvent<Collider> onTriggerStay;
    public UnityEvent<Collider> onTriggerExit;

    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;

    [Header("Hand Filtering")]
    [SerializeField] private bool checkHand = false;
    [SerializeField] private HandFilter handFilter = HandFilter.Any;

    [Header("Component Filtering")]
    [SerializeField] private bool checkComponent = false;
    [SerializeField] private string requiredComponentName = "";

    [Header("Tag Filtering")]
    [SerializeField] private bool checkTag = false;
    [SerializeField] private string requiredTag = "";

    [Header("Layer Filtering")]
    [SerializeField] private bool checkLayer = false;
    [SerializeField] private LayerMask allowedLayers;

    [Header("GameObject Name Filtering")]
    [SerializeField] private bool checkGameObjectName = false;
    [SerializeField] private string requiredGameObjectName = "";

    public enum HandFilter
    {
        Any,
        MainHand,
        OffHand
    }

    private void Start()
    {
        if (enableDebug)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[ColliderTriggerEvents] START - Settings:\n" +
                     $"  checkHand: {checkHand} | handFilter: {handFilter}\n" +
                     $"  checkComponent: {checkComponent} | requiredComponent: {requiredComponentName}\n" +
                     $"  checkTag: {checkTag} | requiredTag: {requiredTag}\n" +
                     $"  checkLayer: {checkLayer} | allowedLayers: {allowedLayers.value}\n" +
                     $"  checkGameObjectName: {checkGameObjectName} | requiredGameObjectName: {requiredGameObjectName}", gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enableDebug) if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
     Debug.Log($"[ColliderTriggerEvents] ENTER triggered by {other.gameObject.name}", other.gameObject);

        if (ShouldTrigger(other, "ENTER"))
        {
            if (enableDebug) if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
     Debug.Log($"[ColliderTriggerEvents] ✓ ENTER event invoked for {other.gameObject.name}", other.gameObject);
            onTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (enableDebug) if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
     Debug.Log($"[ColliderTriggerEvents] STAY triggered by {other.gameObject.name}", other.gameObject);

        if (ShouldTrigger(other, "STAY"))
        {
            if (enableDebug) if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
     Debug.Log($"[ColliderTriggerEvents] ✓ STAY event invoked for {other.gameObject.name}", other.gameObject);
            onTriggerStay?.Invoke(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enableDebug) if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
     Debug.Log($"[ColliderTriggerEvents] EXIT triggered by {other.gameObject.name}", other.gameObject);

        if (ShouldTrigger(other, "EXIT"))
        {
            if (enableDebug) if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
     Debug.Log($"[ColliderTriggerEvents] ✓ EXIT event invoked for {other.gameObject.name}", other.gameObject);
            onTriggerExit?.Invoke(other);
        }
    }

    protected virtual bool ShouldTrigger(Collider other, string eventType)
    {
        if (enableDebug)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[ColliderTriggerEvents] [{eventType}] Checking filters for {other.gameObject.name}:\n" +
                     $"  Hand Check: {checkHand} | Component Check: {checkComponent} | Tag Check: {checkTag} | Layer Check: {checkLayer} | GameObject Name Check: {checkGameObjectName}", other.gameObject);
        }

        if (checkHand)
        {
            bool handResult = CheckHandFilter(other.gameObject);
            if (enableDebug)
            {
                HandType? handType = TargetHandFinder.GetHandTypeFromObject(other.gameObject);
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[ColliderTriggerEvents] [{eventType}] Hand filter ({handFilter}): {(handResult ? "✓ PASS" : "✗ FAIL")} | Detected hand: {(handType.HasValue ? handType.Value.ToString() : "None")}", other.gameObject);
            }
            if (!handResult) return false;
        }

        if (checkComponent)
        {
            bool componentResult = CheckComponent(other.gameObject);
            if (enableDebug)
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[ColliderTriggerEvents] [{eventType}] Component filter ({requiredComponentName}): {(componentResult ? "✓ PASS" : "✗ FAIL")}", other.gameObject);
            if (!componentResult) return false;
        }

        if (checkTag)
        {
            bool tagResult = CheckTagFilter(other.gameObject);
            if (enableDebug)
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[ColliderTriggerEvents] [{eventType}] Tag filter ({requiredTag}): {(tagResult ? "✓ PASS" : "✗ FAIL")} | Current tag: {other.gameObject.tag}", other.gameObject);
            if (!tagResult) return false;
        }

        if (checkLayer)
        {
            bool layerResult = CheckLayerFilter(other.gameObject);
            if (enableDebug)
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[ColliderTriggerEvents] [{eventType}] Layer filter: {(layerResult ? "✓ PASS" : "✗ FAIL")} | Current layer: {LayerMask.LayerToName(other.gameObject.layer)} ({other.gameObject.layer})", other.gameObject);
            if (!layerResult) return false;
        }

        if (checkGameObjectName)
        {
            bool nameResult = CheckGameObjectName(other.gameObject);
            if (enableDebug)
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[ColliderTriggerEvents] [{eventType}] GameObject name filter ({requiredGameObjectName}): {(nameResult ? "✓ PASS" : "✗ FAIL")} | Current name: {other.gameObject.name}", other.gameObject);
            if (!nameResult) return false;
        }

        if (enableDebug)
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[ColliderTriggerEvents] [{eventType}] ✓ ALL FILTERS PASSED for {other.gameObject.name}", other.gameObject);

        return true;
    }

    protected virtual bool ShouldTrigger(Collider other)
    {
        return ShouldTrigger(other, "UNKNOWN");
    }

    private bool CheckHandFilter(GameObject obj)
    {
        switch (handFilter)
        {
            case HandFilter.MainHand:
                return TargetHandFinder.IsMainHand(obj);
            case HandFilter.OffHand:
                return TargetHandFinder.IsOffHand(obj);
            case HandFilter.Any:
                return TargetHandFinder.GetHandTypeFromObject(obj) != null;
            default:
                return false;
        }
    }

    private bool CheckComponent(GameObject obj)
    {
        if (string.IsNullOrEmpty(requiredComponentName))
            return true;

        Component component = obj.GetComponent(requiredComponentName);
        return component != null;
    }

    private bool CheckTagFilter(GameObject obj)
    {
        if (string.IsNullOrEmpty(requiredTag))
            return true;

        return obj.CompareTag(requiredTag);
    }

    private bool CheckLayerFilter(GameObject obj)
    {
        return ((1 << obj.layer) & allowedLayers) != 0;
    }

    private bool CheckGameObjectName(GameObject obj)
    {
        if (string.IsNullOrEmpty(requiredGameObjectName))
            return true;

        return obj.name.Contains(requiredGameObjectName);
    }
}