using UnityEngine;
using HandSurvivor.Utilities;

/// <summary>
/// Attaches the XP Grabber prefab to the off-hand at runtime
/// Place this component on the VR rig prefab
/// </summary>
public class XPGrabberAttacher : MonoBehaviour
{
    [Header("XP Grabber Setup")]
    [SerializeField] private GameObject xpGrabberObject;
    [SerializeField] private float attachDelay = 0.5f;

    private void Start()
    {
        if (xpGrabberObject == null)
        {
            Debug.LogError("[XPGrabberAttacher] XP Grabber object not assigned!");
            return;
        }

        Invoke(nameof(AttachXPGrabber), attachDelay);
    }

    private void AttachXPGrabber()
    {
        TargetHandFinder.HandComponents offHand = TargetHandFinder.FindOffHand();

        if (!offHand.IsValid)
        {
            Debug.LogError("[XPGrabberAttacher] Could not find off-hand! Retrying in 1 second...");
            Invoke(nameof(AttachXPGrabber), 1f);
            return;
        }

        // Attach existing grabber to off-hand
        xpGrabberObject.transform.SetParent(offHand.Hand.transform);
        xpGrabberObject.transform.localPosition = Vector3.zero;
        xpGrabberObject.transform.localRotation = Quaternion.identity;

        Debug.Log($"[XPGrabberAttacher] Successfully attached XP Grabber to off-hand ({offHand.HandType})", xpGrabberObject);
    }
}
