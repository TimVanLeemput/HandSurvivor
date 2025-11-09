namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Defines the curl state of a finger for pose detection
    /// </summary>
    public enum FingerState
    {
        /// <summary>
        /// Any state - this finger doesn't matter for the pose
        /// </summary>
        Any,

        /// <summary>
        /// Finger must be extended/straight
        /// </summary>
        Extended,

        /// <summary>
        /// Finger must be curled/bent
        /// </summary>
        Curled
    }

    /// <summary>
    /// Defines required state for all fingers in a hand pose
    /// </summary>
    [System.Serializable]
    public class HandPoseDefinition
    {
        public FingerState thumb = FingerState.Any;
        public FingerState index = FingerState.Any;
        public FingerState middle = FingerState.Any;
        public FingerState ring = FingerState.Any;
        public FingerState pinky = FingerState.Any;

        /// <summary>
        /// Tolerance for curl detection (0-1, higher = more lenient)
        /// </summary>
        public float curlTolerance = 0.3f;

        /// <summary>
        /// Predefined pose: Finger Gun (index extended, others curled)
        /// </summary>
        public static HandPoseDefinition FingerGun => new HandPoseDefinition
        {
            thumb = FingerState.Extended,
            index = FingerState.Extended,
            middle = FingerState.Curled,
            ring = FingerState.Curled,
            pinky = FingerState.Curled,
            curlTolerance = 0.35f
        };

        /// <summary>
        /// Predefined pose: Open Palm (all fingers extended)
        /// </summary>
        public static HandPoseDefinition OpenPalm => new HandPoseDefinition
        {
            thumb = FingerState.Extended,
            index = FingerState.Extended,
            middle = FingerState.Extended,
            ring = FingerState.Extended,
            pinky = FingerState.Extended,
            curlTolerance = 0.3f
        };

        /// <summary>
        /// Predefined pose: Fist (all fingers curled)
        /// </summary>
        public static HandPoseDefinition Fist => new HandPoseDefinition
        {
            thumb = FingerState.Curled,
            index = FingerState.Curled,
            middle = FingerState.Curled,
            ring = FingerState.Curled,
            pinky = FingerState.Curled,
            curlTolerance = 0.3f
        };

        /// <summary>
        /// Predefined pose: Two Fingers (index and middle extended)
        /// </summary>
        public static HandPoseDefinition TwoFingers => new HandPoseDefinition
        {
            thumb = FingerState.Curled,
            index = FingerState.Extended,
            middle = FingerState.Extended,
            ring = FingerState.Curled,
            pinky = FingerState.Curled,
            curlTolerance = 0.3f
        };
    }
}
