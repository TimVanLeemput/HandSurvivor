using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor.Stats
{
    /// <summary>
    /// Statistics for the current gameplay session
    /// Resets when a new run starts
    /// </summary>
    [Serializable]
    public class PlayerRunStats
    {
        [Header("Combat Stats")]
        public float totalDamageDealt;
        public SerializableDictionary<string, float> damageBySkill = new SerializableDictionary<string, float>();
        public int totalKills;
        public SerializableDictionary<string, int> killsByEnemyType = new SerializableDictionary<string, int>();
        public int skillActivations;
        public SerializableDictionary<string, int> activationsBySkill = new SerializableDictionary<string, int>();

        [Header("Survival Stats")]
        public float survivalTime;
        public int currentWave;
        public int damageToNexus;

        [Header("Progression Stats")]
        public int currentLevel;
        public int xpEarnedThisRun;
        public List<string> skillsAcquiredThisRun = new List<string>();
        public int passiveUpgradesApplied;

        [Header("Performance Stats")]
        public float highestSingleHit;

        /// <summary>
        /// Reset all stats for a new run
        /// </summary>
        public void Reset()
        {
            totalDamageDealt = 0f;
            damageBySkill.Clear();
            totalKills = 0;
            killsByEnemyType.Clear();
            skillActivations = 0;
            activationsBySkill.Clear();

            survivalTime = 0f;
            currentWave = 0;
            damageToNexus = 0;

            currentLevel = 1;
            xpEarnedThisRun = 0;
            skillsAcquiredThisRun.Clear();
            passiveUpgradesApplied = 0;

            highestSingleHit = 0f;
        }

        /// <summary>
        /// Add damage to tracking
        /// </summary>
        public void AddDamage(string skillId, float damage)
        {
            totalDamageDealt += damage;

            if (!string.IsNullOrEmpty(skillId))
            {
                if (!damageBySkill.ContainsKey(skillId))
                    damageBySkill[skillId] = 0f;
                damageBySkill[skillId] += damage;
            }

            if (damage > highestSingleHit)
                highestSingleHit = damage;
        }

        /// <summary>
        /// Add kill to tracking
        /// </summary>
        public void AddKill(string enemyType)
        {
            totalKills++;

            if (!string.IsNullOrEmpty(enemyType))
            {
                if (!killsByEnemyType.ContainsKey(enemyType))
                    killsByEnemyType[enemyType] = 0;
                killsByEnemyType[enemyType]++;
            }
        }

        /// <summary>
        /// Add skill activation to tracking
        /// </summary>
        public void AddSkillActivation(string skillId)
        {
            skillActivations++;

            if (!string.IsNullOrEmpty(skillId))
            {
                if (!activationsBySkill.ContainsKey(skillId))
                    activationsBySkill[skillId] = 0;
                activationsBySkill[skillId]++;
            }
        }
    }

    /// <summary>
    /// Serializable dictionary for Unity Inspector and JSON serialization
    /// Unity doesn't serialize Dictionary by default
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public TValue this[TKey key]
        {
            get
            {
                int index = keys.IndexOf(key);
                if (index >= 0 && index < values.Count)
                    return values[index];
                throw new KeyNotFoundException($"Key '{key}' not found in dictionary");
            }
            set
            {
                int index = keys.IndexOf(key);
                if (index >= 0)
                {
                    values[index] = value;
                }
                else
                {
                    keys.Add(key);
                    values.Add(value);
                }
            }
        }

        public bool ContainsKey(TKey key) => keys.Contains(key);

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public int Count => keys.Count;

        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }

        public void FromDictionary(Dictionary<TKey, TValue> dict)
        {
            Clear();
            foreach (KeyValuePair<TKey, TValue> kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
    }
}
