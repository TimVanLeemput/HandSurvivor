using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HandSurvivor.PowerUps.UI
{
    /// <summary>
    /// UI component for displaying a single power-up's cooldown and duration
    /// Handles visual feedback for active state, cooldown state, and timers
    /// </summary>
    public class PowerUpUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PowerUpBase powerUp;

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private Image durationFillImage;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private GameObject cooldownOverlay;

        [Header("Display Settings")]
        [SerializeField] private bool showCooldownTimer = true;
        [SerializeField] private bool showDurationTimer = true;
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
        /// Initialize the UI component with the power-up reference
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            if (powerUp == null)
            {
                Debug.LogError("[PowerUpUI] No PowerUpBase assigned!", this);
                return;
            }

            // Subscribe to power-up events
            powerUp.OnActivate.AddListener(OnPowerUpActivated);
            powerUp.OnDeactivate.AddListener(OnPowerUpDeactivated);
            powerUp.OnExpire.AddListener(OnPowerUpExpired);

            // Setup UI elements
            SetupFillImages();
            UpdatePowerUpInfo();
            UpdateVisualState();

            isInitialized = true;
        }

        private void OnDestroy()
        {
            if (powerUp != null)
            {
                powerUp.OnActivate.RemoveListener(OnPowerUpActivated);
                powerUp.OnDeactivate.RemoveListener(OnPowerUpDeactivated);
                powerUp.OnExpire.RemoveListener(OnPowerUpExpired);
            }
        }

        private void Update()
        {
            if (!isInitialized || powerUp == null)
            {
                return;
            }

            UpdateTimers();
        }

        /// <summary>
        /// Set the power-up to track
        /// </summary>
        public void SetPowerUp(PowerUpBase newPowerUp)
        {
            // Unsubscribe from old power-up
            if (powerUp != null)
            {
                powerUp.OnActivate.RemoveListener(OnPowerUpActivated);
                powerUp.OnDeactivate.RemoveListener(OnPowerUpDeactivated);
                powerUp.OnExpire.RemoveListener(OnPowerUpExpired);
            }

            powerUp = newPowerUp;
            isInitialized = false;

            if (powerUp != null)
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

        private void UpdatePowerUpInfo()
        {
            if (powerUp.Data == null)
            {
                return;
            }

            // Set icon
            if (iconImage != null && powerUp.Data.icon != null)
            {
                iconImage.sprite = powerUp.Data.icon;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = powerUp.Data.displayName;
            }
        }

        private void UpdateTimers()
        {
            UpdateCooldownDisplay();
            UpdateDurationDisplay();
            UpdateVisualState();
        }

        private void UpdateCooldownDisplay()
        {
            if (!powerUp.IsOnCooldown)
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
            float remainingCooldown = powerUp.RemainingCooldown;
            float totalCooldown = powerUp.Data.cooldown;
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
            // Only show duration for Duration and Toggle type power-ups
            if (powerUp.Data.powerUpType != PowerUpType.Duration &&
                powerUp.Data.powerUpType != PowerUpType.Toggle)
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

            if (!powerUp.IsActive)
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
            float remainingDuration = powerUp.RemainingDuration;
            float totalDuration = powerUp.Data.duration;
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
                if (powerUp.IsActive)
                {
                    iconImage.color = activeColor;
                }
                else if (powerUp.IsOnCooldown)
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
                activeIndicator.SetActive(powerUp.IsActive);
            }
        }

        private void OnPowerUpActivated()
        {
            Debug.Log($"[PowerUpUI] {powerUp.Data.displayName} activated");
            UpdateVisualState();
        }

        private void OnPowerUpDeactivated()
        {
            Debug.Log($"[PowerUpUI] {powerUp.Data.displayName} deactivated");
            UpdateVisualState();
        }

        private void OnPowerUpExpired()
        {
            Debug.Log($"[PowerUpUI] {powerUp.Data.displayName} expired");
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
        /// Check if this UI is tracking a power-up
        /// </summary>
        public bool HasPowerUp()
        {
            return powerUp != null;
        }

        /// <summary>
        /// Clear the power-up reference
        /// </summary>
        public void Clear()
        {
            SetPowerUp(null);
            SetVisible(false);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Force Update Display")]
        private void DebugForceUpdate()
        {
            if (powerUp != null)
            {
                UpdateTimers();
            }
        }
#endif
    }
}
