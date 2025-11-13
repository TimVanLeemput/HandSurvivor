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
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("Strings")]
        [SerializeField] private string activeSkillTitle = "Choose Active Skill";
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

        public void ShowActiveSkillSelection(List<ActiveSkillData> skills, List<GameObject> skillPrefabs, Action<GameObject> onSelected)
        {
            if (skills.Count != skillPrefabs.Count)
            {
                Debug.LogError("[SkillSelectionUI] Skill data and prefab count mismatch!");
                return;
            }

            activeSkillCallback = onSelected;
            currentSkillPrefabs = skillPrefabs;

            ClearOptions();

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

                SetupOptionButton(optionObj, skillData.icon, skillData.displayName, skillData.description, () => OnActiveSkillOptionClicked(index));
            }

            selectionPanel.SetActive(true);
            Debug.Log($"[SkillSelectionUI] Showing {skills.Count} active skill options");
        }

        public void ShowPassiveUpgradeSelection(List<PassiveUpgradeData> upgrades, Action<PassiveUpgradeData> onSelected)
        {
            passiveUpgradeCallback = onSelected;
            currentPassiveData = upgrades;

            ClearOptions();

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

                SetupOptionButton(optionObj, upgradeData.icon, upgradeData.displayName, upgradeData.description, () => OnPassiveUpgradeOptionClicked(index));
            }

            selectionPanel.SetActive(true);
            Debug.Log($"[SkillSelectionUI] Showing {upgrades.Count} passive upgrade options");
        }

        private void SetupOptionButton(GameObject optionObj, Sprite icon, string title, string description, Action onClick)
        {
            Image iconImage = optionObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
            }

            TextMeshProUGUI titleText = optionObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = title;
            }

            TextMeshProUGUI descText = optionObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                descText.text = description;
            }

            Button button = optionObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        private void OnActiveSkillOptionClicked(int index)
        {
            if (index >= 0 && index < currentSkillPrefabs.Count)
            {
                GameObject selectedPrefab = currentSkillPrefabs[index];
                Debug.Log($"[SkillSelectionUI] Active skill selected: {selectedPrefab.name}");

                selectionPanel.SetActive(false);
                activeSkillCallback?.Invoke(selectedPrefab);
            }
        }

        private void OnPassiveUpgradeOptionClicked(int index)
        {
            if (index >= 0 && index < currentPassiveData.Count)
            {
                PassiveUpgradeData selectedUpgrade = currentPassiveData[index];
                Debug.Log($"[SkillSelectionUI] Passive upgrade selected: {selectedUpgrade.displayName}");

                selectionPanel.SetActive(false);
                passiveUpgradeCallback?.Invoke(selectedUpgrade);
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
            currentSkillPrefabs.Clear();
            currentPassiveData.Clear();
        }

        public void Hide()
        {
            selectionPanel.SetActive(false);
            ClearOptions();
        }
    }
}
