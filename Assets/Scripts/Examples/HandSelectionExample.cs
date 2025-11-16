using UnityEngine;

namespace HandSurvivor.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the hand selection system in gameplay.
    /// This shows various ways to query and respond to hand preferences.
    /// </summary>
    public class HandSelectionExample : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Example Settings")]
        [SerializeField]
        private bool logOnStart = true;

        private void Start()
        {
            if (logOnStart)
            {
                LogCurrentHandSetup();
            }

            // Subscribe to hand changes if needed
            HandSelectionManager.Instance.OnMainHandChanged.AddListener(OnHandChanged);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (HandSelectionManager.Instance != null)
            {
                HandSelectionManager.Instance.OnMainHandChanged.RemoveListener(OnHandChanged);
            }
        }

        /// <summary>
        /// Example: How to check which hand is main/off hand
        /// </summary>
        private void LogCurrentHandSetup()
        {
            HandType mainHand = HandSelectionManager.GetMainHand();
            HandType offHand = HandSelectionManager.GetOffHand();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


                Debug.Log($"[HandExample] Current Setup:\n" +
                     $"  Main Hand (Physical Damage): {mainHand}\n" +
                     $"  Off Hand (Spirit Abilities): {offHand}");
        }

        /// <summary>
        /// Example: React when player changes hand preference
        /// </summary>
        private void OnHandChanged(HandType newMainHand)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[HandExample] Player changed main hand to: {newMainHand}");

            // Example: Update weapon attachments, ability positions, etc.
            UpdateGameplayElements();
        }

        /// <summary>
        /// Example: How to handle hand-specific actions in gameplay
        /// </summary>
        public void OnHandTriggerPressed(HandType hand)
        {
            // Check if this is the main hand (physical damage)
            if (HandSelectionManager.CheckIsMainHand(hand))
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[HandExample] {hand} hand trigger pressed - PHYSICAL DAMAGE ATTACK!");
                PerformPhysicalAttack();
            }
            // Check if this is the off hand (spirit abilities)
            else if (HandSelectionManager.CheckIsOffHand(hand))
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[HandExample] {hand} hand trigger pressed - SPIRIT ABILITY!");
                ActivateSpiritAbility();
            }
        }

        /// <summary>
        /// Example: Physical damage attack for main hand
        /// </summary>
        private void PerformPhysicalAttack()
        {
            // Implement physical damage logic here
            // Example: Spawn punch VFX, apply damage to enemies, etc.
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log("[HandExample] Executing physical damage attack...");
        }

        /// <summary>
        /// Example: Spirit ability for off hand
        /// </summary>
        private void ActivateSpiritAbility()
        {
            // Implement spirit ability logic here
            // Example: Collect powerups, launch spirit projectiles, etc.
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log("[HandExample] Activating spirit ability...");
        }

        /// <summary>
        /// Example: Update gameplay elements based on hand roles
        /// </summary>
        private void UpdateGameplayElements()
        {
            HandType mainHand = HandSelectionManager.GetMainHand();
            HandType offHand = HandSelectionManager.GetOffHand();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


                Debug.Log($"[HandExample] Updating gameplay elements:\n" +
                     $"  Attaching physical weapon to {mainHand} hand\n" +
                     $"  Attaching spirit collector to {offHand} hand");

            // Example implementation:
            // - Move weapon model to main hand
            // - Move spirit powerup collector to off hand
            // - Update UI indicators
            // - Reconfigure input mappings
        }

        /// <summary>
        /// Example: Get hand role for conditional logic
        /// </summary>
        public void OnPowerupCollected(HandType collectingHand)
        {
            HandRole role = HandSelectionManager.Instance.GetHandRole(collectingHand);

            if (role == HandRole.OffHand)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[HandExample] Powerup collected with OFF HAND ({collectingHand}) - BONUS POINTS!");
                // Give bonus for using correct hand
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[HandExample] Powerup collected with MAIN HAND ({collectingHand}) - standard points");
                // Standard collection
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Example: Simulate Left Hand Action")]
        private void DebugSimulateLeftHandAction()
        {
            OnHandTriggerPressed(HandType.Left);
        }

        [ContextMenu("Example: Simulate Right Hand Action")]
        private void DebugSimulateRightHandAction()
        {
            OnHandTriggerPressed(HandType.Right);
        }

        [ContextMenu("Example: Log Current Setup")]
        private void DebugLogSetup()
        {
            LogCurrentHandSetup();
        }
#endif
    }
}
