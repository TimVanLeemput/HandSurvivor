using UnityEditor;
using UnityEngine;

namespace HandSurvivor.Editor.Tools
{
    /// <summary>
    /// EditorWindow for fine-grained control over debug logging
    /// Allows enabling/disabling specific log types (Log, LogWarning, LogError)
    /// </summary>
    public class DebugLogControlWindow : EditorWindow
    {
        private const string PREF_ENABLE_ALL = "HandSurvivor_EnableAllDebugLogs";
        private const string PREF_ENABLE_LOG = "HandSurvivor_EnableDebugLog";
        private const string PREF_ENABLE_WARNING = "HandSurvivor_EnableDebugLogWarning";
        private const string PREF_ENABLE_ERROR = "HandSurvivor_EnableDebugLogError";

        private bool enableAllLogs;
        private bool enableLog;
        private bool enableLogWarning;
        private bool enableLogError;

        [MenuItem("Tools/Debug Logs/Debug Log Control Panel")]
        private static void ShowWindow()
        {
            DebugLogControlWindow window = GetWindow<DebugLogControlWindow>("Debug Log Control");
            window.minSize = new Vector2(350, 250);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPreferences();
        }

        private void OnGUI()
        {
            GUILayout.Label("Debug Log Control Panel", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Control which debug logs are displayed globally.\n" +
                "Individual scripts can still disable their own logs via the showDebugLogs field.",
                MessageType.Info);

            GUILayout.Space(10);

            // Master toggle
            EditorGUI.BeginChangeCheck();
            bool newEnableAll = EditorGUILayout.Toggle("Enable All Debug Logs", enableAllLogs);
            if (EditorGUI.EndChangeCheck())
            {
                enableAllLogs = newEnableAll;
                HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs = enableAllLogs;
                EditorPrefs.SetBool(PREF_ENABLE_ALL, enableAllLogs);

                Debug.Log($"[DebugLogControl] All debug logs {(enableAllLogs ? "ENABLED" : "DISABLED")}");
            }

            GUILayout.Space(15);

            // Show type-specific controls only if master is enabled
            EditorGUI.BeginDisabledGroup(!enableAllLogs);

            GUILayout.Label("Log Type Filters:", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Debug.Log toggle
            EditorGUI.BeginChangeCheck();
            enableLog = EditorGUILayout.Toggle("  Debug.Log", enableLog);
            if (EditorGUI.EndChangeCheck())
            {
                HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog = enableLog;
                EditorPrefs.SetBool(PREF_ENABLE_LOG, enableLog);
                Debug.Log($"[DebugLogControl] Debug.Log {(enableLog ? "ENABLED" : "DISABLED")}");
            }

            // Debug.LogWarning toggle
            EditorGUI.BeginChangeCheck();
            enableLogWarning = EditorGUILayout.Toggle("  Debug.LogWarning", enableLogWarning);
            if (EditorGUI.EndChangeCheck())
            {
                HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogWarning = enableLogWarning;
                EditorPrefs.SetBool(PREF_ENABLE_WARNING, enableLogWarning);
                Debug.Log($"[DebugLogControl] Debug.LogWarning {(enableLogWarning ? "ENABLED" : "DISABLED")}");
            }

            // Debug.LogError toggle
            EditorGUI.BeginChangeCheck();
            enableLogError = EditorGUILayout.Toggle("  Debug.LogError", enableLogError);
            if (EditorGUI.EndChangeCheck())
            {
                HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogError = enableLogError;
                EditorPrefs.SetBool(PREF_ENABLE_ERROR, enableLogError);
                Debug.Log($"[DebugLogControl] Debug.LogError {(enableLogError ? "ENABLED" : "DISABLED")}");
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(15);

            // Quick presets
            GUILayout.Label("Quick Presets:", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("All On"))
            {
                SetAllLogTypes(true, true, true, true);
            }
            if (GUILayout.Button("All Off"))
            {
                SetAllLogTypes(false, false, false, false);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Errors Only"))
            {
                SetAllLogTypes(true, false, false, true);
            }
            if (GUILayout.Button("Warnings + Errors"))
            {
                SetAllLogTypes(true, false, true, true);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Current status
            GUILayout.Label("Current Status:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"Master: {(enableAllLogs ? "ON" : "OFF")}\n" +
                $"Log: {(enableLog ? "ON" : "OFF")} | Warning: {(enableLogWarning ? "ON" : "OFF")} | Error: {(enableLogError ? "ON" : "OFF")}",
                MessageType.None);
        }

        private void LoadPreferences()
        {
            enableAllLogs = EditorPrefs.GetBool(PREF_ENABLE_ALL, true);
            enableLog = EditorPrefs.GetBool(PREF_ENABLE_LOG, true);
            enableLogWarning = EditorPrefs.GetBool(PREF_ENABLE_WARNING, true);
            enableLogError = EditorPrefs.GetBool(PREF_ENABLE_ERROR, true);

            // Sync with DebugLogManager
            HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs = enableAllLogs;
            HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog = enableLog;
            HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogWarning = enableLogWarning;
            HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogError = enableLogError;
        }

        private void SetAllLogTypes(bool all, bool log, bool warning, bool error)
        {
            enableAllLogs = all;
            enableLog = log;
            enableLogWarning = warning;
            enableLogError = error;

            HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs = enableAllLogs;
            HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLog = enableLog;
            HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogWarning = enableLogWarning;
            HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogError = enableLogError;

            EditorPrefs.SetBool(PREF_ENABLE_ALL, enableAllLogs);
            EditorPrefs.SetBool(PREF_ENABLE_LOG, enableLog);
            EditorPrefs.SetBool(PREF_ENABLE_WARNING, enableLogWarning);
            EditorPrefs.SetBool(PREF_ENABLE_ERROR, enableLogError);

            Debug.Log($"[DebugLogControl] Preset applied - All:{all}, Log:{log}, Warning:{warning}, Error:{error}");
            Repaint();
        }
    }
}
