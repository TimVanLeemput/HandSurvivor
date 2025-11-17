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
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private GameObject emptySlotIndicator;

        [Header("Visual Settings")]
        [SerializeField] private Image.FillMethod fillMethod = Image.FillMethod.Radial360;
        [SerializeField] private bool clockwiseFill = true;

        private ActiveSkillBase currentSkill;

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
            // Always check cooldown state, not just when tracking
            if (currentSkill != null)
            {
                UpdateCooldownDisplay();
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[SkillSlotUI] Update - currentSkill is NULL!");
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
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[SkillSlotUI] SetSkill called with {skill.Data.displayName} (Instance ID: {skill.GetInstanceID()})");
                ShowSkill();
                SubscribeToSkillEvents();
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[SkillSlotUI] SetSkill called with NULL skill");
                ShowEmpty();
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
                cooldownFillImage.sprite = currentSkill.Data.skillImage;
                cooldownFillImage.enabled = true;
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
                float totalCooldown = currentSkill.GetModifiedCooldown();
                float remaining = currentSkill.RemainingCooldown;
                // Invert: more remaining = less fill, less remaining = more fill
                float cooldownPercent = totalCooldown > 0 ? 1f - (remaining / totalCooldown) : 1f;

                // Debug.Log($"[SkillSlotUI] {currentSkill.Data.displayName} - IsOnCooldown={currentSkill.IsOnCooldown}, Remaining={remaining:F2}s, Total={totalCooldown:F2}s, Percent={cooldownPercent:F2}, FillAmount being set to {cooldownPercent:F2}");

                cooldownFillImage.fillAmount = cooldownPercent;
            }
            else
            {
                // Cooldown finished - show full image
                cooldownFillImage.fillAmount = 1f;
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
