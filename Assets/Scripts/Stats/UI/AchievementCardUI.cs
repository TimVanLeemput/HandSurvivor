using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HandSurvivor.Stats.UI
{
    /// <summary>
    /// Individual achievement card in the achievement menu
    /// Displays achievement info with locked/unlocked states
    /// </summary>
    public class AchievementCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image progressBarFill;
        [SerializeField] private GameObject progressBarContainer;
        [SerializeField] private TextMeshProUGUI unlockDateText;

        [Header("Locked State")]
        [SerializeField] private Sprite lockedIcon;
        [SerializeField] private string lockedTitle = "???";
        [SerializeField] private string lockedDescription = "Hidden Achievement";
        [SerializeField] private Color lockedIconColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color lockedTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Header("Unlocked State")]
        [SerializeField] private Color unlockedIconColor = Color.white;
        [SerializeField] private Color unlockedTextColor = Color.white;

        [Header("Settings")]
        [SerializeField] private bool showProgressOnLocked = false;

        private Achievement achievement;
        private bool isUnlocked;

        /// <summary>
        /// Initialize the card with achievement data
        /// </summary>
        public void Initialize(Achievement achievement, bool isUnlocked, float progress, string progressString)
        {
            this.achievement = achievement;
            this.isUnlocked = isUnlocked;

            if (isUnlocked)
            {
                SetupUnlockedState(progress, progressString);
            }
            else
            {
                SetupLockedState(progress, progressString);
            }
        }

        private void SetupUnlockedState(float progress, string progressString)
        {
            // Set icon
            if (iconImage != null)
            {
                iconImage.sprite = achievement.icon != null ? achievement.icon : lockedIcon;
                iconImage.color = unlockedIconColor;
            }

            // Set title
            if (titleText != null)
            {
                titleText.text = achievement.displayName;
                titleText.color = unlockedTextColor;
            }

            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = achievement.description;
                descriptionText.color = unlockedTextColor;
            }

            // Set progress (100% for unlocked)
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = 1f;
            }

            if (progressText != null)
            {
                progressText.text = progressString;
                progressText.color = unlockedTextColor;
            }

            // Show progress bar if container exists
            if (progressBarContainer != null)
            {
                progressBarContainer.SetActive(true);
            }

            // Set unlock date
            if (unlockDateText != null)
            {
                DateTime? unlockDate = achievement.GetUnlockedDate();
                if (unlockDate.HasValue)
                {
                    unlockDateText.text = $"Unlocked: {unlockDate.Value:MMM dd, yyyy}";
                    unlockDateText.color = unlockedTextColor;
                    unlockDateText.gameObject.SetActive(true);
                }
                else
                {
                    unlockDateText.gameObject.SetActive(false);
                }
            }
        }

        private void SetupLockedState(float progress, string progressString)
        {
            // Set locked icon
            if (iconImage != null)
            {
                iconImage.sprite = lockedIcon;
                iconImage.color = lockedIconColor;
            }

            // Set locked title
            if (titleText != null)
            {
                titleText.text = lockedTitle;
                titleText.color = lockedTextColor;
            }

            // Set locked description
            if (descriptionText != null)
            {
                descriptionText.text = lockedDescription;
                descriptionText.color = lockedTextColor;
            }

            // Handle progress display for locked achievements
            if (showProgressOnLocked)
            {
                // Show progress bar with current progress
                if (progressBarFill != null)
                {
                    progressBarFill.fillAmount = progress;
                }

                if (progressText != null)
                {
                    progressText.text = progressString;
                    progressText.color = lockedTextColor;
                }

                if (progressBarContainer != null)
                {
                    progressBarContainer.SetActive(true);
                }
            }
            else
            {
                // Hide progress for locked achievements
                if (progressBarContainer != null)
                {
                    progressBarContainer.SetActive(false);
                }

                if (progressText != null)
                {
                    progressText.gameObject.SetActive(false);
                }
            }

            // Hide unlock date for locked achievements
            if (unlockDateText != null)
            {
                unlockDateText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Update progress (for real-time updates)
        /// </summary>
        public void UpdateProgress(float progress, string progressString)
        {
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = progress;
            }

            if (progressText != null && (isUnlocked || showProgressOnLocked))
            {
                progressText.text = progressString;
            }
        }

        /// <summary>
        /// Get the achievement this card represents
        /// </summary>
        public Achievement GetAchievement()
        {
            return achievement;
        }

        /// <summary>
        /// Check if this card is showing an unlocked achievement
        /// </summary>
        public bool IsUnlocked()
        {
            return isUnlocked;
        }
    }
}
