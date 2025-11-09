namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Defines how a power-up behaves when activated
    /// </summary>
    public enum PowerUpType
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
