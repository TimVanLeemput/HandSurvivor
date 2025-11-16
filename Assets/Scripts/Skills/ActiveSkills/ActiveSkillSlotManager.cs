using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor.ActiveSkills
{
    public class ActiveSkillSlotManager : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         public static ActiveSkillSlotManager Instance;

        [Header("Slot Configuration")]
        [SerializeField] private int maxSlots = 4;

        [Header("Events")]
        public UnityEvent<ActiveSkillBase> OnSkillSlotted;
        public UnityEvent<ActiveSkillBase> OnSkillRemoved;
        public UnityEvent OnAllSlotsFilled;

        private List<ActiveSkillBase> slots = new List<ActiveSkillBase>();

        public int MaxSlots => maxSlots;
        public int GetFilledSlotCount() => slots.Count;
        public bool AreAllSlotsFilled() => slots.Count >= maxSlots;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool CanAddSkill()
        {
            bool canAdd = slots.Count < maxSlots;
            if (!canAdd)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning($"[ActiveSkillSlotManager] Cannot add skill - all {maxSlots} slots are full!");
            }
            return canAdd;
        }

        public bool TryAddSkill(ActiveSkillBase skill)
        {
            if (skill == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning("[ActiveSkillSlotManager] Cannot add null skill!");
                return false;
            }

            if (!CanAddSkill())
            {
                return false;
            }

            if (slots.Contains(skill))
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning($"[ActiveSkillSlotManager] Skill {skill.Data.displayName} is already slotted!");
                return false;
            }

            slots.Add(skill);
            skill.transform.SetParent(transform);

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


                Debug.Log($"[ActiveSkillSlotManager] Slotted skill: {skill.Data.displayName} (Slot {slots.Count}/{maxSlots})");

            OnSkillSlotted?.Invoke(skill);

            if (AreAllSlotsFilled())
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[ActiveSkillSlotManager] All {maxSlots} slots are now filled!");
                OnAllSlotsFilled?.Invoke();
            }

            return true;
        }

        public bool RemoveSkill(ActiveSkillBase skill)
        {
            if (skill == null)
            {
                return false;
            }

            if (slots.Remove(skill))
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[ActiveSkillSlotManager] Removed skill: {skill.Data.displayName}");
                OnSkillRemoved?.Invoke(skill);
                return true;
            }

            return false;
        }

        public List<ActiveSkillBase> GetSlottedSkills()
        {
            return new List<ActiveSkillBase>(slots);
        }

        public ActiveSkillBase GetSkillAtSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < slots.Count)
            {
                return slots[slotIndex];
            }
            return null;
        }

        public int GetSlotIndex(ActiveSkillBase skill)
        {
            return slots.IndexOf(skill);
        }

        public bool HasSkill(ActiveSkillBase skill)
        {
            return slots.Contains(skill);
        }

        public void ClearAllSlots()
        {
            List<ActiveSkillBase> slotsCopy = new List<ActiveSkillBase>(slots);

            foreach (ActiveSkillBase skill in slotsCopy)
            {
                if (skill != null)
                {
                    Destroy(skill.gameObject);
                }
            }

            slots.Clear();
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log("[ActiveSkillSlotManager] All slots cleared");
        }
    }
}
