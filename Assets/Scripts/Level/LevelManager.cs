using UnityEngine;
using UnityEngine.SceneManagement;
using HandSurvivor.UI;

namespace HandSurvivor.Level
{
    /// <summary>
    /// Place this in each level scene to register it with GameOverManager.
    /// Automatically detects its own scene and registers it for restarts.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private string levelScenePath;

        private void Awake()
        {
            // Capture scene path in Awake before any DontDestroyOnLoad operations
            CaptureScenePath();
        }

        private void Start()
        {
            RegisterLevel();
        }

        private void CaptureScenePath()
        {
            Scene myScene = gameObject.scene;

            // Validate scene is real (not DontDestroyOnLoad)
            if (!myScene.IsValid() || string.IsNullOrEmpty(myScene.path) || myScene.name == "DontDestroyOnLoad")
            {
                Debug.LogError($"[LevelManager] Invalid scene for {gameObject.name}. Make sure LevelManager is in a level scene, not DontDestroyOnLoad!");
                return;
            }

            levelScenePath = myScene.path;

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[LevelManager] Captured scene path: {levelScenePath}");
        }

        private void RegisterLevel()
        {
            if (string.IsNullOrEmpty(levelScenePath))
            {
                Debug.LogError($"[LevelManager] No scene path captured for {gameObject.name}!");
                return;
            }

            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.SetCurrentLevel(levelScenePath);

                if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[LevelManager] Registered level: {levelScenePath}");
            }
            else
            {
                Debug.LogWarning("[LevelManager] GameOverManager not found!");
            }
        }
    }
}
