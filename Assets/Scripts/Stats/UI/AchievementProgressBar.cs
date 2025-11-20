using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HandSurvivor.Stats.UI
{
    /// <summary>
    /// Progress bar component for achievement progress display
    /// Can be used standalone or as part of achievement cards
    /// </summary>
    public class AchievementProgressBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Colors")]
        [SerializeField] private Gradient progressGradient;
        [SerializeField] private bool useGradient = false;
        [SerializeField] private Color defaultFillColor = Color.yellow;

        [Header("Animation")]
        [SerializeField] private bool animateProgress = true;
        [SerializeField] private float animationSpeed = 2f;

        private float targetProgress = 0f;
        private float currentProgress = 0f;

        /// <summary>
        /// Set progress value (0-1)
        /// </summary>
        public void SetProgress(float progress, bool instant = false)
        {
            targetProgress = Mathf.Clamp01(progress);

            if (instant || !animateProgress)
            {
                currentProgress = targetProgress;
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Set progress with formatted text
        /// </summary>
        public void SetProgress(float progress, string text, bool instant = false)
        {
            SetProgress(progress, instant);

            if (progressText != null)
            {
                progressText.text = text;
            }
        }

        private void Update()
        {
            if (animateProgress && Mathf.Abs(currentProgress - targetProgress) > 0.001f)
            {
                currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * animationSpeed);
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = currentProgress;

                // Update color based on progress if using gradient
                if (useGradient && progressGradient != null)
                {
                    fillImage.color = progressGradient.Evaluate(currentProgress);
                }
                else if (!useGradient)
                {
                    fillImage.color = defaultFillColor;
                }
            }
        }

        /// <summary>
        /// Get current progress value
        /// </summary>
        public float GetProgress()
        {
            return currentProgress;
        }

        /// <summary>
        /// Reset progress to zero
        /// </summary>
        public void ResetProgress()
        {
            SetProgress(0f, true);
        }
    }
}
