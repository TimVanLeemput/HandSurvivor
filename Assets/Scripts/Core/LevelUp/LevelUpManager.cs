using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Core.Passive;

namespace HandSurvivor.Core.LevelUp
{
    public class LevelUpManager : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         public static LevelUpManager Instance;

        [Header("References")]
        [SerializeField] private SkillPoolData skillPool;

        [Header("Settings")]
        [SerializeField] private int choiceCount = 3;

        private int pendingLevels = 0;
        private bool isShowingSelection = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartCoroutine(InitializeWithRetry());
        }

        private IEnumerator InitializeWithRetry()
        {
            float timeout = 5f;
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                if (XPManager.Instance != null && SkillSelectionUI.Instance != null)
                {
                    break;
                }
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (XPManager.Instance != null)
            {
                XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogError("[LevelUpManager] XPManager.Instance is null after 5 second timeout!",gameObject);
            }

            if (skillPool == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogError("[LevelUpManager] SkillPool not assigned!",gameObject);
            }

            if (SkillSelectionUI.Instance == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogError("[LevelUpManager] SkillSelectionUI.Instance is null after 5 second timeout!",gameObject);
            }
        }

        private void OnDestroy()
        {
            if (XPManager.Instance != null)
            {
                XPManager.Instance.OnLevelUp.RemoveListener(OnPlayerLevelUp);
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[LevelUpManager] Player reached level {newLevel}");

            pendingLevels++;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[LevelUpManager] Pending levels: {pendingLevels}");

            // Only show selection if not already showing
            if (!isShowingSelection)
            {
                ShowNextSelection();
            }
        }

        private void ShowNextSelection()
        {
            if (SkillSelectionUI.Instance == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError("[LevelUpManager] SkillSelectionUI.Instance is null!");
                return;
            }

            isShowingSelection = true;

            // Get owned skill IDs for filtering
            List<string> ownedSkillIds = null;
            if (ActiveSkillInventory.Instance != null)
            {
                ownedSkillIds = ActiveSkillInventory.Instance.GetAllSkillIds();
            }

            // Get mixed selection of skills and upgrades
            skillPool.GetRandomMixedSelection(choiceCount, ownedSkillIds,
                out List<GameObject> selectedSkills, out List<PassiveUpgradeData> selectedUpgrades);

            // Check if we have any options at all
            if (selectedSkills.Count == 0 && selectedUpgrades.Count == 0)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[LevelUpManager] No skills or upgrades available! Consuming level without selection.");
                ProcessLevelSelection();
                return;
            }

            // Extract skill data from prefabs
            List<ActiveSkillData> skillDataList = new List<ActiveSkillData>();
            foreach (GameObject prefab in selectedSkills)
            {
                ActiveSkillBase skillBase = prefab.GetComponent<ActiveSkillBase>();
                if (skillBase != null && skillBase.Data != null)
                {
                    skillDataList.Add(skillBase.Data);
                }
            }

            // Show mixed selection UI
            SkillSelectionUI.Instance.ShowMixedSelection(skillDataList, selectedSkills, selectedUpgrades,
                OnActiveSkillSelected, OnPassiveUpgradeSelected);
        }

        private void OnActiveSkillSelected(GameObject skillPrefab)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[LevelUpManager] Active skill selected: {skillPrefab.name}");

            GameObject skillObj = Instantiate(skillPrefab);
            ActiveSkillBase skillBase = skillObj.GetComponent<ActiveSkillBase>();

            if (skillBase != null)
            {
                skillBase.Pickup();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[LevelUpManager] Added skill to inventory: {skillBase.Data.displayName}");
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError($"[LevelUpManager] Selected skill prefab has no ActiveSkillBase component!");
                Destroy(skillObj);
            }

            ProcessLevelSelection();
        }

        private void OnPassiveUpgradeSelected(PassiveUpgradeData upgrade)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[LevelUpManager] Passive upgrade selected: {upgrade.displayName}");

            if (PassiveUpgradeManager.Instance != null)
            {
                PassiveUpgradeManager.Instance.ApplyUpgrade(upgrade);
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError("[LevelUpManager] PassiveUpgradeManager.Instance is null!");
            }

            ProcessLevelSelection();
        }

        private void ProcessLevelSelection()
        {
            pendingLevels--;

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[LevelUpManager] Level spent. Remaining pending levels: {pendingLevels}");

            if (pendingLevels > 0)
            {
                // More levels pending - show next selection
                ShowNextSelection();
            }
            else
            {
                // All levels spent - hide UI
                isShowingSelection = false;
                if (SkillSelectionUI.Instance != null)
                {
                    SkillSelectionUI.Instance.Hide();
                }
            }
        }
    }
}
