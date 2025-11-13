using System.Collections.Generic;
using UnityEngine;

namespace HandSurvivor.ActiveSkills.UI
{
    /// <summary>
    /// Manages multiple ActiveSkillUI elements and syncs with ActiveSkillInventory
    /// Automatically creates/destroys UI elements as active skills are added/removed
    /// </summary>
    public class ActiveSkillUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform uiContainer;
        [SerializeField] private GameObject ActiveSkillUIPrefab;

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

        private List<ActiveSkillUI> activeUIElements = new List<ActiveSkillUI>();
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
                Debug.LogWarning("[ActiveSkillUIManager] No UI container assigned, using self as container", this);
                uiContainer = transform;
            }

            if (ActiveSkillUIPrefab == null)
            {
                Debug.LogError("[ActiveSkillUIManager] No ActiveSkillUI prefab assigned!", this);
                return;
            }

            // Subscribe to inventory if auto-sync enabled
            if (autoSyncWithInventory && ActiveSkillInventory.Instance != null)
            {
                ActiveSkillInventory.Instance.OnActiveSkillAdded.AddListener(OnInventoryactiveSkillAdded);
                ActiveSkillInventory.Instance.OnActiveSkillRemoved.AddListener(OnInventoryactiveSkillRemoved);
                ActiveSkillInventory.Instance.OnActiveSkillTypeRemoved.AddListener(OnInventoryactiveSkillTypeRemoved);

                // Sync with existing active skills in inventory
                SyncWithInventory();
            }

            isInitialized = true;
        }

        private void OnDestroy()
        {
            // Unsubscribe from inventory
            if (ActiveSkillInventory.Instance != null)
            {
                ActiveSkillInventory.Instance.OnActiveSkillAdded.RemoveListener(OnInventoryactiveSkillAdded);
                ActiveSkillInventory.Instance.OnActiveSkillRemoved.RemoveListener(OnInventoryactiveSkillRemoved);
                ActiveSkillInventory.Instance.OnActiveSkillTypeRemoved.RemoveListener(OnInventoryactiveSkillTypeRemoved);
            }
        }

        /// <summary>
        /// Sync UI with current ActiveSkillInventory contents
        /// </summary>
        private void SyncWithInventory()
        {
            if (ActiveSkillInventory.Instance == null)
            {
                return;
            }

            // Clear existing UI
            ClearAll();

            // Add UI for each active skill stack in inventory
            foreach (ActiveSkillStack stack in ActiveSkillInventory.Instance.ActiveSkillStacks)
            {
                AddActiveSkillUI(stack.activeSkillInstance);
            }

            Debug.Log($"[ActiveSkillUIManager] Synced with inventory: {activeUIElements.Count} active skills");
        }

        /// <summary>
        /// Add a UI element for a active skill
        /// </summary>
        public ActiveSkillUI AddActiveSkillUI(ActiveSkillBase activeSkill)
        {
            if (activeSkill == null)
            {
                Debug.LogWarning("[ActiveSkillUIManager] Cannot add UI for null active skill");
                return null;
            }

            // Check if already displaying this active skill
            if (GetUIForactiveSkill(activeSkill) != null)
            {
                Debug.LogWarning($"[ActiveSkillUIManager] Already displaying UI for {activeSkill.Data.displayName}");
                return null;
            }

            // Check max slots
            if (activeUIElements.Count >= maxDisplaySlots)
            {
                Debug.LogWarning($"[ActiveSkillUIManager] Max display slots ({maxDisplaySlots}) reached");
                return null;
            }

            // Instantiate UI element
            GameObject uiObj = Instantiate(ActiveSkillUIPrefab, uiContainer);
            ActiveSkillUI ActiveSkillUI = uiObj.GetComponent<ActiveSkillUI>();

            if (ActiveSkillUI == null)
            {
                Debug.LogError("[ActiveSkillUIManager] ActiveSkillUI prefab doesn't have ActiveSkillUI component!");
                Destroy(uiObj);
                return null;
            }

            // Setup and add to list
            ActiveSkillUI.SetActiveSkill(activeSkill);
            ActiveSkillUI.SetVisible(true);
            activeUIElements.Add(ActiveSkillUI);

            Debug.Log($"[ActiveSkillUIManager] Added UI for {activeSkill.Data.displayName} ({activeUIElements.Count}/{maxDisplaySlots})");

            return ActiveSkillUI;
        }

        /// <summary>
        /// Remove UI element for a active skill
        /// </summary>
        public void RemoveActiveSkillUI(ActiveSkillBase activeSkill)
        {
            ActiveSkillUI uiToRemove = GetUIForactiveSkill(activeSkill);

            if (uiToRemove == null)
            {
                return;
            }

            activeUIElements.Remove(uiToRemove);
            Destroy(uiToRemove.gameObject);

            Debug.Log($"[ActiveSkillUIManager] Removed UI for {activeSkill.Data.displayName} ({activeUIElements.Count}/{maxDisplaySlots})");
        }

        /// <summary>
        /// Get the UI element displaying a specific active skill
        /// </summary>
        private ActiveSkillUI GetUIForactiveSkill(ActiveSkillBase activeSkill)
        {
            if (activeSkill == null)
            {
                return null;
            }

            foreach (ActiveSkillUI ui in activeUIElements)
            {
                if (ui.GetActiveSkill() == activeSkill)
                {
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
            foreach (ActiveSkillUI ui in activeUIElements)
            {
                if (ui != null)
                {
                    Destroy(ui.gameObject);
                }
            }

            activeUIElements.Clear();
            Debug.Log("[ActiveSkillUIManager] Cleared all active skill UI elements");
        }

        /// <summary>
        /// Called when a active skill is added to inventory (or stack count increases)
        /// </summary>
        private void OnInventoryactiveSkillAdded(ActiveSkillBase activeSkill, int newCount)
        {
            // Check if UI already exists for this active skill
            ActiveSkillUI existingUI = GetUIForactiveSkill(activeSkill);
            if (existingUI == null)
            {
                // First time adding this type, create new UI
                AddActiveSkillUI(activeSkill);
            }
            else
            {
                // Already exists, just update it (UI will refresh on its own via Update)
                Debug.Log($"[ActiveSkillUIManager] active skill stack updated: {activeSkill.Data.displayName} x{newCount}");
            }
        }

        /// <summary>
        /// Called when a active skill stack count decreases
        /// </summary>
        private void OnInventoryactiveSkillRemoved(ActiveSkillBase activeSkill, int newCount)
        {
            // Stack count decreased but still exists, UI will update on its own
            Debug.Log($"[ActiveSkillUIManager] active skill stack decreased: {activeSkill.Data.displayName} x{newCount}");
        }

        /// <summary>
        /// Called when a active skill type is completely removed from inventory (stack reached 0)
        /// </summary>
        private void OnInventoryactiveSkillTypeRemoved(ActiveSkillBase activeSkill)
        {
            RemoveActiveSkillUI(activeSkill);
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
            if (ActiveSkillInventory.Instance != null)
            {
                SyncWithInventory();
            }
            else
            {
                Debug.LogWarning("[ActiveSkillUIManager] No ActiveSkillInventory instance found");
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
            Debug.Log($"[ActiveSkillUIManager] Active UI Elements: {activeUIElements.Count}/{maxDisplaySlots}\n" +
                     $"Available Slots: {GetAvailableSlots()}\n" +
                     $"Auto-Sync: {autoSyncWithInventory}");
        }
#endif
    }
}
