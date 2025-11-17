namespace HandSurvivor
{
    /// <summary>
    /// Interface for objects that need to respond to hand preference changes
    /// Implement this to receive callbacks when the main/off hand is swapped
    /// </summary>
    public interface IHandSwappable
    {
        /// <summary>
        /// Called when the hand preference changes (main hand swapped)
        /// </summary>
        /// <param name="newMainHand">The new main hand type</param>
        void OnHandPreferenceChanged(HandType newMainHand);
    }
}
