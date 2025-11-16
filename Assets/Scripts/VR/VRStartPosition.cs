using UnityEngine;

public class VRStartPosition : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [SerializeField] private bool setRotation = true;
    [SerializeField] private bool overrideHeight = false;

    private void Start()
    {
        OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();

        if (cameraRig != null)
        {
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
                cameraRig.transform.rotation = transform.rotation;
            }
        }
        else
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogWarning("VRStartPosition: OVRCameraRig not found in scene");
        }
    }
}
