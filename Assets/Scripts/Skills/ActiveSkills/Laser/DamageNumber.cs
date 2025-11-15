using TMPro;
using UnityEngine;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Floating damage number that billboards toward the VR camera and animates upward with fade-out
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private Camera vrCamera;
        private float spawnTime;
        private Vector3 startPosition;
        private float lifetime;
        private float riseSpeed;

        private void Awake()
        {
            if (damageText == null)
            {
                damageText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInChildren<CanvasGroup>();
            }
        }

        public void Initialize(int damage, Vector3 position, float lifetimeValue, float riseSpeedValue, float fontSizeValue, Color textColorValue, float worldScaleValue)
        {
            // Set damage text
            damageText.text = damage.ToString();
            damageText.fontSize = fontSizeValue;
            damageText.color = textColorValue;

            // Set position and scale
            transform.position = position;
            startPosition = position;

            // Apply world scale to the canvas child
            Transform canvasTransform = damageText.transform;
            canvasTransform.localScale = Vector3.one * worldScaleValue;

            // Set animation parameters
            lifetime = lifetimeValue;
            riseSpeed = riseSpeedValue;

            // Get VR camera for billboarding
            vrCamera = FindVRCamera.GetVRCamera();

            // Reset animation
            spawnTime = Time.time;
            canvasGroup.alpha = 1f;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (vrCamera == null)
            {
                vrCamera = FindVRCamera.GetVRCamera();
                if (vrCamera == null) return;
            }

            // Billboard toward camera
            transform.LookAt(vrCamera.transform);
            transform.Rotate(0, 180, 0); // Flip to face camera

            // Animate upward and fade
            float elapsed = Time.time - spawnTime;
            float progress = elapsed / lifetime;

            // Rise upward
            transform.position = startPosition + Vector3.up * (elapsed * riseSpeed);

            // Fade out
            canvasGroup.alpha = fadeCurve.Evaluate(progress);

            // Return to pool when lifetime expired
            if (elapsed >= lifetime)
            {
                DamageNumberManager.Instance.ReturnToPool(this);
            }
        }
    }
}
