using UnityEngine;
using UnityEngine.Events;
using HandSurvivor.Utilities;

[RequireComponent(typeof(Collider))]
public class ColliderTriggerEvents : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onTriggerEnter;
    public UnityEvent<Collider> onTriggerStay;
    public UnityEvent<Collider> onTriggerExit;

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

    [Header("Finger Bone Filtering")]
    [SerializeField] private bool checkFingerBone = false;
    [SerializeField] private OVRSkeleton.BoneId requiredBone;

    public enum HandFilter
    {
        Any,
        MainHand,
        OffHand
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldTrigger(other))
        {
            onTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (ShouldTrigger(other))
        {
            onTriggerStay?.Invoke(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ShouldTrigger(other))
        {
            onTriggerExit?.Invoke(other);
        }
    }

    protected virtual bool ShouldTrigger(Collider other)
    {
        if (checkHand && !CheckHandFilter(other.gameObject))
            return false;

        if (checkComponent && !CheckComponent(other.gameObject))
            return false;

        if (checkTag && !CheckTagFilter(other.gameObject))
            return false;

        if (checkLayer && !CheckLayerFilter(other.gameObject))
            return false;

        if (checkFingerBone && !CheckFingerBone(other.gameObject))
            return false;

        return true;
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

    private bool CheckFingerBone(GameObject obj)
    {
        OVRBone bone = obj.GetComponent<OVRBone>();
        if (bone == null)
            return false;

        return bone.Id == requiredBone;
    }
}