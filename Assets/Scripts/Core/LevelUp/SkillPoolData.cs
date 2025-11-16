using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Core.Passive;

namespace HandSurvivor.Core.LevelUp
{
    [CreateAssetMenu(fileName = "SkillPoolData", menuName = "HandSurvivor/Skill Pool Data", order = 0)]
    public class SkillPoolData : ScriptableObject
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Active Skills")]
        [Tooltip("Pool of active skill prefabs that can be offered to player")]
        public List<GameObject> activeSkillPrefabs = new List<GameObject>();

        [Header("Passive Upgrades")]
        [Tooltip("Pool of passive upgrades that can be offered to player")]
        public List<PassiveUpgradeData> passiveUpgrades = new List<PassiveUpgradeData>();

        public List<GameObject> GetRandomActiveSkills(int count)
        {
            if (activeSkillPrefabs.Count == 0)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning("[SkillPoolData] No active skills in pool!");
                return new List<GameObject>();
            }

            int actualCount = Mathf.Min(count, activeSkillPrefabs.Count);

            List<GameObject> shuffled = activeSkillPrefabs.OrderBy(x => Random.value).ToList();
            return shuffled.Take(actualCount).ToList();
        }

        public List<PassiveUpgradeData> GetRandomPassiveUpgrades(int count)
        {
            if (passiveUpgrades.Count == 0)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning("[SkillPoolData] No passive upgrades in pool!");
                return new List<PassiveUpgradeData>();
            }

            int actualCount = Mathf.Min(count, passiveUpgrades.Count);

            List<PassiveUpgradeData> shuffled = passiveUpgrades.OrderBy(x => Random.value).ToList();
            return shuffled.Take(actualCount).ToList();
        }

        private void OnValidate()
        {
            foreach (GameObject prefab in activeSkillPrefabs)
            {
                if (prefab != null && prefab.GetComponent<ActiveSkillBase>() == null)
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                        Debug.LogWarning($"[SkillPoolData] Prefab '{prefab.name}' does not have ActiveSkillBase component!");
                }
            }
        }
    }
}
