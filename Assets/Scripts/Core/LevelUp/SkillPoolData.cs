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

        public List<GameObject> GetRandomActiveSkills(int count, List<string> ownedSkillIds = null)
        {
            if (activeSkillPrefabs.Count == 0)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning("[SkillPoolData] No active skills in pool!");
                return new List<GameObject>();
            }

            // Filter out skills player already owns
            List<GameObject> availableSkills = activeSkillPrefabs;
            if (ownedSkillIds != null && ownedSkillIds.Count > 0)
            {
                availableSkills = activeSkillPrefabs.Where(prefab =>
                {
                    ActiveSkillBase skillBase = prefab.GetComponent<ActiveSkillBase>();
                    if (skillBase == null || skillBase.Data == null)
                        return true; // Include if we can't determine ID (edge case)
                    return !ownedSkillIds.Contains(skillBase.Data.activeSkillId);
                }).ToList();

                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[SkillPoolData] Filtered pool: {activeSkillPrefabs.Count} total, {availableSkills.Count} available (excluded {ownedSkillIds.Count} owned)");
            }

            if (availableSkills.Count == 0)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[SkillPoolData] No available active skills (player owns all skills)!");
                return new List<GameObject>();
            }

            int actualCount = Mathf.Min(count, availableSkills.Count);

            List<GameObject> shuffled = availableSkills.OrderBy(x => Random.value).ToList();
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

        /// <summary>
        /// Get random mix of active skills and passive upgrades
        /// </summary>
        public void GetRandomMixedSelection(int count, List<string> ownedSkillIds,
            out List<GameObject> selectedSkills, out List<PassiveUpgradeData> selectedUpgrades)
        {
            selectedSkills = new List<GameObject>();
            selectedUpgrades = new List<PassiveUpgradeData>();

            // Get available skills (filtered by owned)
            List<GameObject> availableSkills = GetRandomActiveSkills(count, ownedSkillIds);

            // Get available upgrades
            List<PassiveUpgradeData> availableUpgrades = GetRandomPassiveUpgrades(count);

            // Combine both pools
            List<object> combinedPool = new List<object>();
            combinedPool.AddRange(availableSkills.Cast<object>());
            combinedPool.AddRange(availableUpgrades.Cast<object>());

            if (combinedPool.Count == 0)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[SkillPoolData] No skills or upgrades available for mixed selection!");
                return;
            }

            // Shuffle and take requested count
            int actualCount = Mathf.Min(count, combinedPool.Count);
            List<object> shuffled = combinedPool.OrderBy(x => Random.value).ToList();
            List<object> selected = shuffled.Take(actualCount).ToList();

            // Separate back into skills and upgrades
            foreach (object item in selected)
            {
                if (item is GameObject skill)
                    selectedSkills.Add(skill);
                else if (item is PassiveUpgradeData upgrade)
                    selectedUpgrades.Add(upgrade);
            }

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[SkillPoolData] Mixed selection: {selectedSkills.Count} skills + {selectedUpgrades.Count} upgrades = {selected.Count} total");
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
