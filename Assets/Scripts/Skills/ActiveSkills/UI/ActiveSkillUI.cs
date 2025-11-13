using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HandSurvivor.ActiveSkills.UI
{
    /// <summary>
    /// UI component for displaying a single active skill's cooldown and duration
    /// Handles visual feedback for active state, cooldown state, and timers
    /// </summary>
        public class ActiveSkillUI : MonoBehaviour
        {
        [Header("References")]
        [SerializeField] private ActiveSkillBase activeSkill;

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private Image durationFillImage;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private GameObject cooldownOverlay;

        [Header("Display Settings")]
        [SerializeField] private bool showCooldownTimer = true;
        [SerializeField] private bool showDurationTimer = true;
        [SerializeField] private bool showStackCount = true;
        [SerializeField] private bool showPercentage = false;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color cooldownColor = Color.gray;
        [SerializeField] private Color inactiveColor = Color.white;

        [Header("Fill Type")]
        [SerializeField] private FillMode fillMode = FillMode.Radial;

        private bool isInitialized = false;

        public enum FillMode
        {
            Radial,     // Circular fill (Image.Type = Filled, FillMethod = Radial360)
            Horizontal, // Left to right fill
            Vertical    // Bottom to top fill
        }

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the UI component with the active skill reference
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            if (activeSkill == null)
            {
                Debug.LogError("[ActiveSkillUI] No ActiveSkillBase assigned!", this);
                return;
            }

            // Subscribe to active skill events
            activeSkill.OnActivate.AddListener(OnActiveSkillActivated);
            activeSkill.OnDeactivate.AddListener(OnActiveSkillDeactivated);
            activeSkill.OnExpire.AddListener(OnActiveSkillExpired);

            // Setup UI elements
            SetupFillImages();
            UpdateActiveSkillInfo();
            UpdateVisualState();

            isInitialized = true;
        }

        private void OnDestroy()
        {
            if (activeSkill != null)
            {
                activeSkill.OnActivate.RemoveListener(OnActiveSkillActivated);
                activeSkill.OnDeactivate.RemoveListener(OnActiveSkillDeactivated);
                activeSkill.OnExpire.RemoveListener(OnActiveSkillExpired);
            }
        }

        private void Update()
        {
            if (!isInitialized || activeSkill == null)
            {
                return;
            }

            UpdateTimers();
        }

        /// <summary>
        /// Set the active skill to track
        /// </summary>
        public void SetActiveSkill(ActiveSkillBase newActiveSkill)
        {
            // Unsubscribe from old active skill
            if (activeSkill != null)
            {
                activeSkill.OnActivate.RemoveListener(OnActiveSkillActivated);
                activeSkill.OnDeactivate.RemoveListener(OnActiveSkillDeactivated);
                activeSkill.OnExpire.RemoveListener(OnActiveSkillExpired);
            }

            activeSkill = newActiveSkill;
            isInitialized = false;

            if (activeSkill != null)
            {
                Initialize();
            }
        }

        private void SetupFillImages()
        {
            // Configure cooldown fill image
            if (cooldownFillImage != null)
            {
                ConfigureFillImage(cooldownFillImage);
            }

            // Configure duration fill image
            if (durationFillImage != null)
            {
                ConfigureFillImage(durationFillImage);
            }
        }

        private void ConfigureFillImage(Image image)
        {
            switch (fillMode)
            {
                case FillMode.Radial:
                    image.type = Image.Type.Filled;
                    image.fillMethod = Image.FillMethod.Radial360;
                    image.fillOrigin = (int)Image.Origin360.Top;
                    image.fillClockwise = false;
                    break;

                case FillMode.Horizontal:
                    image.type = Image.Type.Filled;
                    image.fillMethod = Image.FillMethod.Horizontal;
                    image.fillOrigin = (int)Image.OriginHorizontal.Left;
                    break;

                case FillMode.Vertical:
                    image.type = Image.Type.Filled;
                    image.fillMethod = Image.FillMethod.Vertical;
                    image.fillOrigin = (int)Image.OriginVertical.Bottom;
                    break;
            }
        }

        private void UpdateActiveSkillInfo()
        {
            if (activeSkill.Data == null)
            {
                return;
            }

            // Set icon
            if (iconImage != null && activeSkill.Data.skillImage != null)
            {
                iconImage.sprite = activeSkill.Data.skillImage;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = activeSkill.Data.displayName;
            }
        }

        private void UpdateTimers()
        {
            UpdateCooldownDisplay();
            UpdateDurationDisplay();
            UpdateStackCountDisplay();
            UpdateVisualState();
        }

        private void UpdateCooldownDisplay()
        {
            if (!activeSkill.IsOnCooldown)
            {
                // Not on cooldown - hide cooldown UI
                if (cooldownFillImage != null)
                {
                    cooldownFillImage.fillAmount = 0f;
                }

                if (cooldownText != null && showCooldownTimer)
                {
                    cooldownText.text = "";
                }

                if (cooldownOverlay != null)
                {
                    cooldownOverlay.SetActive(false);
                }

                return;
            }

            // On cooldown - update UI
            float remainingCooldown = activeSkill.RemainingCooldown;
            float totalCooldown = activeSkill.Data.cooldown;
            float cooldownPercent = remainingCooldown / totalCooldown;

            // Update fill image
            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = cooldownPercent;
                cooldownFillImage.color = cooldownColor;
            }

            // Update text
            if (cooldownText != null && showCooldownTimer)
            {
                if (showPercentage)
                {
                    cooldownText.text = $"{Mathf.CeilToInt(cooldownPercent * 100)}%";
                }
                else
                {
                    cooldownText.text = FormatTime(remainingCooldown);
                }
            }

            // Show cooldown overlay
            if (cooldownOverlay != null)
            {
                cooldownOverlay.SetActive(true);
            }
        }

        private void UpdateDurationDisplay()
        {
            // Only show duration for Duration and Toggle type active skills
            if (activeSkill.Data.ActiveSkillType != ActiveSkillType.Duration &&
                activeSkill.Data.ActiveSkillType != ActiveSkillType.Toggle)
            {
                if (durationFillImage != null)
                {
                    durationFillImage.fillAmount = 0f;
                }

                if (durationText != null)
                {
                    durationText.text = "";
                }

                return;
            }

            if (!activeSkill.IsActive)
            {
                // Not active - hide duration UI
                if (durationFillImage != null)
                {
                    durationFillImage.fillAmount = 0f;
                }

                if (durationText != null && showDurationTimer)
                {
                    durationText.text = "";
                }

                return;
            }

            // Active - update UI
            float remainingDuration = activeSkill.RemainingDuration;
            float totalDuration = activeSkill.Data.duration;
            float durationPercent = remainingDuration / totalDuration;

            // Update fill image
            if (durationFillImage != null)
            {
                durationFillImage.fillAmount = durationPercent;
                durationFillImage.color = activeColor;
            }

            // Update text
            if (durationText != null && showDurationTimer)
            {
                if (showPercentage)
                {
                    durationText.text = $"{Mathf.CeilToInt(durationPercent * 100)}%";
                }
                else
                {
                    durationText.text = FormatTime(remainingDuration);
                }
            }
        }

        private void UpdateVisualState()
        {
            // Update icon color based on state
            if (iconImage != null)
            {
                if (activeSkill.IsActive)
                {
                    iconImage.color = activeColor;
                }
                else if (activeSkill.IsOnCooldown)
                {
                    iconImage.color = cooldownColor;
                }
                else
                {
                    iconImage.color = inactiveColor;
                }
            }

            // Update active indicator
            if (activeIndicator != null)
            {
                activeIndicator.SetActive(activeSkill.IsActive);
            }
        }

        private void OnActiveSkillActivated()
        {
            Debug.Log($"[ActiveSkillUI] {activeSkill.Data.displayName} activated");
            UpdateVisualState();
        }

        private void OnActiveSkillDeactivated()
        {
            Debug.Log($"[ActiveSkillUI] {activeSkill.Data.displayName} deactivated");
            UpdateVisualState();
        }

        private void OnActiveSkillExpired()
        {
            Debug.Log($"[ActiveSkillUI] {activeSkill.Data.displayName} expired");
            UpdateVisualState();
        }

        /// <summary>
        /// Format time in seconds to readable format (e.g., "1:23" or "5.2s")
        /// </summary>
        private string FormatTime(float seconds)
        {
            if (seconds >= 60f)
            {
                int minutes = Mathf.FloorToInt(seconds / 60f);
                int secs = Mathf.FloorToInt(seconds % 60f);
                return $"{minutes}:{secs:D2}";
            }
            else if (seconds >= 10f)
            {
                return $"{Mathf.CeilToInt(seconds)}s";
            }
            else
            {
                return $"{seconds:F1}s";
            }
        }

        /// <summary>
        /// Show or hide this UI element
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Check if this UI is tracking a active skill
        /// </summary>
        public bool HasActiveSkill()
        {
            return activeSkill != null;
        }

        /// <summary>
        /// Get the active skill this UI is tracking
        /// </summary>
        public ActiveSkillBase GetActiveSkill()
        {
            return activeSkill;
        }

        /// <summary>
        /// Update the stack count display
        /// </summary>
        private void UpdateStackCountDisplay()
        {
            if (!showStackCount || countText == null || activeSkill == null)
            {
                return;
            }

            // Get stack count from inventory
            int stackCount = ActiveSkillInventory.Instance != null
                ? ActiveSkillInventory.Instance.GetActiveSkillCount(activeSkill)
                : 0;

            if (stackCount > 1)
            {
                countText.text = $"x{stackCount}";
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.text = "";
                countText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Clear the active skill reference
        /// </summary>
        public void Clear()
        {
            SetActiveSkill(null);
            SetVisible(false);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Force Update Display")]
        private void DebugForceUpdate()
        {
            if (activeSkill != null)
            {
                UpdateTimers();
            }
        }
#endif
    }
}
