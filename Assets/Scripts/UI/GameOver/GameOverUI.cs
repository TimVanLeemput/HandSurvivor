using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HandSurvivor.Stats;

namespace HandSurvivor.UI
{
    /// <summary>
    /// Displays game over screen with run statistics
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private string gameOverTitle = "GAME OVER";

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI enemiesKilledText;
        [SerializeField] private TextMeshProUGUI wavesCompletedText;
        [SerializeField] private TextMeshProUGUI timeSurvivedText;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void OnEnable()
        {
            RefreshStats();
        }

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        /// <summary>
        /// Show the game over screen and populate stats
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshStats();

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[GameOverUI] Game over screen shown");
        }

        /// <summary>
        /// Hide the game over screen
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Refresh all stats from PlayerStatsManager
        /// </summary>
        public void RefreshStats()
        {
            if (titleText != null)
                titleText.text = gameOverTitle;

            if (PlayerStatsManager.Instance == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[GameOverUI] PlayerStatsManager not found");
                return;
            }

            PlayerRunStats runStats = PlayerStatsManager.Instance.GetCurrentRun();

            // Enemies killed
            if (enemiesKilledText != null)
                enemiesKilledText.text = runStats.totalKills.ToString();

            // Waves completed
            if (wavesCompletedText != null)
                wavesCompletedText.text = runStats.currentWave.ToString();

            // Time survived (formatted as mm:ss)
            if (timeSurvivedText != null)
                timeSurvivedText.text = FormatTime(runStats.survivalTime);

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[GameOverUI] Stats refreshed - Kills: {runStats.totalKills}, Wave: {runStats.currentWave}, Time: {runStats.survivalTime:F1}s");
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        private void OnRestartClicked()
        {
            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[GameOverUI] Restart clicked");

            GameOverManager.Instance?.RestartGame();
        }

        private void OnMainMenuClicked()
        {
            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[GameOverUI] Main menu clicked");

            GameOverManager.Instance?.ReturnToMainMenu();
        }

        private void OnDestroy()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveAllListeners();

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveAllListeners();
        }
    }
}
