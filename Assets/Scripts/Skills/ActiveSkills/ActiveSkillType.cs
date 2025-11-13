namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Defines how an active skill behaves when activated
    /// </summary>
    public enum ActiveSkillType
    {
        /// <summary>
        /// Single use, consumed immediately on activation
        /// </summary>
        OneTime,

        /// <summary>
        /// Active for a duration, then consumed
        /// </summary>
        Duration,

        /// <summary>
        /// Can be toggled on/off, has limited duration when active
        /// </summary>
        Toggle,

        /// <summary>
        /// Permanent upgrade
        /// </summary>
        Permanent
    }
}
