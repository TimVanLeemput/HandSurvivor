using UnityEngine;
using UnityEngine.Events;
using MyBox;
using HandSurvivor.Stats;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Core.Passive;
using HandSurvivor.UI;

namespace HandSurvivor.Core
{
    /// <summary>
    /// Central game state manager. Handles game-wide resets and state management.
    /// Persists across scene loads.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("Events")]
        public UnityEvent OnGameReset;
        public UnityEvent OnGamePaused;
        public UnityEvent OnGameResumed;

        [Header("State")]
        [SerializeField, ReadOnly] private bool isPaused = false;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Reset all game state for a fresh run.
        /// Call this before restarting or returning to menu.
        /// Note: Call PlayerStatsManager.EndRun() BEFORE this to save run stats to lifetime.
        /// </summary>
        [ButtonMethod]
        public void ResetGameState()
        {
            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[GameManager] === RESETTING GAME STATE ===");

            // Reset Game Over UI
            ResetGameOverUI();

            // Reset player progression
            ResetXP();
            ResetActiveSkills();
            ResetPassiveUpgrades();

            // Reset stats tracking (starts new run, preserves lifetime)
            ResetStats();

            OnGameReset?.Invoke();

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[GameManager] === GAME STATE RESET COMPLETE ===");
        }

        private void ResetGameOverUI()
        {
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.HideGameOverUI();
            }
        }

        private void ResetXP()
        {
            if (XPManager.Instance != null)
            {
                XPManager.Instance.Reset();
                if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[GameManager] XP reset");
            }
        }

        private void ResetActiveSkills()
        {
            // Clear slots first (destroys GameObjects)
            if (ActiveSkillSlotManager.Instance != null)
            {
                ActiveSkillSlotManager.Instance.ClearAllSlots();
                if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[GameManager] Active skill slots cleared");
            }

            // Clear inventory tracking
            if (ActiveSkillInventory.Instance != null)
            {
                ActiveSkillInventory.Instance.ClearInventory();
                if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[GameManager] Active skill inventory cleared");
            }
        }

        private void ResetPassiveUpgrades()
        {
            if (PassiveUpgradeManager.Instance != null)
            {
                PassiveUpgradeManager.Instance.ClearAllUpgrades();
                if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[GameManager] Passive upgrades cleared");
            }
        }

        private void ResetStats()
        {
            if (PlayerStatsManager.Instance != null)
            {
                PlayerStatsManager.Instance.StartNewRun();
                if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[GameManager] Stats reset - new run started");
            }
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        [ButtonMethod]
        public void PauseGame()
        {
            if (isPaused) return;

            isPaused = true;
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[GameManager] Game paused");
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        [ButtonMethod]
        public void ResumeGame()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1f;
            OnGameResumed?.Invoke();

            if (showDebugLogs && DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[GameManager] Game resumed");
        }

        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
