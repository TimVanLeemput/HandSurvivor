using System.Collections.Generic;
using HandSurvivor.ActiveSkills;
using UnityEngine;

namespace HandSurvivor.Skills
{
    /// <summary>
    /// Manages the skill slots UI container.
    /// Dynamically creates slot UI elements based on ActiveSkillSlotManager.MaxSlots.
    /// Updates in real-time to reflect slotted skills and their cooldown states.
    /// </summary>
    public class SkillSlotsUIManager : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private GameObject skillSlotUIPrefab;

        [Header("Container")]
        [SerializeField] private Transform slotsContainer;

        private List<SkillSlotUI> slotUIElements = new List<SkillSlotUI>();
        private int currentMaxSlots = 0;

        private void Start()
        {
            InitializeSlots();
            SubscribeToEvents();
            RefreshAllSlots();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Creates slot UI elements matching MaxSlots configuration.
        /// </summary>
        private void InitializeSlots()
        {
            if (ActiveSkillSlotManager.Instance == null)
            {
                Debug.LogWarning("[SkillSlotsUIManager] ActiveSkillSlotManager.Instance is null!");
                return;
            }

            currentMaxSlots = ActiveSkillSlotManager.Instance.MaxSlots;

            for (int i = 0; i < currentMaxSlots; i++)
            {
                CreateSlotUI();
            }

            Debug.Log($"[SkillSlotsUIManager] Initialized {currentMaxSlots} skill slot UI elements");
        }

        /// <summary>
        /// Instantiates a single slot UI element.
        /// </summary>
        private void CreateSlotUI()
        {
            if (skillSlotUIPrefab == null || slotsContainer == null)
            {
                Debug.LogError("[SkillSlotsUIManager] Missing prefab or container reference!");
                return;
            }

            GameObject slotObj = Instantiate(skillSlotUIPrefab, slotsContainer);
            SkillSlotUI slotUI = slotObj.GetComponent<SkillSlotUI>();

            if (slotUI != null)
            {
                slotUIElements.Add(slotUI);
            }
            else
            {
                Debug.LogError("[SkillSlotsUIManager] SkillSlotUI component not found on prefab!");
            }
        }

        /// <summary>
        /// Subscribes to slot manager events.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (ActiveSkillSlotManager.Instance != null)
            {
                ActiveSkillSlotManager.Instance.OnSkillSlotted.AddListener(OnSkillSlotted);
                ActiveSkillSlotManager.Instance.OnSkillRemoved.AddListener(OnSkillRemoved);
            }
        }

        /// <summary>
        /// Unsubscribes from slot manager events.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (ActiveSkillSlotManager.Instance != null)
            {
                ActiveSkillSlotManager.Instance.OnSkillSlotted.RemoveListener(OnSkillSlotted);
                ActiveSkillSlotManager.Instance.OnSkillRemoved.RemoveListener(OnSkillRemoved);
            }
        }

        /// <summary>
        /// Called when a skill is slotted.
        /// </summary>
        private void OnSkillSlotted(ActiveSkillBase skill)
        {
            RefreshAllSlots();
        }

        /// <summary>
        /// Called when a skill is removed from slots.
        /// </summary>
        private void OnSkillRemoved(ActiveSkillBase skill)
        {
            RefreshAllSlots();
        }

        /// <summary>
        /// Refreshes all slot displays to match current slot state.
        /// </summary>
        private void RefreshAllSlots()
        {
            if (ActiveSkillSlotManager.Instance == null)
            {
                return;
            }

            List<ActiveSkillBase> slottedSkills = ActiveSkillSlotManager.Instance.GetSlottedSkills();

            for (int i = 0; i < slotUIElements.Count; i++)
            {
                if (i < slottedSkills.Count)
                {
                    slotUIElements[i].SetSkill(slottedSkills[i]);
                }
                else
                {
                    slotUIElements[i].Clear();
                }
            }
        }

        /// <summary>
        /// Rebuilds slot UI if MaxSlots configuration changes at runtime.
        /// </summary>
        public void RebuildSlots()
        {
            if (ActiveSkillSlotManager.Instance == null)
            {
                return;
            }

            int newMaxSlots = ActiveSkillSlotManager.Instance.MaxSlots;

            if (newMaxSlots == currentMaxSlots)
            {
                return;
            }

            // Clear existing slots
            foreach (SkillSlotUI slotUI in slotUIElements)
            {
                if (slotUI != null)
                {
                    Destroy(slotUI.gameObject);
                }
            }
            slotUIElements.Clear();

            // Recreate with new count
            currentMaxSlots = newMaxSlots;
            InitializeSlots();
            RefreshAllSlots();

            Debug.Log($"[SkillSlotsUIManager] Rebuilt UI with {currentMaxSlots} slots");
        }
    }
}
