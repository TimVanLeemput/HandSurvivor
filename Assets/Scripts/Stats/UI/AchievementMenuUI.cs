using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HandSurvivor.Stats.UI
{
    /// <summary>
    /// Main achievement browser menu
    /// Displays grid of achievement cards with filtering options
    /// </summary>
    public class AchievementMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private AchievementCardUI cardPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Filter Buttons")]
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterUnlockedButton;
        [SerializeField] private Button filterLockedButton;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        [Header("Settings")]
        [SerializeField] private bool showHiddenAchievements = false;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private List<AchievementCardUI> spawnedCards = new List<AchievementCardUI>();
        private AchievementFilter currentFilter = AchievementFilter.All;

        private void OnEnable()
        {
            RefreshMenu();
        }

        private void Start()
        {
            SetupButtons();
            RefreshMenu();
        }

        private void SetupButtons()
        {
            if (filterAllButton != null)
                filterAllButton.onClick.AddListener(() => SetFilter(AchievementFilter.All));

            if (filterUnlockedButton != null)
                filterUnlockedButton.onClick.AddListener(() => SetFilter(AchievementFilter.Unlocked));

            if (filterLockedButton != null)
                filterLockedButton.onClick.AddListener(() => SetFilter(AchievementFilter.Locked));

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseMenu);
        }

        /// <summary>
        /// Set filter and refresh display
        /// </summary>
        public void SetFilter(AchievementFilter filter)
        {
            currentFilter = filter;
            RefreshMenu();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[AchievementMenuUI] Filter set to: {filter}");
        }

        /// <summary>
        /// Refresh the achievement menu
        /// </summary>
        public void RefreshMenu()
        {
            if (AchievementManager.Instance == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[AchievementMenuUI] AchievementManager not available");
                return;
            }

            // Clear existing cards
            ClearCards();

            // Get filtered achievements
            List<Achievement> achievements = GetFilteredAchievements();

            // Spawn cards for each achievement
            foreach (Achievement achievement in achievements)
            {
                SpawnCard(achievement);
            }

            // Update header
            UpdateHeader();

            // Reset scroll position
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[AchievementMenuUI] Menu refreshed - {achievements.Count} achievements displayed");
        }

        private List<Achievement> GetFilteredAchievements()
        {
            List<Achievement> achievements = new List<Achievement>();

            switch (currentFilter)
            {
                case AchievementFilter.All:
                    achievements = AchievementManager.Instance.GetAllAchievements();
                    break;

                case AchievementFilter.Unlocked:
                    achievements = AchievementManager.Instance.GetUnlockedAchievements();
                    break;

                case AchievementFilter.Locked:
                    achievements = AchievementManager.Instance.GetLockedAchievements();
                    break;
            }

            return achievements;
        }

        private void SpawnCard(Achievement achievement)
        {
            if (cardPrefab == null || cardContainer == null)
            {
                Debug.LogError("[AchievementMenuUI] Card prefab or container not assigned!");
                return;
            }

            // Create card instance
            AchievementCardUI card = Instantiate(cardPrefab, cardContainer);

            // Determine if achievement is unlocked
            bool isUnlocked = AchievementManager.Instance.IsAchievementUnlocked(achievement.achievementId);

            // Get progress
            float progress = AchievementManager.Instance.GetAchievementProgress(achievement);
            string progressString = AchievementManager.Instance.GetAchievementProgressString(achievement);

            // Initialize card (it will handle locked/unlocked display logic)
            card.Initialize(achievement, isUnlocked, progress, progressString);

            spawnedCards.Add(card);
        }

        private void ClearCards()
        {
            foreach (AchievementCardUI card in spawnedCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }

            spawnedCards.Clear();
        }

        private void UpdateHeader()
        {
            if (headerText != null)
            {
                string filterName = currentFilter.ToString();
                headerText.text = $"Achievements - {filterName}";
            }

            if (progressText != null)
            {
                int unlocked = AchievementManager.Instance.GetUnlockedAchievements().Count;
                int total = AchievementManager.Instance.GetAllAchievements().Count;
                float percentage = AchievementManager.Instance.GetTotalUnlockPercentage() * 100f;

                progressText.text = $"{unlocked}/{total} ({percentage:F0}%)";
            }
        }

        /// <summary>
        /// Close the menu
        /// </summary>
        public void CloseMenu()
        {
            gameObject.SetActive(false);

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[AchievementMenuUI] Menu closed");
        }

        /// <summary>
        /// Open the menu
        /// </summary>
        public void OpenMenu()
        {
            gameObject.SetActive(true);
            RefreshMenu();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[AchievementMenuUI] Menu opened");
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (filterAllButton != null)
                filterAllButton.onClick.RemoveAllListeners();

            if (filterUnlockedButton != null)
                filterUnlockedButton.onClick.RemoveAllListeners();

            if (filterLockedButton != null)
                filterLockedButton.onClick.RemoveAllListeners();

            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Filter options for achievement display
    /// </summary>
    public enum AchievementFilter
    {
        All,
        Unlocked,
        Locked
    }
}
