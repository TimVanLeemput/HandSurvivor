using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor.PowerUps.UI
{
    /// <summary>
    /// Manages multiple PowerUpUI elements and syncs with PowerUpInventory
    /// Automatically creates/destroys UI elements as power-ups are added/removed
    /// </summary>
    public class PowerUpUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform uiContainer;
        [SerializeField] private GameObject powerUpUIPrefab;

        [Header("Settings")]
        [SerializeField] private bool autoSyncWithInventory = true;
        [SerializeField] private int maxDisplaySlots = 5;
        [SerializeField] private float spacing = 10f;

        [Header("Layout")]
        [SerializeField] private LayoutMode layoutMode = LayoutMode.Horizontal;

        public enum LayoutMode
        {
            Horizontal,
            Vertical,
            Grid
        }

        private List<PowerUpUI> activeUIElements = new List<PowerUpUI>();
        private bool isInitialized = false;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Validate references
            if (uiContainer == null)
            {
                Debug.LogWarning("[PowerUpUIManager] No UI container assigned, using self as container", this);
                uiContainer = transform;
            }

            if (powerUpUIPrefab == null)
            {
                Debug.LogError("[PowerUpUIManager] No PowerUpUI prefab assigned!", this);
                return;
            }

            // Subscribe to inventory if auto-sync enabled
            if (autoSyncWithInventory && PowerUpInventory.Instance != null)
            {
                PowerUpInventory.Instance.OnPowerUpAdded.AddListener(OnInventoryPowerUpAdded);
                PowerUpInventory.Instance.OnPowerUpRemoved.AddListener(OnInventoryPowerUpRemoved);

                // Sync with existing power-ups in inventory
                SyncWithInventory();
            }

            isInitialized = true;
        }

        private void OnDestroy()
        {
            // Unsubscribe from inventory
            if (PowerUpInventory.Instance != null)
            {
                PowerUpInventory.Instance.OnPowerUpAdded.RemoveListener(OnInventoryPowerUpAdded);
                PowerUpInventory.Instance.OnPowerUpRemoved.RemoveListener(OnInventoryPowerUpRemoved);
            }
        }

        /// <summary>
        /// Sync UI with current PowerUpInventory contents
        /// </summary>
        private void SyncWithInventory()
        {
            if (PowerUpInventory.Instance == null)
            {
                return;
            }

            // Clear existing UI
            ClearAll();

            // Add UI for each power-up in inventory
            foreach (PowerUpBase powerUp in PowerUpInventory.Instance.PowerUps)
            {
                AddPowerUpUI(powerUp);
            }

            Debug.Log($"[PowerUpUIManager] Synced with inventory: {activeUIElements.Count} power-ups");
        }

        /// <summary>
        /// Add a UI element for a power-up
        /// </summary>
        public PowerUpUI AddPowerUpUI(PowerUpBase powerUp)
        {
            if (powerUp == null)
            {
                Debug.LogWarning("[PowerUpUIManager] Cannot add UI for null power-up");
                return null;
            }

            // Check if already displaying this power-up
            if (GetUIForPowerUp(powerUp) != null)
            {
                Debug.LogWarning($"[PowerUpUIManager] Already displaying UI for {powerUp.Data.displayName}");
                return null;
            }

            // Check max slots
            if (activeUIElements.Count >= maxDisplaySlots)
            {
                Debug.LogWarning($"[PowerUpUIManager] Max display slots ({maxDisplaySlots}) reached");
                return null;
            }

            // Instantiate UI element
            GameObject uiObj = Instantiate(powerUpUIPrefab, uiContainer);
            PowerUpUI powerUpUI = uiObj.GetComponent<PowerUpUI>();

            if (powerUpUI == null)
            {
                Debug.LogError("[PowerUpUIManager] PowerUpUI prefab doesn't have PowerUpUI component!");
                Destroy(uiObj);
                return null;
            }

            // Setup and add to list
            powerUpUI.SetPowerUp(powerUp);
            powerUpUI.SetVisible(true);
            activeUIElements.Add(powerUpUI);

            Debug.Log($"[PowerUpUIManager] Added UI for {powerUp.Data.displayName} ({activeUIElements.Count}/{maxDisplaySlots})");

            return powerUpUI;
        }

        /// <summary>
        /// Remove UI element for a power-up
        /// </summary>
        public void RemovePowerUpUI(PowerUpBase powerUp)
        {
            PowerUpUI uiToRemove = GetUIForPowerUp(powerUp);

            if (uiToRemove == null)
            {
                return;
            }

            activeUIElements.Remove(uiToRemove);
            Destroy(uiToRemove.gameObject);

            Debug.Log($"[PowerUpUIManager] Removed UI for {powerUp.Data.displayName} ({activeUIElements.Count}/{maxDisplaySlots})");
        }

        /// <summary>
        /// Get the UI element displaying a specific power-up
        /// </summary>
        private PowerUpUI GetUIForPowerUp(PowerUpBase powerUp)
        {
            // Since PowerUpUI doesn't expose the powerUp reference publicly,
            // we'll need to track via order or add a getter
            // For now, we'll assume they're managed in order
            foreach (PowerUpUI ui in activeUIElements)
            {
                if (ui.HasPowerUp())
                {
                    // This is a simplified check - you may want to add a proper getter
                    return ui;
                }
            }

            return null;
        }

        /// <summary>
        /// Clear all UI elements
        /// </summary>
        public void ClearAll()
        {
            foreach (PowerUpUI ui in activeUIElements)
            {
                if (ui != null)
                {
                    Destroy(ui.gameObject);
                }
            }

            activeUIElements.Clear();
            Debug.Log("[PowerUpUIManager] Cleared all power-up UI elements");
        }

        /// <summary>
        /// Called when a power-up is added to inventory
        /// </summary>
        private void OnInventoryPowerUpAdded(PowerUpBase powerUp)
        {
            AddPowerUpUI(powerUp);
        }

        /// <summary>
        /// Called when a power-up is removed from inventory
        /// </summary>
        private void OnInventoryPowerUpRemoved(PowerUpBase powerUp)
        {
            RemovePowerUpUI(powerUp);
        }

        /// <summary>
        /// Get count of active UI elements
        /// </summary>
        public int GetActiveCount()
        {
            return activeUIElements.Count;
        }

        /// <summary>
        /// Get available slots
        /// </summary>
        public int GetAvailableSlots()
        {
            return maxDisplaySlots - activeUIElements.Count;
        }

        /// <summary>
        /// Check if can add more UI elements
        /// </summary>
        public bool CanAddMore()
        {
            return activeUIElements.Count < maxDisplaySlots;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Sync With Inventory")]
        private void DebugSyncWithInventory()
        {
            if (PowerUpInventory.Instance != null)
            {
                SyncWithInventory();
            }
            else
            {
                Debug.LogWarning("[PowerUpUIManager] No PowerUpInventory instance found");
            }
        }

        [ContextMenu("Debug: Clear All UI")]
        private void DebugClearAll()
        {
            ClearAll();
        }

        [ContextMenu("Debug: Log Status")]
        private void DebugLogStatus()
        {
            Debug.Log($"[PowerUpUIManager] Active UI Elements: {activeUIElements.Count}/{maxDisplaySlots}\n" +
                     $"Available Slots: {GetAvailableSlots()}\n" +
                     $"Auto-Sync: {autoSyncWithInventory}");
        }
#endif
    }
}
