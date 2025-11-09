using UnityEngine.Events;

namespace HandSurvivor.PowerUps
{
    /// <summary>
    /// Interface for different power-up activation methods (hand pose, button, voice, etc.)
    /// </summary>
    public interface IPowerUpActivator
    {
        /// <summary>
        /// Event fired when activation condition is met
        /// </summary>
        UnityEvent OnActivationTriggered { get; }

        /// <summary>
        /// Event fired when deactivation condition is met (for toggle types)
        /// </summary>
        UnityEvent OnDeactivationTriggered { get; }

        /// <summary>
        /// Initialize the activator with necessary components
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start listening for activation input
        /// </summary>
        void Enable();

        /// <summary>
        /// Stop listening for activation input
        /// </summary>
        void Disable();

        /// <summary>
        /// Clean up resources
        /// </summary>
        void Cleanup();
    }
}
