using UnityEngine.Events;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Interface for different active skill activation methods (hand pose, button, voice, etc.)
    /// </summary>
    public interface IActiveSkillActivator
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
