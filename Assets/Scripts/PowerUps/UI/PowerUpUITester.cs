using MyBox;
using UnityEngine;

namespace HandSurvivor.PowerUps.UI
{
    /// <summary>
    /// Testing utility for PowerUpUI system
    /// Provides buttons to add/remove/activate power-ups for UI testing
    /// </summary>
    public class PowerUpUITester : MonoBehaviour
    {
        [Header("Test Power-Ups")]
        [SerializeField] private GameObject laserPowerUpPrefab;
        [SerializeField] private GameObject specialBeamCannonPrefab;

        [Header("References")]
        [SerializeField] private PowerUpUIManager uiManager;

        private PowerUpBase testPowerUp;

        [ButtonMethod]
        private void AddLaserPowerUp()
        {
            if (laserPowerUpPrefab == null)
            {
                Debug.LogWarning("[PowerUpUITester] LaserPowerUp prefab not assigned!");
                return;
            }

            GameObject powerUpObj = Instantiate(laserPowerUpPrefab);
            PowerUpBase powerUp = powerUpObj.GetComponent<PowerUpBase>();

            if (powerUp != null)
            {
                powerUp.Pickup();
                Debug.Log("[PowerUpUITester] Added LaserPowerUp to inventory");
            }
        }

        [ButtonMethod]
        private void AddSpecialBeamCannon()
        {
            if (specialBeamCannonPrefab == null)
            {
                Debug.LogWarning("[PowerUpUITester] SpecialBeamCannon prefab not assigned!");
                return;
            }

            GameObject powerUpObj = Instantiate(specialBeamCannonPrefab);
            PowerUpBase powerUp = powerUpObj.GetComponent<PowerUpBase>();

            if (powerUp != null)
            {
                powerUp.Pickup();
                Debug.Log("[PowerUpUITester] Added SpecialBeamCannon to inventory");
            }
        }

        [ButtonMethod]
        private void ActivateFirstPowerUp()
        {
            if (PowerUpInventory.Instance == null || PowerUpInventory.Instance.PowerUpStacks.Count == 0)
            {
                Debug.LogWarning("[PowerUpUITester] No power-ups in inventory!");
                return;
            }

            PowerUpBase powerUp = PowerUpInventory.Instance.PowerUpStacks[0].powerUpInstance;
            powerUp.TryActivate();
            Debug.Log($"[PowerUpUITester] Activated {powerUp.Data.displayName}");
        }

        [ButtonMethod]
        private void RemoveFirstPowerUp()
        {
            if (PowerUpInventory.Instance == null || PowerUpInventory.Instance.PowerUpStacks.Count == 0)
            {
                Debug.LogWarning("[PowerUpUITester] No power-ups in inventory!");
                return;
            }

            PowerUpBase powerUp = PowerUpInventory.Instance.PowerUpStacks[0].powerUpInstance;
            string name = powerUp.Data.displayName;
            PowerUpInventory.Instance.RemovePowerUp(powerUp);
            Destroy(powerUp.gameObject);
            Debug.Log($"[PowerUpUITester] Removed {name}");
        }

        [ButtonMethod]
        private void ClearAllPowerUps()
        {
            if (PowerUpInventory.Instance != null)
            {
                PowerUpInventory.Instance.ClearInventory();
                Debug.Log("[PowerUpUITester] Cleared all power-ups");
            }
        }

        [ButtonMethod]
        private void SyncUIWithInventory()
        {
            if (uiManager != null)
            {
                // This will be called via context menu on the manager
                Debug.Log("[PowerUpUITester] Use PowerUpUIManager context menu: 'Debug: Sync With Inventory'");
            }
            else
            {
                Debug.LogWarning("[PowerUpUITester] UIManager not assigned!");
            }
        }

        [ButtonMethod]
        private void LogInventoryStatus()
        {
            if (PowerUpInventory.Instance == null)
            {
                Debug.LogWarning("[PowerUpUITester] No PowerUpInventory in scene!");
                return;
            }

            Debug.Log($"[PowerUpUITester] Inventory Status:\n" +
                     $"  Active: {PowerUpInventory.Instance.GetActivePowerUps().Count}\n" +
                     $"  On Cooldown: {PowerUpInventory.Instance.GetCooldownPowerUps().Count}");
        }

        [ButtonMethod]
        private void TestCooldownScenario()
        {
            Debug.Log("[PowerUpUITester] Testing cooldown scenario...");

            // Add power-up
            if (laserPowerUpPrefab != null)
            {
                GameObject powerUpObj = Instantiate(laserPowerUpPrefab);
                PowerUpBase powerUp = powerUpObj.GetComponent<PowerUpBase>();

                if (powerUp != null)
                {
                    powerUp.Pickup();
                    Debug.Log("  1. Added power-up to inventory");

                    // Wait a frame then activate
                    StartCoroutine(TestActivateAfterDelay(powerUp, 0.5f));
                }
            }
        }

        private System.Collections.IEnumerator TestActivateAfterDelay(PowerUpBase powerUp, float delay)
        {
            yield return new WaitForSeconds(delay);

            Debug.Log("  2. Activating power-up...");
            powerUp.TryActivate();

            Debug.Log("  3. Power-up active - UI should show duration countdown");
            Debug.Log($"     Duration: {powerUp.RemainingDuration:F1}s");
        }

#if UNITY_EDITOR
        [ContextMenu("Quick Setup Test Scene")]
        private void QuickSetupTestScene()
        {
            Debug.Log("[PowerUpUITester] Quick Setup Guide:\n" +
                     "1. Assign LaserPowerUp prefab\n" +
                     "2. Assign SpecialBeamCannon prefab (optional)\n" +
                     "3. Assign PowerUpUIManager reference\n" +
                     "4. Click 'Add Laser Power Up' button\n" +
                     "5. Click 'Activate First Power Up' button\n" +
                     "6. Watch UI show duration countdown!");
        }
#endif
    }
}
