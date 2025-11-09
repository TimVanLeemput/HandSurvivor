namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Predefined hand poses for power-up activation
    /// Can be extended with custom poses
    /// </summary>
    public enum HandPoseType
    {
        /// <summary>
        /// Index finger extended, other fingers curled (finger gun)
        /// Used for laser power-up
        /// </summary>
        FingerGun,

        /// <summary>
        /// All fingers extended (open palm)
        /// </summary>
        OpenPalm,

        /// <summary>
        /// All fingers curled (fist)
        /// </summary>
        Fist,

        /// <summary>
        /// Index and middle fingers extended (peace sign / two fingers)
        /// </summary>
        TwoFingers,

        /// <summary>
        /// Thumb and pinky extended (rock/shaka sign)
        /// </summary>
        RockSign,

        /// <summary>
        /// Index finger and thumb forming circle (OK sign)
        /// </summary>
        OKSign,

        /// <summary>
        /// Custom pose defined by component
        /// </summary>
        Custom
    }
}
