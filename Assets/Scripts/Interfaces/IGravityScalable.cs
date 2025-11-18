namespace HandSurvivor.Interfaces
{
    /// <summary>
    /// Interface for objects that need custom gravity scaling to compensate for reduced world gravity.
    /// Useful for throwable objects that should feel like they have normal gravity in a low-gravity environment.
    /// </summary>
    public interface IGravityScalable
    {
        /// <summary>
        /// Gets the gravity multiplier for this object.
        /// Example: If world gravity is divided by 100, use gravityMultiplier = 100 for normal gravity feel.
        /// </summary>
        float GravityMultiplier { get; }

        /// <summary>
        /// Called to apply scaled gravity to the object.
        /// Should be called in FixedUpdate for physics consistency.
        /// </summary>
        void ApplyScaledGravity();
    }
}
