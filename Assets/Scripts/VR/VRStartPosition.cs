using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class VRStartPosition : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [SerializeField] private bool setRotation = true;
    [SerializeField] private bool overrideHeight = false;

    private void Start()
    {
        StartCoroutine(SetVRStartPosition());
    }

    private IEnumerator SetVRStartPosition()
    {
        OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();

        if (cameraRig == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.LogWarning("VRStartPosition: OVRCameraRig not found in scene");
            yield break;
        }

        OVRManager ovrManager = FindFirstObjectByType<OVRManager>();

        if (ovrManager != null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"VRStartPosition: Original tracking origin = {ovrManager.trackingOriginType}");

            // Switch to Stage to apply rotation
            ovrManager.trackingOriginType = OVRManager.TrackingOrigin.Stage;
            yield return null; // Wait one frame for tracking origin change
        }

        Vector3 newPosition = cameraRig.transform.position;
        newPosition.x = transform.position.x;
        newPosition.z = transform.position.z;

        if (overrideHeight)
        {
            newPosition.y = transform.position.y;
        }

        cameraRig.transform.position = newPosition;

        if (setRotation)
        {
            Transform trackingSpace = cameraRig.trackingSpace;
            if (trackingSpace != null)
            {
                trackingSpace.rotation = transform.rotation;
            }
            else
            {
                cameraRig.transform.rotation = transform.rotation;
            }
        }

        yield return null; // Wait one frame for rotation to apply

        // Always switch back to FloorLevel
        if (ovrManager != null)
        {
            ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"VRStartPosition: Set tracking origin to FloorLevel");
        }
    }
}
