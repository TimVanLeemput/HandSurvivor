using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandSurvivor.UI
{
    public class ExitButton : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         public void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log("[ExitButton] Exiting play mode");
#else
            Application.Quit();
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log("[ExitButton] Quitting application");
#endif
        }
    }
}
