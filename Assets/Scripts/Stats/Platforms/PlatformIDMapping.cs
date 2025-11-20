using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor.Stats.Platforms
{
    /// <summary>
    /// ScriptableObject for mapping internal achievement IDs to platform-specific IDs
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformIDMapping", menuName = "HandSurvivor/Platform ID Mapping", order = 101)]
    public class PlatformIDMapping : ScriptableObject
    {
        [Header("Platform Mappings")]
        [SerializeField] private List<AchievementPlatformMapping> mappings = new List<AchievementPlatformMapping>();

        /// <summary>
        /// Get platform-specific ID for an achievement
        /// </summary>
        public string GetPlatformID(string achievementId, string platformName)
        {
            AchievementPlatformMapping mapping = mappings.Find(m => m.achievementId == achievementId);

            if (mapping == null)
                return string.Empty;

            switch (platformName.ToLower())
            {
                case "steam":
                    return mapping.steamID;
                case "meta":
                    return mapping.metaID;
                case "custom":
                    return mapping.customID;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Add a new mapping
        /// </summary>
        public void AddMapping(string achievementId, string steamID = "", string metaID = "", string customID = "")
        {
            AchievementPlatformMapping existing = mappings.Find(m => m.achievementId == achievementId);

            if (existing != null)
            {
                existing.steamID = steamID;
                existing.metaID = metaID;
                existing.customID = customID;
            }
            else
            {
                mappings.Add(new AchievementPlatformMapping
                {
                    achievementId = achievementId,
                    steamID = steamID,
                    metaID = metaID,
                    customID = customID
                });
            }
        }

        /// <summary>
        /// Remove a mapping
        /// </summary>
        public void RemoveMapping(string achievementId)
        {
            mappings.RemoveAll(m => m.achievementId == achievementId);
        }

        /// <summary>
        /// Get all mappings
        /// </summary>
        public List<AchievementPlatformMapping> GetAllMappings()
        {
            return new List<AchievementPlatformMapping>(mappings);
        }
    }

    /// <summary>
    /// Individual achievement platform mapping
    /// </summary>
    [Serializable]
    public class AchievementPlatformMapping
    {
        [Tooltip("Internal achievement ID (matches Achievement.achievementId)")]
        public string achievementId;

        [Header("Platform IDs")]
        [Tooltip("Steam achievement ID (e.g., ACH_FIRST_KILL)")]
        public string steamID;

        [Tooltip("Meta Platform achievement ID")]
        public string metaID;

        [Tooltip("Custom platform achievement ID")]
        public string customID;
    }
}
