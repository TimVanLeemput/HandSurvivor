using UnityEngine;
using HandSurvivor.UI;

namespace HandSurvivor.VR
{
    /// <summary>
    /// Triggers the VR suction effect when the Nexus is destroyed,
    /// then shows the GameOver UI after the sequence completes.
    /// </summary>
    public class GameOverSuctionTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Nexus nexus;
        [SerializeField] private VRSuctionController suctionController;
        [SerializeField] private SuctionVisualEffects visualEffects;

        [Header("Suction Target")]
        [Tooltip("Where the player gets sucked into. If null, uses Nexus position.")]
        [SerializeField] private Transform suctionTarget;
        [SerializeField] private GameObject suctionTargetVisualPrefab;

        [Header("Settings")]
        [SerializeField] private float delayBeforeGameOver = 0.5f;

        private GameObject _spawnedTargetVisual;
        private bool _triggered;

        private void Start()
        {
            // Auto-find components if not assigned
            if (nexus == null)
                nexus = FindFirstObjectByType<Nexus>();

            if (suctionController == null)
                suctionController = FindFirstObjectByType<VRSuctionController>();

            if (visualEffects == null && suctionController != null)
                visualEffects = suctionController.GetComponent<SuctionVisualEffects>();

            // Subscribe to Nexus destruction
            if (nexus != null)
            {
                nexus.OnNexusDestroyed.AddListener(OnNexusDestroyed);
            }
            else
            {
                Debug.LogWarning("[GameOverSuctionTrigger] No Nexus found!");
            }
        }

        private void OnNexusDestroyed()
        {
            if (_triggered) return;
            _triggered = true;

            // Determine suction target
            Transform target = suctionTarget;
            if (target == null && nexus != null)
            {
                target = nexus.transform;
            }

            if (target == null)
            {
                Debug.LogError("[GameOverSuctionTrigger] No suction target!");
                TriggerGameOverDirectly();
                return;
            }

            // Spawn visual at target if we have a prefab
            if (suctionTargetVisualPrefab != null)
            {
                _spawnedTargetVisual = Instantiate(suctionTargetVisualPrefab, target.position, Quaternion.identity);
            }

            // Start suction
            if (suctionController != null)
            {
                suctionController.OnSuctionComplete.AddListener(OnSuctionComplete);
                suctionController.StartSuction(target);
            }
            else
            {
                Debug.LogWarning("[GameOverSuctionTrigger] No VRSuctionController - triggering game over directly");
                TriggerGameOverDirectly();
            }
        }

        private void OnSuctionComplete()
        {
            suctionController.OnSuctionComplete.RemoveListener(OnSuctionComplete);

            // Cleanup spawned visual
            if (_spawnedTargetVisual != null)
            {
                Destroy(_spawnedTargetVisual);
            }

            // Delay slightly before showing UI
            Invoke(nameof(TriggerGameOverDirectly), delayBeforeGameOver);
        }

        private void TriggerGameOverDirectly()
        {
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.TriggerGameOver();
            }
            else
            {
                Debug.LogError("[GameOverSuctionTrigger] GameOverManager not found!");
            }

            // Reset effects after game over is triggered
            if (visualEffects != null)
            {
                visualEffects.ResetEffects();
            }
        }

        private void OnDestroy()
        {
            if (nexus != null)
            {
                nexus.OnNexusDestroyed.RemoveListener(OnNexusDestroyed);
            }

            if (_spawnedTargetVisual != null)
            {
                Destroy(_spawnedTargetVisual);
            }
        }
    }
}
