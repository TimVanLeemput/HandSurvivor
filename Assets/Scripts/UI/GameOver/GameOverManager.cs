using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using HandSurvivor.Stats;
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

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField,ReadOnly] private string restartGameSceneName = "";

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

            // Cache current scene name for restart
            if (string.IsNullOrEmpty(restartGameSceneName))
                restartGameSceneName = SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Trigger game over state
        /// </summary>
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
        /// Restart the current game
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            isGameOver = false;

            OnRestart?.Invoke();

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[GameOverManager] Restarting game: {restartGameSceneName}");

            SceneManager.LoadScene(restartGameSceneName);
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            isGameOver = false;

            OnReturnToMenu?.Invoke();

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[GameOverManager] Returning to main menu: {mainMenuSceneName}");

            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
