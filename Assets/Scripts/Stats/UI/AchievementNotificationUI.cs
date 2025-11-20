using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HandSurvivor.Stats.UI
{
    /// <summary>
    /// Individual achievement notification UI component
    /// Displays in world-space, billboards toward VR camera, and auto-dismisses
    /// </summary>
    public class AchievementNotificationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI achievementUnlockedText;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float holdDuration = 3.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private AnimationCurve scaleInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private Vector3 startScale = new Vector3(0.01f, 0.01f, 0.01f);

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip unlockSound;

        private Transform vrCamera;
        private Vector3 originalScale;
        private Coroutine animationCoroutine;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            originalScale = transform.localScale;

            // Find VR camera
            vrCamera = global::FindVRCamera.GetVRCamera().gameObject.transform;
        }

 private void Update()
        {
            // Billboard toward VR camera
            if (vrCamera != null)
            {
                transform.LookAt(vrCamera.position);
                transform.Rotate(0, 180, 0); // Flip to face camera
            }
        }

        /// <summary>
        /// Initialize and show the notification with achievement data
        /// </summary>
        public void Show(Achievement achievement, System.Action onComplete = null)
        {
            if (achievement == null)
            {
                Debug.LogError("[AchievementNotificationUI] Achievement is null!");
                return;
            }

            // Set UI elements
            if (titleText != null)
                titleText.text = achievement.displayName;

            if (iconImage != null && achievement.icon != null)
                iconImage.sprite = achievement.icon;

            if (achievementUnlockedText != null)
                achievementUnlockedText.text = "Achievement Unlocked";

            // Play unlock sound
            if (audioSource != null && unlockSound != null)
            {
                audioSource.PlayOneShot(unlockSound);
            }

            // Start animation sequence
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(AnimateNotification(onComplete));
        }

        private IEnumerator AnimateNotification(System.Action onComplete)
        {
            // Reset state - start extremely tiny
            canvasGroup.alpha = 0f;
            transform.localScale = startScale;

            // Fade in and grow from tiny to normal size
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;

                // Fade in alpha
                canvasGroup.alpha = fadeInCurve.Evaluate(t);

                // Grow from extremely tiny to normal size
                float scaleT = scaleInCurve.Evaluate(t);
                transform.localScale = Vector3.Lerp(startScale, originalScale, scaleT);

                yield return null;
            }

            canvasGroup.alpha = 1f;
            transform.localScale = originalScale;

            // Hold visible
            yield return new WaitForSeconds(holdDuration);

            // Fade out (keep size)
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                canvasGroup.alpha = fadeOutCurve.Evaluate(t);
                yield return null;
            }

            canvasGroup.alpha = 0f;

            // Notify completion
            onComplete?.Invoke();
        }

        /// <summary>
        /// Force hide this notification immediately
        /// </summary>
        public void Hide()
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Get total animation duration
        /// </summary>
        public float GetTotalDuration()
        {
            return fadeInDuration + holdDuration + fadeOutDuration;
        }
    }
}
