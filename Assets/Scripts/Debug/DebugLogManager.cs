using UnityEngine;

namespace HandSurvivor.DebugSystem
{
    /// <summary>
    /// Global manager for controlling debug log visibility across the entire project.
    /// Individual scripts check their own showDebugLogs flag AND this global flag.
    /// </summary>
    public static class DebugLogManager
    {
        private const string PREF_KEY_ALL = "HandSurvivor_EnableAllDebugLogs";
        private const string PREF_KEY_LOG = "HandSurvivor_EnableDebugLog";
        private const string PREF_KEY_WARNING = "HandSurvivor_EnableDebugLogWarning";
        private const string PREF_KEY_ERROR = "HandSurvivor_EnableDebugLogError";

        private static bool? _enableAllDebugLogs;
        private static bool? _enableDebugLog;
        private static bool? _enableDebugLogWarning;
        private static bool? _enableDebugLogError;

        /// <summary>
        /// Global master switch for all debug logs.
        /// When false, ALL debug logs are suppressed regardless of individual script settings.
        /// When true, individual script settings and type filters control visibility.
        /// </summary>
        public static bool EnableAllDebugLogs
        {
            get
            {
                if (_enableAllDebugLogs == null)
                {
#if UNITY_EDITOR
                    _enableAllDebugLogs = UnityEditor.EditorPrefs.GetBool(PREF_KEY_ALL, true);
#else
                    _enableAllDebugLogs = true;
#endif
                }

                return _enableAllDebugLogs.Value;
            }
            set
            {
                _enableAllDebugLogs = value;
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool(PREF_KEY_ALL, value);
#endif
            }
        }

        /// <summary>
        /// Enable/disable Debug.Log calls specifically
        /// </summary>
        public static bool EnableDebugLog
        {
            get
            {
                if (_enableDebugLog == null)
                {
#if UNITY_EDITOR
                    _enableDebugLog = UnityEditor.EditorPrefs.GetBool(PREF_KEY_LOG, true);
#else
                    _enableDebugLog = true;
#endif
                }

                return _enableDebugLog.Value;
            }
            set
            {
                _enableDebugLog = value;
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool(PREF_KEY_LOG, value);
#endif
            }
        }

        /// <summary>
        /// Enable/disable Debug.LogWarning calls specifically
        /// </summary>
        public static bool EnableDebugLogWarning
        {
            get
            {
                if (_enableDebugLogWarning == null)
                {
#if UNITY_EDITOR
                    _enableDebugLogWarning = UnityEditor.EditorPrefs.GetBool(PREF_KEY_WARNING, true);
#else
                    _enableDebugLogWarning = true;
#endif
                }

                return _enableDebugLogWarning.Value;
            }
            set
            {
                _enableDebugLogWarning = value;
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool(PREF_KEY_WARNING, value);
#endif
            }
        }

        /// <summary>
        /// Enable/disable Debug.LogError calls specifically
        /// </summary>
        public static bool EnableDebugLogError
        {
            get
            {
                if (_enableDebugLogError == null)
                {
#if UNITY_EDITOR
                    _enableDebugLogError = UnityEditor.EditorPrefs.GetBool(PREF_KEY_ERROR, true);
#else
                    _enableDebugLogError = true;
#endif
                }

                return _enableDebugLogError.Value;
            }
            set
            {
                _enableDebugLogError = value;
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool(PREF_KEY_ERROR, value);
#endif
            }
        }
    }
}