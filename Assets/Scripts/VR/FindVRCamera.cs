using UnityEngine;

/// <summary>
/// Helper component that finds and assigns the VR camera to components on the same GameObject
/// Works with Canvas, AudioListener, and any component with a Camera field
/// No editor fields needed - fully automatic
/// </summary>
public class FindVRCamera : MonoBehaviour
{
    [Header("Debug")] [SerializeField] private bool showDebugLogs = true;
    public static Camera VRCamera;

    private void Awake()
    {
        OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();

        if (cameraRig == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogWarning("[FindVRCamera] Could not find OVRCameraRig in scene!", this);
            return;
        }

        Camera vrCamera = cameraRig.centerEyeAnchor.GetComponent<Camera>();

        if (vrCamera == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogWarning("[FindVRCamera] OVRCameraRig center eye anchor does not have a Camera component!",
                    this);
            return;
        }

        // Assign to Canvas if present
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = vrCamera;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[FindVRCamera] Assigned VR camera to Canvas on {gameObject.name}", this);
        }
    }

    public static Camera GetVRCamera()
    {
        OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
        VRCamera = cameraRig.centerEyeAnchor.GetComponent<Camera>();
        return cameraRig != null ? cameraRig.centerEyeAnchor.GetComponent<Camera>() : null;
    }
}