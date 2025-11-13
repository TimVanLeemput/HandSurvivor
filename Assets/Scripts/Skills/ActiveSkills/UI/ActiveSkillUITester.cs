using MyBox;
using UnityEngine;

namespace HandSurvivor.ActiveSkills.UI
{
    /// <summary>
    /// Testing utility for ActiveSkillUI system
    /// Provides buttons to add/remove/activate active skills for UI testing
    /// </summary>
    public class ActiveSkillUITester : MonoBehaviour
    {
        [Header("Test active skills")]
        [SerializeField] private GameObject laseractiveSkillPrefab;
        [SerializeField] private GameObject specialBeamCannonPrefab;

        [Header("References")]
        [SerializeField] private ActiveSkillUIManager uiManager;

        private ActiveSkillBase testactiveSkill;

        [ButtonMethod]
        private void AddLaseractiveSkill()
        {
            if (laseractiveSkillPrefab == null)
            {
                Debug.LogWarning("[ActiveSkillUITester] LaseractiveSkill prefab not assigned!");
                return;
            }

            GameObject activeSkillObj = Instantiate(laseractiveSkillPrefab);
            ActiveSkillBase activeSkill = activeSkillObj.GetComponent<ActiveSkillBase>();

            if (activeSkill != null)
            {
                activeSkill.Pickup();
                Debug.Log("[ActiveSkillUITester] Added LaseractiveSkill to inventory");
            }
        }

        [ButtonMethod]
        private void AddSpecialBeamCannon()
        {
            if (specialBeamCannonPrefab == null)
            {
                Debug.LogWarning("[ActiveSkillUITester] SpecialBeamCannon prefab not assigned!");
                return;
            }

            GameObject activeSkillObj = Instantiate(specialBeamCannonPrefab);
            ActiveSkillBase activeSkill = activeSkillObj.GetComponent<ActiveSkillBase>();

            if (activeSkill != null)
            {
                activeSkill.Pickup();
                Debug.Log("[ActiveSkillUITester] Added SpecialBeamCannon to inventory");
            }
        }

        [ButtonMethod]
        private void ActivateFirstactiveSkill()
        {
            if (ActiveSkillInventory.Instance == null || ActiveSkillInventory.Instance.ActiveSkillStacks.Count == 0)
            {
                Debug.LogWarning("[ActiveSkillUITester] No active skills in inventory!");
                return;
            }

            ActiveSkillBase activeSkill = ActiveSkillInventory.Instance.ActiveSkillStacks[0].activeSkillInstance;
            activeSkill.TryActivate();
            Debug.Log($"[ActiveSkillUITester] Activated {activeSkill.Data.displayName}");
        }

        [ButtonMethod]
        private void RemoveFirstactiveSkill()
        {
            if (ActiveSkillInventory.Instance == null || ActiveSkillInventory.Instance.ActiveSkillStacks.Count == 0)
            {
                Debug.LogWarning("[ActiveSkillUITester] No active skills in inventory!");
                return;
            }

            ActiveSkillBase activeSkill = ActiveSkillInventory.Instance.ActiveSkillStacks[0].activeSkillInstance;
            string name = activeSkill.Data.displayName;
            ActiveSkillInventory.Instance.RemoveActiveSkill(activeSkill);
            Destroy(activeSkill.gameObject);
            Debug.Log($"[ActiveSkillUITester] Removed {name}");
        }

        [ButtonMethod]
        private void ClearAllactiveSkills()
        {
            if (ActiveSkillInventory.Instance != null)
            {
                ActiveSkillInventory.Instance.ClearInventory();
                Debug.Log("[ActiveSkillUITester] Cleared all active skills");
            }
        }

        [ButtonMethod]
        private void SyncUIWithInventory()
        {
            if (uiManager != null)
            {
                // This will be called via context menu on the manager
                Debug.Log("[ActiveSkillUITester] Use ActiveSkillUIManager context menu: 'Debug: Sync With Inventory'");
            }
            else
            {
                Debug.LogWarning("[ActiveSkillUITester] UIManager not assigned!");
            }
        }

        [ButtonMethod]
        private void LogInventoryStatus()
        {
            if (ActiveSkillInventory.Instance == null)
            {
                Debug.LogWarning("[ActiveSkillUITester] No ActiveSkillInventory in scene!");
                return;
            }

            Debug.Log($"[ActiveSkillUITester] Inventory Status:\n" +
                     $"  Active: {ActiveSkillInventory.Instance.GetActiveActiveSkills().Count}\n" +
                     $"  On Cooldown: {ActiveSkillInventory.Instance.GetCooldownActiveSkills().Count}");
        }

        [ButtonMethod]
        private void TestCooldownScenario()
        {
            Debug.Log("[ActiveSkillUITester] Testing cooldown scenario...");

            // Add active skill
            if (laseractiveSkillPrefab != null)
            {
                GameObject activeSkillObj = Instantiate(laseractiveSkillPrefab);
                ActiveSkillBase activeSkill = activeSkillObj.GetComponent<ActiveSkillBase>();

                if (activeSkill != null)
                {
                    activeSkill.Pickup();
                    Debug.Log("  1. Added active skill to inventory");

                    // Wait a frame then activate
                    StartCoroutine(TestActivateAfterDelay(activeSkill, 0.5f));
                }
            }
        }

        private System.Collections.IEnumerator TestActivateAfterDelay(ActiveSkillBase activeSkill, float delay)
        {
            yield return new WaitForSeconds(delay);

            Debug.Log("  2. Activating active skill...");
            activeSkill.TryActivate();

            Debug.Log("  3. active skill active - UI should show duration countdown");
            Debug.Log($"     Duration: {activeSkill.RemainingDuration:F1}s");
        }

#if UNITY_EDITOR
        [ContextMenu("Quick Setup Test Scene")]
        private void QuickSetupTestScene()
        {
            Debug.Log("[ActiveSkillUITester] Quick Setup Guide:\n" +
                     "1. Assign LaseractiveSkill prefab\n" +
                     "2. Assign SpecialBeamCannon prefab (optional)\n" +
                     "3. Assign ActiveSkillUIManager reference\n" +
                     "4. Click 'Add Laser Power Up' Button\n" +
                     "5. Click 'Activate First Power Up' Button\n" +
                     "6. Watch UI show duration countdown!");
        }
#endif
    }
}
