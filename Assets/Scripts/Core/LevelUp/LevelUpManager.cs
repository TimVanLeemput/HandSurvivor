using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Core.Passive;

namespace HandSurvivor.Core.LevelUp
{
    public class LevelUpManager : MonoBehaviour
    {
        public static LevelUpManager Instance;

        [Header("References")]
        [SerializeField] private SkillPoolData skillPool;

        [Header("Settings")]
        [SerializeField] private int choiceCount = 3;

        private bool isPaused = false;

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
                Debug.LogError("[LevelUpManager] XPManager.Instance is null after 5 second timeout!",gameObject);
            }

            if (skillPool == null)
            {
                Debug.LogError("[LevelUpManager] SkillPool not assigned!",gameObject);
            }

            if (SkillSelectionUI.Instance == null)
            {
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
            Debug.Log($"[LevelUpManager] Player reached level {newLevel}");

            if (ActiveSkillSlotManager.Instance == null)
            {
                Debug.LogError("[LevelUpManager] ActiveSkillSlotManager.Instance is null!");
                return;
            }

            int filledSlots = ActiveSkillSlotManager.Instance.GetFilledSlotCount();
            bool allSlotsFilled = filledSlots >= ActiveSkillSlotManager.Instance.MaxSlots;

            if (allSlotsFilled)
            {
                ShowPassiveUpgradeSelection();
            }
            else
            {
                ShowActiveSkillSelection();
            }
        }

        private void ShowActiveSkillSelection()
        {
            if (SkillSelectionUI.Instance == null)
            {
                Debug.LogError("[LevelUpManager] SkillSelectionUI.Instance is null!");
                return;
            }

            List<GameObject> randomSkills = skillPool.GetRandomActiveSkills(choiceCount);

            if (randomSkills.Count == 0)
            {
                Debug.LogWarning("[LevelUpManager] No active skills available!");
                return;
            }

            List<ActiveSkillData> skillDataList = new List<ActiveSkillData>();
            foreach (GameObject prefab in randomSkills)
            {
                ActiveSkillBase skillBase = prefab.GetComponent<ActiveSkillBase>();
                if (skillBase != null && skillBase.Data != null)
                {
                    skillDataList.Add(skillBase.Data);
                }
            }

            PauseGame();
            SkillSelectionUI.Instance.ShowActiveSkillSelection(skillDataList, randomSkills, OnActiveSkillSelected);
        }

        private void ShowPassiveUpgradeSelection()
        {
            if (SkillSelectionUI.Instance == null)
            {
                Debug.LogError("[LevelUpManager] SkillSelectionUI.Instance is null!");
                return;
            }

            List<PassiveUpgradeData> randomUpgrades = skillPool.GetRandomPassiveUpgrades(choiceCount);

            if (randomUpgrades.Count == 0)
            {
                Debug.LogWarning("[LevelUpManager] No passive upgrades available!");
                return;
            }

            PauseGame();
            SkillSelectionUI.Instance.ShowPassiveUpgradeSelection(randomUpgrades, OnPassiveUpgradeSelected);
        }

        private void OnActiveSkillSelected(GameObject skillPrefab)
        {
            Debug.Log($"[LevelUpManager] Active skill selected: {skillPrefab.name}");

            GameObject skillObj = Instantiate(skillPrefab);
            ActiveSkillBase skillBase = skillObj.GetComponent<ActiveSkillBase>();

            if (skillBase != null)
            {
                skillBase.Pickup();
                Debug.Log($"[LevelUpManager] Added skill to inventory: {skillBase.Data.displayName}");
            }
            else
            {
                Debug.LogError($"[LevelUpManager] Selected skill prefab has no ActiveSkillBase component!");
                Destroy(skillObj);
            }

            ResumeGame();
        }

        private void OnPassiveUpgradeSelected(PassiveUpgradeData upgrade)
        {
            Debug.Log($"[LevelUpManager] Passive upgrade selected: {upgrade.displayName}");

            if (PassiveUpgradeManager.Instance != null)
            {
                PassiveUpgradeManager.Instance.ApplyUpgrade(upgrade);
            }
            else
            {
                Debug.LogError("[LevelUpManager] PassiveUpgradeManager.Instance is null!");
            }

            ResumeGame();
        }

        private void PauseGame()
        {
            if (!isPaused)
            {
                // Time.timeScale = 0f;
                // isPaused = true;
                // Debug.Log("[LevelUpManager] Game paused");
            }
        }

        private void ResumeGame()
        {
            if (isPaused)
            {
                Time.timeScale = 1f;
                isPaused = false;
                Debug.Log("[LevelUpManager] Game resumed");
            }
        }
    }
}
