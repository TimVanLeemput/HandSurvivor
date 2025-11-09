using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HandSurvivor.UI
{
    /// <summary>
    /// UI component for hand selection interface.
    /// Allows player to choose their dominant hand (main hand for physical damage).
    /// Can be used in traditional UI or VR canvases.
    /// </summary>
    public class HandSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        [Tooltip("Button to select Left hand as main hand")]
        private Button leftHandButton;

        [SerializeField]
        [Tooltip("Button to select Right hand as main hand")]
        private Button rightHandButton;

        [SerializeField]
        [Tooltip("Optional text to display current selection")]
        private TextMeshProUGUI selectionText;

        [SerializeField]
        [Tooltip("Optional text for left button label")]
        private TextMeshProUGUI leftHandLabel;

        [SerializeField]
        [Tooltip("Optional text for right button label")]
        private TextMeshProUGUI rightHandLabel;

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("Color for selected button")]
        private Color selectedColor = new Color(0.2f, 0.8f, 0.2f, 1f);

        [SerializeField]
        [Tooltip("Color for unselected button")]
        private Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [SerializeField]
        [Tooltip("Show hand role descriptions")]
        private bool showRoleDescriptions = true;

        [Header("Auto-Close")]
        [SerializeField]
        [Tooltip("Automatically close UI after selection")]
        private bool autoCloseAfterSelection = false;

        [SerializeField]
        [Tooltip("Delay before auto-closing (seconds)")]
        private float autoCloseDelay = 1.0f;

        private void Start()
        {
            // Setup button listeners
            if (leftHandButton != null)
            {
                leftHandButton.onClick.AddListener(OnLeftHandSelected);
            }

            if (rightHandButton != null)
            {
                rightHandButton.onClick.AddListener(OnRightHandSelected);
            }

            // Subscribe to hand selection changes
            if (HandSelectionManager.Instance != null)
            {
                HandSelectionManager.Instance.OnMainHandChanged.AddListener(OnHandPreferenceChanged);
                HandSelectionManager.Instance.OnPreferenceLoaded.AddListener(OnHandPreferenceChanged);
            }

            // Update UI to reflect current selection
            UpdateUI();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (leftHandButton != null)
            {
                leftHandButton.onClick.RemoveListener(OnLeftHandSelected);
            }

            if (rightHandButton != null)
            {
                rightHandButton.onClick.RemoveListener(OnRightHandSelected);
            }

            if (HandSelectionManager.Instance != null)
            {
                HandSelectionManager.Instance.OnMainHandChanged.RemoveListener(OnHandPreferenceChanged);
                HandSelectionManager.Instance.OnPreferenceLoaded.RemoveListener(OnHandPreferenceChanged);
            }
        }

        /// <summary>
        /// Called when left hand button is clicked
        /// </summary>
        private void OnLeftHandSelected()
        {
            HandSelectionManager.Instance.SetMainHand(HandType.Left);
            Debug.Log("[HandSelectionUI] Player selected LEFT hand as main hand");

            if (autoCloseAfterSelection)
            {
                Invoke(nameof(CloseUI), autoCloseDelay);
            }
        }

        /// <summary>
        /// Called when right hand button is clicked
        /// </summary>
        private void OnRightHandSelected()
        {
            HandSelectionManager.Instance.SetMainHand(HandType.Right);
            Debug.Log("[HandSelectionUI] Player selected RIGHT hand as main hand");

            if (autoCloseAfterSelection)
            {
                Invoke(nameof(CloseUI), autoCloseDelay);
            }
        }

        /// <summary>
        /// Called when hand preference changes
        /// </summary>
        private void OnHandPreferenceChanged(HandType newMainHand)
        {
            UpdateUI();
        }

        /// <summary>
        /// Updates UI to reflect current hand selection
        /// </summary>
        private void UpdateUI()
        {
            HandType mainHand = HandSelectionManager.Instance.MainHand;
            HandType offHand = HandSelectionManager.Instance.OffHand;

            // Update button colors
            if (leftHandButton != null)
            {
                ColorBlock colors = leftHandButton.colors;
                colors.normalColor = mainHand == HandType.Left ? selectedColor : unselectedColor;
                leftHandButton.colors = colors;
            }

            if (rightHandButton != null)
            {
                ColorBlock colors = rightHandButton.colors;
                colors.normalColor = mainHand == HandType.Right ? selectedColor : unselectedColor;
                rightHandButton.colors = colors;
            }

            // Update labels
            if (showRoleDescriptions)
            {
                if (leftHandLabel != null)
                {
                    string role = mainHand == HandType.Left ? "MAIN HAND\nPhysical Damage" : "OFF HAND\nSpirit Abilities";
                    leftHandLabel.text = $"Left Hand\n<size=60%>{role}</size>";
                }

                if (rightHandLabel != null)
                {
                    string role = mainHand == HandType.Right ? "MAIN HAND\nPhysical Damage" : "OFF HAND\nSpirit Abilities";
                    rightHandLabel.text = $"Right Hand\n<size=60%>{role}</size>";
                }
            }

            // Update selection text
            if (selectionText != null)
            {
                selectionText.text = $"Main Hand: {mainHand}\nOff Hand: {offHand}\n\n" +
                                    $"<size=80%>Main Hand = Physical Damage\n" +
                                    $"Off Hand = Spirit Abilities</size>";
            }
        }

        /// <summary>
        /// Closes/hides the UI panel
        /// </summary>
        private void CloseUI()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the UI panel
        /// </summary>
        public void ShowUI()
        {
            gameObject.SetActive(true);
            UpdateUI();
        }

        /// <summary>
        /// Public method to swap hands (can be called from button)
        /// </summary>
        public void SwapHands()
        {
            HandSelectionManager.Instance.SwapHands();
            Debug.Log("[HandSelectionUI] Hands swapped via UI");
        }

#if UNITY_EDITOR
        [ContextMenu("Simulate Left Hand Selection")]
        private void DebugSelectLeft()
        {
            OnLeftHandSelected();
        }

        [ContextMenu("Simulate Right Hand Selection")]
        private void DebugSelectRight()
        {
            OnRightHandSelected();
        }
#endif
    }
}
