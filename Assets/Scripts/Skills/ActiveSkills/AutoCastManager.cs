using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor.ActiveSkills
{
    public class AutoCastManager : MonoBehaviour
    {
        public static AutoCastManager Instance;

        [Header("Settings")]
        [SerializeField] private float checkInterval = 0.1f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private List<ActiveSkillBase> trackedSkills = new List<ActiveSkillBase>();
        private float lastCheckTime = 0f;

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

        private void Start()
        {
            if (ActiveSkillSlotManager.Instance != null)
            {
                ActiveSkillSlotManager.Instance.OnSkillSlotted.AddListener(OnSkillSlotted);
                ActiveSkillSlotManager.Instance.OnSkillRemoved.AddListener(OnSkillRemoved);

                List<ActiveSkillBase> existingSkills = ActiveSkillSlotManager.Instance.GetSlottedSkills();
                foreach (ActiveSkillBase skill in existingSkills)
                {
                    TrackSkill(skill);
                }
            }
            else
            {
                Debug.LogError("[AutoCastManager] ActiveSkillSlotManager.Instance is null!");
            }
        }

        private void OnDestroy()
        {
            if (ActiveSkillSlotManager.Instance != null)
            {
                ActiveSkillSlotManager.Instance.OnSkillSlotted.RemoveListener(OnSkillSlotted);
                ActiveSkillSlotManager.Instance.OnSkillRemoved.RemoveListener(OnSkillRemoved);
            }
        }

        private void Update()
        {
            if (Time.time - lastCheckTime >= checkInterval)
            {
                lastCheckTime = Time.time;
                CheckAndActivateSkills();
            }
        }

        private void OnSkillSlotted(ActiveSkillBase skill)
        {
            TrackSkill(skill);
        }

        private void OnSkillRemoved(ActiveSkillBase skill)
        {
            UntrackSkill(skill);
        }

        private void TrackSkill(ActiveSkillBase skill)
        {
            if (skill != null && !trackedSkills.Contains(skill))
            {
                trackedSkills.Add(skill);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AutoCastManager] Now tracking: {skill.Data.displayName}");
                }
            }
        }

        private void UntrackSkill(ActiveSkillBase skill)
        {
            if (skill != null && trackedSkills.Contains(skill))
            {
                trackedSkills.Remove(skill);

                if (enableDebugLogs)
                {
                    Debug.Log($"[AutoCastManager] Stopped tracking: {skill.Data.displayName}");
                }
            }
        }

        private void CheckAndActivateSkills()
        {
            for (int i = trackedSkills.Count - 1; i >= 0; i--)
            {
                ActiveSkillBase skill = trackedSkills[i];

                if (skill == null)
                {
                    trackedSkills.RemoveAt(i);
                    continue;
                }

                // Use CanActivate() to determine if skill should fire
                // (handles Duration skills that can overlap)
                if (skill.CanActivate())
                {
                    skill.TryActivate();

                    if (enableDebugLogs)
                    {
                        Debug.Log($"[AutoCastManager] Auto-activated: {skill.Data.displayName}");
                    }
                }
            }
        }

        public void SetCheckInterval(float interval)
        {
            checkInterval = Mathf.Max(0.05f, interval);
        }

        public List<ActiveSkillBase> GetTrackedSkills()
        {
            return new List<ActiveSkillBase>(trackedSkills);
        }

        public void SetDebugMode(bool enabled)
        {
            enableDebugLogs = enabled;
        }
    }
}
