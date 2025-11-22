using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HandSurvivor.Stats;
using HandSurvivor.Level;
using HandSurvivor.Core;
using MyBox;

namespace HandSurvivor.UI
{
    /// <summary>
    /// Manages game over state and transitions
    /// </summary>
    public class GameOverManager : MonoBehaviour
    {
        public static GameOverManager Instance { get; private set; }

        [Header("UI Reference")]
        [SerializeField] private GameOverUI gameOverUI;

        [Header("Scene Configuration")]
        [Tooltip("Base scenes to load when restarting (loaded before level scene)")]
        [SerializeField] private List<HandSurvivor.Level.SceneReference> restartScenes = new List<HandSurvivor.Level.SceneReference>();

        [Tooltip("Scenes to load when returning to main menu")]
        [SerializeField] private List<HandSurvivor.Level.SceneReference> mainMenuScenes = new List<HandSurvivor.Level.SceneReference>();

        [Tooltip("Unload all currently loaded scenes before loading new ones")]
        [SerializeField] private bool unloadAllScenesFirst = true;

        [Header("Current Level")]
        [Tooltip("The current level scene path (set at runtime via SetCurrentLevel)")]
        [SerializeField, ReadOnly] private string currentLevelScenePath;

        [Header("Events")]
        public UnityEvent OnGameOver;
        public UnityEvent OnRestart;
        public UnityEvent OnReturnToMenu;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private bool isGameOver = false;

        public bool IsGameOver => isGameOver;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Trigger game over state
        /// </summary>
        [ButtonMethod]
        public void TriggerGameOver()
        {
            if (isGameOver) return;

            isGameOver = true;

            // End the run in stats manager
            if (PlayerStatsManager.Instance != null)
                PlayerStatsManager.Instance.EndRun(victory: false);

            // Show UI
            if (gameOverUI != null)
                gameOverUI.Show();

            // Pause game
            Time.timeScale = 0f;

            OnGameOver?.Invoke();

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[GameOverManager] Game over triggered");
        }

        /// <summary>
        /// Set the current level scene path (call this when a level starts)
        /// </summary>
        public void SetCurrentLevel(string levelScenePath)
        {
            currentLevelScenePath = levelScenePath;

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[GameOverManager] Current level set: {levelScenePath}");
        }

        /// <summary>
        /// Hide and reset the game over UI
        /// </summary>
        public void HideGameOverUI()
        {
            if (gameOverUI != null)
            {
                gameOverUI.Hide();
                gameOverUI.ResetDisplay();
            }
        }

        /// <summary>
        /// Restart the current game
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            isGameOver = false;

            // Reset all game state via central GameManager
            GameManager.Instance?.ResetGameState();

            OnRestart?.Invoke();

            if (SceneLoaderManager.Instance == null)
            {
                Debug.LogError("[GameOverManager] SceneLoaderManager not found!");
                return;
            }

            // Build list of scene paths to load
            List<string> scenePaths = new List<string>();

            // Add base restart scenes
            if (restartScenes != null)
            {
                foreach (HandSurvivor.Level.SceneReference sceneRef in restartScenes)
                {
                    if (sceneRef != null && sceneRef.IsValid)
                        scenePaths.Add(sceneRef.ScenePath);
                }
            }

            // Add current level scene
            if (!string.IsNullOrEmpty(currentLevelScenePath))
                scenePaths.Add(currentLevelScenePath);

            if (scenePaths.Count == 0)
            {
                Debug.LogError("[GameOverManager] No scenes to load for restart!");
                return;
            }

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[GameOverManager] Restarting game with {scenePaths.Count} scenes (including level: {currentLevelScenePath})");

            if (unloadAllScenesFirst)
                SceneLoaderManager.Instance.UnloadAllScenes();

            SceneLoaderManager.Instance.LoadScenesByPath(scenePaths);
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (mainMenuScenes == null || mainMenuScenes.Count == 0)
            {
                Debug.LogError("[GameOverManager] No main menu scenes configured!");
                return;
            }

            Time.timeScale = 1f;
            isGameOver = false;

            // Reset all game state via central GameManager
            GameManager.Instance?.ResetGameState();

            OnReturnToMenu?.Invoke();

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[GameOverManager] Returning to main menu with {mainMenuScenes.Count} scenes");

            LoadSceneSequence(mainMenuScenes);
        }

        private void LoadSceneSequence(List<HandSurvivor.Level.SceneReference> scenes)
        {
            if (SceneLoaderManager.Instance == null)
            {
                Debug.LogError("[GameOverManager] SceneLoaderManager not found!");
                return;
            }

            if (unloadAllScenesFirst)
            {
                SceneLoaderManager.Instance.UnloadAllScenes();
            }

            SceneLoaderManager.Instance.LoadScenes(scenes);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
