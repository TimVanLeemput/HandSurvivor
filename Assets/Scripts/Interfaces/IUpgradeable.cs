using HandSurvivor.Core.Passive;

namespace HandSurvivor.Upgrades
{
    /// <summary>
    /// Interface for any component that can receive passive upgrades.
    /// Implemented by both ActiveSkills and basic abilities (like XPGrabber).
    /// </summary>
    public interface IUpgradeable
    {
        /// <summary>
        /// Returns the unique identifier used to match this upgradeable with PassiveUpgradeData.targetSkillId
        /// </summary>
        string GetUpgradeableId();

        /// <summary>
        /// Applies a passive upgrade to this component
        /// </summary>
        void ApplyPassiveUpgrade(PassiveUpgradeData upgrade);
    }
}
