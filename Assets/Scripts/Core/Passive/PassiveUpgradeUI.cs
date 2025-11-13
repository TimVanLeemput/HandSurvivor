using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HandSurvivor.Core.Passive
{
    /// <summary>
    /// UI component for displaying a single passive upgrade option
    /// Shows icon, name, description, and level/stack count
    /// </summary>
    public class PassiveUpgradeUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PassiveUpgradeData upgradeData;

        [Header("UI Elements")]
        [SerializeField] private Button passiveUpgradeButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private GameObject newIndicator;

        [Header("Display Settings")]
        [SerializeField] private bool showLevelCount = true;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color selectedColor = Color.green;

        private bool isInitialized = false;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the UI component with the passive upgrade reference
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            if (upgradeData == null)
            {
                Debug.LogWarning("[PassiveUpgradeUI] No PassiveUpgradeData assigned!", this);
                return;
            }

            UpdateUpgradeInfo();
            isInitialized = true;
        }

        /// <summary>
        /// Set the passive upgrade to display
        /// </summary>
        public void SetPassiveUpgrade(PassiveUpgradeData newUpgradeData)
        {
            upgradeData = newUpgradeData;
            isInitialized = false;

            if (upgradeData != null)
            {
                Initialize();
            }
        }

        private void UpdateUpgradeInfo()
        {
            if (upgradeData == null)
            {
                return;
            }

            // Set icon
            if (iconImage != null && upgradeData.passiveImage != null)
            {
                iconImage.sprite = upgradeData.passiveImage;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = upgradeData.displayName;
            }

            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = upgradeData.GetFormattedDescription();
            }

            // Update level display
            UpdateLevelDisplay();

            // Update new indicator
            UpdateNewIndicator();
        }

        /// <summary>
        /// Update the level/stack count display
        /// </summary>
        public void UpdateLevelDisplay()
        {
            if (!showLevelCount || levelText == null || upgradeData == null)
            {
                return;
            }

            if (PassiveUpgradeManager.Instance != null)
            {
                int stackCount = PassiveUpgradeManager.Instance.GetUpgradeStackCount(upgradeData.upgradeId);

                if (stackCount > 0)
                {
                    levelText.text = $"Lv.{stackCount}";
                    levelText.gameObject.SetActive(true);
                }
                else
                {
                    levelText.text = "";
                    levelText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update the "new" indicator (shown if this is a new upgrade)
        /// </summary>
        private void UpdateNewIndicator()
        {
            if (newIndicator == null || upgradeData == null)
            {
                return;
            }

            if (PassiveUpgradeManager.Instance != null)
            {
                int stackCount = PassiveUpgradeManager.Instance.GetUpgradeStackCount(upgradeData.upgradeId);
                newIndicator.SetActive(stackCount == 0);
            }
        }

        /// <summary>
        /// Set the visual state of the upgrade card
        /// </summary>
        public void SetVisualState(VisualState state)
        {
            if (iconImage == null)
            {
                return;
            }

            switch (state)
            {
                case VisualState.Normal:
                    iconImage.color = normalColor;
                    break;

                case VisualState.Highlighted:
                    iconImage.color = highlightColor;
                    break;

                case VisualState.Selected:
                    iconImage.color = selectedColor;
                    break;
            }
        }

        /// <summary>
        /// Called when this upgrade is selected by the player
        /// </summary>
        public void OnSelected()
        {
            if (upgradeData == null)
            {
                Debug.LogError("[PassiveUpgradeUI] Cannot select - no upgrade data!");
                return;
            }

            if (PassiveUpgradeManager.Instance != null)
            {
                PassiveUpgradeManager.Instance.ApplyUpgrade(upgradeData);
            }

            UpdateLevelDisplay();
            UpdateNewIndicator();

            Debug.Log($"[PassiveUpgradeUI] Selected upgrade: {upgradeData.displayName}");
        }

        /// <summary>
        /// Show or hide this UI element
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Check if this UI has a passive upgrade assigned
        /// </summary>
        public bool HasUpgradeData()
        {
            return upgradeData != null;
        }

        /// <summary>
        /// Get the passive upgrade data this UI is displaying
        /// </summary>
        public PassiveUpgradeData GetUpgradeData()
        {
            return upgradeData;
        }

        /// <summary>
        /// Clear the upgrade reference
        /// </summary>
        public void Clear()
        {
            SetPassiveUpgrade(null);
            SetVisible(false);
        }

        /// <summary>
        /// Enable or disable button interaction
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (passiveUpgradeButton != null)
            {
                passiveUpgradeButton.interactable = interactable;
            }
        }

        public enum VisualState
        {
            Normal,
            Highlighted,
            Selected
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Force Update Display")]
        private void DebugForceUpdate()
        {
            if (upgradeData != null)
            {
                UpdateUpgradeInfo();
            }
        }
#endif
    }
}
