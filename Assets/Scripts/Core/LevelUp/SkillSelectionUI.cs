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
        [Header("UI References")] [SerializeField]
        private GameObject selectionPanel;

        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("Strings")] [SerializeField] private string activeSkillTitle = "Choose Active Skill";
        [SerializeField] private string passiveUpgradeTitle = "Choose Passive Upgrade";

        private Action<GameObject> activeSkillCallback;
        private Action<PassiveUpgradeData> passiveUpgradeCallback;
        private List<GameObject> currentOptions = new List<GameObject>();
        private List<GameObject> currentSkillPrefabs = new List<GameObject>();
        private List<PassiveUpgradeData> currentPassiveData = new List<PassiveUpgradeData>();

        private void Awake()
        {
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
            Debug.Log($"[SkillSelectionUI] Showing {upgrades.Count} passive upgrade options");
        }

        private void SetupOptionButton(GameObject optionObj, Sprite skillImage, string title, string description,
            Action onClick)
        {
            Debug.Log($"[SkillSelectionUI] Setting up Button: {title}", optionObj);

            SkillOptionButton skillOptionButton = optionObj.GetComponent<SkillOptionButton>();
            
            if (skillOptionButton == null)
            {
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
                    Debug.Log($"[SkillSelectionUI] Button CLICKED: {title}");
                    if (onClick != null)
                    {
                        Debug.Log($"[SkillSelectionUI] Invoking onClick callback for: {title}");
                        onClick.Invoke();
                    }
                    else
                    {
                        Debug.LogError($"[SkillSelectionUI] onClick callback is NULL for: {title}");
                    }
                });
                Debug.Log($"[SkillSelectionUI] Button listener added successfully for: {title}");
            }
            else
            {
                Debug.LogError($"[SkillSelectionUI] NO BUTTON COMPONENT found on: {title}", optionObj);
            }
        }

        private void OnActiveSkillOptionClicked(int index)
        {
            Debug.Log($"[SkillSelectionUI] OnActiveSkillOptionClicked called with index: {index}");

            if (index >= 0 && index < currentSkillPrefabs.Count)
            {
                GameObject selectedPrefab = currentSkillPrefabs[index];
                Debug.Log($"[SkillSelectionUI] Active skill selected: {selectedPrefab.name}");

                selectionPanel.SetActive(false);

                if (activeSkillCallback != null)
                {
                    Debug.Log($"[SkillSelectionUI] Invoking activeSkillCallback with prefab: {selectedPrefab.name}");
                    activeSkillCallback.Invoke(selectedPrefab);
                }
                else
                {
                    Debug.LogError("[SkillSelectionUI] activeSkillCallback is NULL!");
                }
            }
            else
            {
                Debug.LogError(
                    $"[SkillSelectionUI] Invalid index {index} or prefabs count {currentSkillPrefabs.Count}");
            }
        }

        private void OnPassiveUpgradeOptionClicked(int index)
        {
            Debug.Log($"[SkillSelectionUI] OnPassiveUpgradeOptionClicked called with index: {index}");

            if (index >= 0 && index < currentPassiveData.Count)
            {
                PassiveUpgradeData selectedUpgrade = currentPassiveData[index];
                Debug.Log($"[SkillSelectionUI] Passive upgrade selected: {selectedUpgrade.displayName}");

                selectionPanel.SetActive(false);

                if (passiveUpgradeCallback != null)
                {
                    Debug.Log(
                        $"[SkillSelectionUI] Invoking passiveUpgradeCallback with upgrade: {selectedUpgrade.displayName}");
                    passiveUpgradeCallback.Invoke(selectedUpgrade);
                }
                else
                {
                    Debug.LogError("[SkillSelectionUI] passiveUpgradeCallback is NULL!");
                }
            }
            else
            {
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