using HandSurvivor.ActiveSkills;
using UnityEngine;
using UnityEngine.UI;

namespace HandSurvivor.Skills
{
    /// <summary>
    /// Individual skill slot UI element showing icon and cooldown state.
    /// </summary>
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private GameObject emptySlotIndicator;

        [Header("Visual Settings")]
        [SerializeField] private Image.FillMethod fillMethod = Image.FillMethod.Radial360;
        [SerializeField] private bool clockwiseFill = true;

        private ActiveSkillBase currentSkill;
        private bool isTrackingCooldown = false;

        private void Start()
        {
            if (cooldownFillImage != null)
            {
                cooldownFillImage.type = Image.Type.Filled;
                cooldownFillImage.fillMethod = fillMethod;
                cooldownFillImage.fillClockwise = clockwiseFill;
            }
        }

        private void Update()
        {
            if (isTrackingCooldown)
            {
                UpdateCooldownDisplay();
            }
        }

        /// <summary>
        /// Sets the skill to display in this slot.
        /// </summary>
        public void SetSkill(ActiveSkillBase skill)
        {
            // Unsubscribe from previous skill events
            if (currentSkill != null)
            {
                currentSkill.OnActivate.RemoveListener(OnSkillActivated);
            }

            currentSkill = skill;

            if (skill != null)
            {
                ShowSkill();
                SubscribeToSkillEvents();
            }
            else
            {
                ShowEmpty();
                isTrackingCooldown = false;
            }
        }

        /// <summary>
        /// Subscribes to skill events for cooldown tracking.
        /// </summary>
        private void SubscribeToSkillEvents()
        {
            if (currentSkill != null)
            {
                currentSkill.OnActivate.AddListener(OnSkillActivated);
            }
        }

        /// <summary>
        /// Called when skill is activated - starts cooldown tracking.
        /// </summary>
        private void OnSkillActivated()
        {
            isTrackingCooldown = true;
        }

        /// <summary>
        /// Displays skill icon and enables cooldown tracking.
        /// </summary>
        private void ShowSkill()
        {
            if (iconImage != null && currentSkill.Data.skillImage != null)
            {
                iconImage.sprite = currentSkill.Data.skillImage;
                iconImage.enabled = true;
            }

            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(false);
            }

            if (cooldownFillImage != null)
            {
                cooldownFillImage.enabled = true;
            }
        }

        /// <summary>
        /// Shows empty slot state.
        /// </summary>
        private void ShowEmpty()
        {
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(true);
            }

            if (cooldownFillImage != null)
            {
                cooldownFillImage.enabled = false;
            }
        }

        /// <summary>
        /// Updates cooldown fill amount based on skill state.
        /// </summary>
        private void UpdateCooldownDisplay()
        {
            if (currentSkill == null || cooldownFillImage == null)
            {
                return;
            }

            if (currentSkill.IsOnCooldown)
            {
                float cooldownPercent = currentSkill.RemainingCooldown / currentSkill.GetModifiedCooldown();
                cooldownFillImage.fillAmount = cooldownPercent;
            }
            else
            {
                // Cooldown finished - stop tracking
                cooldownFillImage.fillAmount = 0f;
                isTrackingCooldown = false;
            }
        }

        /// <summary>
        /// Clears the slot.
        /// </summary>
        public void Clear()
        {
            SetSkill(null);
        }

        private void OnDestroy()
        {
            // Cleanup event subscriptions
            if (currentSkill != null)
            {
                currentSkill.OnActivate.RemoveListener(OnSkillActivated);
            }
        }
    }
}
