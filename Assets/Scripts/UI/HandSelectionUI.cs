using HandSurvivor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple hand selection toggle UI.
/// Single button that swaps between left/right hand as main hand.
/// </summary>
public class HandSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] [Tooltip("Toggle button to swap main hand")]
    private Button toggleButton;

    [SerializeField] [Tooltip("Label showing current main hand")]
    private TextMeshProUGUI handLabel;

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleMainHand);
        }

        if (HandSelectionManager.Instance != null)
        {
            HandSelectionManager.Instance.OnMainHandChanged.AddListener(UpdateLabel);
            HandSelectionManager.Instance.OnPreferenceLoaded.AddListener(UpdateLabel);
        }

        UpdateLabel(HandSelectionManager.Instance.MainHand);
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleMainHand);
        }

        if (HandSelectionManager.Instance != null)
        {
            HandSelectionManager.Instance.OnMainHandChanged.RemoveListener(UpdateLabel);
            HandSelectionManager.Instance.OnPreferenceLoaded.RemoveListener(UpdateLabel);
        }
    }

    private void ToggleMainHand()
    {
        HandSelectionManager.Instance.SwapHands();
    }

    private void UpdateLabel(HandType mainHand)
    {
        if (handLabel != null)
        {
            handLabel.text = mainHand.ToString();
        }
    }
}
