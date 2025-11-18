namespace HandSurvivor.Interfaces
{
    /// <summary>
    /// Interface for skills that have a charge system
    /// </summary>
    public interface IChargeableSkill
    {
        /// <summary>
        /// Gets the current maximum number of charges for this skill
        /// </summary>
        int GetMaxCharges();

        /// <summary>
        /// Gets the current available charges
        /// </summary>
        int GetCurrentCharges();
    }
}
