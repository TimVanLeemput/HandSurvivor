using UnityEditor;
using UnityEngine;

namespace HandSurvivor.Editor.Tools
{
    /// <summary>
    /// Editor tool for globally enabling/disabling all debug logs via Tools menu
    /// </summary>
    public static class DebugLogToggleTool
    {
        [MenuItem("Tools/Debug Logs/Enable All Debug Logs")]
        private static void EnableAllDebugLogs()
        {
            HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs = true;
            UnityEngine.Debug.Log("[DebugLogToggleTool] All debug logs ENABLED globally");

            // Force refresh all scene objects
            RefreshAllDebugFlags();
        }

        [MenuItem("Tools/Debug Logs/Disable All Debug Logs")]
        private static void DisableAllDebugLogs()
        {
            HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs = false;
            UnityEngine.Debug.Log("[DebugLogToggleTool] All debug logs DISABLED globally");

            // Force refresh all scene objects
            RefreshAllDebugFlags();
        }

        [MenuItem("Tools/Debug Logs/Enable All Debug Logs", true)]
        [MenuItem("Tools/Debug Logs/Disable All Debug Logs", true)]
        private static bool ValidateDebugLogMenu()
        {
            // Menu items always available
            return true;
        }

        private static void RefreshAllDebugFlags()
        {
            // Force Unity to refresh inspector for all MonoBehaviours
            MonoBehaviour[] allMonoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour mb in allMonoBehaviours)
            {
                EditorUtility.SetDirty(mb);
            }
        }
    }
}
