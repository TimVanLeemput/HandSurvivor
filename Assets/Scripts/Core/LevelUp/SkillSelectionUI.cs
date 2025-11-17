using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HandSurvivor.ActiveSkills;
using HandSurvivor.Core.Passive;

namespace HandSurvivor.Core.LevelUp
{
    public class SkillSelectionUI : MonoBehaviour
    {
        public static SkillSelectionUI Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("UI References")] [SerializeField]
        private GameObject selectionPanel;

        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("Strings")] [SerializeField] private string activeSkillTitle = "Choose Active Skill";
        [SerializeField] private string passiveUpgradeTitle = "Choose Passive Upgrade";
        [SerializeField] private string mixedSelectionTitle = "Choose Upgrade";

        private Action<GameObject> activeSkillCallback;
        private Action<PassiveUpgradeData> passiveUpgradeCallback;
        private List<GameObject> currentOptions = new List<GameObject>();
        private List<GameObject> currentSkillPrefabs = new List<GameObject>();
        private List<PassiveUpgradeData> currentPassiveData = new List<PassiveUpgradeData>();

        // For mixed selection - track which index maps to which type
        private enum OptionType { ActiveSkill, PassiveUpgrade }
        private List<OptionType> optionTypes = new List<OptionType>();
        private List<int> optionIndices = new List<int>(); // Index into skill or upgrade list

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
                return;
            }

            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
        }

        public void ShowActiveSkillSelection(List<ActiveSkillData> skills, List<GameObject> skillPrefabs,
            Action<GameObject> onSelected)
        {
            if (skills.Count != skillPrefabs.Count)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError("[SkillSelectionUI] Skill data and prefab count mismatch!");
                return;
            }

            ClearOptions();

            activeSkillCallback = onSelected;
            currentSkillPrefabs = skillPrefabs;
            currentPassiveData.Clear();

            if (titleText != null)
            {
                titleText.text = activeSkillTitle;
            }

            for (int i = 0; i < skills.Count; i++)
            {
                ActiveSkillData skillData = skills[i];
                int index = i;

                GameObject optionObj = Instantiate(optionButtonPrefab, optionsContainer);
                currentOptions.Add(optionObj);

                SetupOptionButton(optionObj, skillData.skillImage, skillData.displayName, skillData.description,
                    () => OnActiveSkillOptionClicked(index));
            }

            selectionPanel.SetActive(true);
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[SkillSelectionUI] Showing {skills.Count} active skill options");
        }

        public void ShowPassiveUpgradeSelection(List<PassiveUpgradeData> upgrades,
            Action<PassiveUpgradeData> onSelected)
        {
            ClearOptions();

            passiveUpgradeCallback = onSelected;
            currentPassiveData = upgrades;
            currentSkillPrefabs.Clear();

            if (titleText != null)
            {
                titleText.text = passiveUpgradeTitle;
            }

            for (int i = 0; i < upgrades.Count; i++)
            {
                PassiveUpgradeData upgradeData = upgrades[i];
                int index = i;

                GameObject optionObj = Instantiate(optionButtonPrefab, optionsContainer);
                currentOptions.Add(optionObj);

                SetupOptionButton(optionObj, upgradeData.passiveImage, upgradeData.displayName, upgradeData.description,
                    () => OnPassiveUpgradeOptionClicked(index));
            }

            selectionPanel.SetActive(true);
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[SkillSelectionUI] Showing {upgrades.Count} passive upgrade options");
        }

        public void ShowMixedSelection(List<ActiveSkillData> skills, List<GameObject> skillPrefabs,
            List<PassiveUpgradeData> upgrades, Action<GameObject> onSkillSelected,
            Action<PassiveUpgradeData> onUpgradeSelected)
        {
            ClearOptions();

            activeSkillCallback = onSkillSelected;
            passiveUpgradeCallback = onUpgradeSelected;
            currentSkillPrefabs = skillPrefabs;
            currentPassiveData = upgrades;
            optionTypes.Clear();
            optionIndices.Clear();

            if (titleText != null)
            {
                titleText.text = mixedSelectionTitle;
            }

            // Add all skills
            for (int i = 0; i < skills.Count; i++)
            {
                ActiveSkillData skillData = skills[i];
                int displayIndex = optionTypes.Count; // Current position in combined list

                GameObject optionObj = Instantiate(optionButtonPrefab, optionsContainer);
                currentOptions.Add(optionObj);
                optionTypes.Add(OptionType.ActiveSkill);
                optionIndices.Add(i); // Index into skillPrefabs list

                SetupOptionButton(optionObj, skillData.skillImage, skillData.displayName, skillData.description,
                    () => OnMixedOptionClicked(displayIndex));
            }

            // Add all upgrades
            for (int i = 0; i < upgrades.Count; i++)
            {
                PassiveUpgradeData upgradeData = upgrades[i];
                int displayIndex = optionTypes.Count; // Current position in combined list

                GameObject optionObj = Instantiate(optionButtonPrefab, optionsContainer);
                currentOptions.Add(optionObj);
                optionTypes.Add(OptionType.PassiveUpgrade);
                optionIndices.Add(i); // Index into upgrades list

                SetupOptionButton(optionObj, upgradeData.passiveImage, upgradeData.displayName, upgradeData.description,
                    () => OnMixedOptionClicked(displayIndex));
            }

            selectionPanel.SetActive(true);
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[SkillSelectionUI] Showing mixed selection: {skills.Count} skills + {upgrades.Count} upgrades = {optionTypes.Count} total");
        }

        private void SetupOptionButton(GameObject optionObj, Sprite skillImage, string title, string description,
            Action onClick)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[SkillSelectionUI] Setting up Button: {title}", optionObj);

            SkillOptionButton skillOptionButton = optionObj.GetComponent<SkillOptionButton>();

            if (skillOptionButton == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError($"[SkillSelectionUI] SkillOptionButton component not found on: {title}", optionObj);
                return;
            }
            
            if (skillImage != null)
            {
                skillOptionButton.SkillImage = skillImage;
            }

            if (skillOptionButton.TitleText != null)
            {
                skillOptionButton.TitleText.text = title;
            }

            if (skillOptionButton.DescriptionText != null)
            {
                skillOptionButton.DescriptionText.text = description;
            }

            
            if (skillOptionButton.Button != null)
            {
                skillOptionButton.Button.onClick.RemoveAllListeners();
                skillOptionButton.Button.interactable = true;
                skillOptionButton.Button.onClick.AddListener(() =>
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log($"[SkillSelectionUI] Button CLICKED: {title}");
                    if (onClick != null)
                    {
                        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                            Debug.Log($"[SkillSelectionUI] Invoking onClick callback for: {title}");
                        onClick.Invoke();
                    }
                    else
                    {
                        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                            Debug.LogError($"[SkillSelectionUI] onClick callback is NULL for: {title}");
                    }
                });
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[SkillSelectionUI] Button listener added successfully for: {title}");
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError($"[SkillSelectionUI] NO BUTTON COMPONENT found on: {title}", optionObj);
            }
        }

        private void OnMixedOptionClicked(int displayIndex)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[SkillSelectionUI] OnMixedOptionClicked called with displayIndex: {displayIndex}");

            if (displayIndex < 0 || displayIndex >= optionTypes.Count)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError($"[SkillSelectionUI] Invalid displayIndex: {displayIndex}");
                return;
            }

            OptionType type = optionTypes[displayIndex];
            int dataIndex = optionIndices[displayIndex];

            selectionPanel.SetActive(false);

            if (type == OptionType.ActiveSkill)
            {
                if (dataIndex >= 0 && dataIndex < currentSkillPrefabs.Count)
                {
                    GameObject selectedPrefab = currentSkillPrefabs[dataIndex];
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log($"[SkillSelectionUI] Mixed: Active skill selected: {selectedPrefab.name}");

                    activeSkillCallback?.Invoke(selectedPrefab);
                }
            }
            else if (type == OptionType.PassiveUpgrade)
            {
                if (dataIndex >= 0 && dataIndex < currentPassiveData.Count)
                {
                    PassiveUpgradeData selectedUpgrade = currentPassiveData[dataIndex];
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log($"[SkillSelectionUI] Mixed: Passive upgrade selected: {selectedUpgrade.displayName}");

                    passiveUpgradeCallback?.Invoke(selectedUpgrade);
                }
            }
        }

        private void OnActiveSkillOptionClicked(int index)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[SkillSelectionUI] OnActiveSkillOptionClicked called with index: {index}");

            if (index >= 0 && index < currentSkillPrefabs.Count)
            {
                GameObject selectedPrefab = currentSkillPrefabs[index];
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[SkillSelectionUI] Active skill selected: {selectedPrefab.name}");

                selectionPanel.SetActive(false);

                if (activeSkillCallback != null)
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log($"[SkillSelectionUI] Invoking activeSkillCallback with prefab: {selectedPrefab.name}");
                    activeSkillCallback.Invoke(selectedPrefab);
                }
                else
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.LogError("[SkillSelectionUI] activeSkillCallback is NULL!");
                }
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError(
                        $"[SkillSelectionUI] Invalid index {index} or prefabs count {currentSkillPrefabs.Count}");
            }
        }

        private void OnPassiveUpgradeOptionClicked(int index)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[SkillSelectionUI] OnPassiveUpgradeOptionClicked called with index: {index}");

            if (index >= 0 && index < currentPassiveData.Count)
            {
                PassiveUpgradeData selectedUpgrade = currentPassiveData[index];
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log($"[SkillSelectionUI] Passive upgrade selected: {selectedUpgrade.displayName}");

                selectionPanel.SetActive(false);

                if (passiveUpgradeCallback != null)
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.Log(
                            $"[SkillSelectionUI] Invoking passiveUpgradeCallback with upgrade: {selectedUpgrade.displayName}");
                    passiveUpgradeCallback.Invoke(selectedUpgrade);
                }
                else
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                        Debug.LogError("[SkillSelectionUI] passiveUpgradeCallback is NULL!");
                }
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogError(
                        $"[SkillSelectionUI] Invalid index {index} or upgrades count {currentPassiveData.Count}");
            }
        }

        private void ClearOptions()
        {
            foreach (GameObject option in currentOptions)
            {
                if (option != null)
                {
                    Destroy(option);
                }
            }

            currentOptions.Clear();
        }

        public void Hide()
        {
            selectionPanel.SetActive(false);
            ClearOptions();
            currentSkillPrefabs.Clear();
            currentPassiveData.Clear();
        }
    }
}