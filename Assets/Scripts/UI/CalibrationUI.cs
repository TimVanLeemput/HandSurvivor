using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HandSurvivor;

public class CalibrationUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARPlaneCalibration planeCalibration;
    [SerializeField] private Button lockButton;
    [SerializeField] private Button unlockButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Aspect Ratio Presets")]
    [SerializeField] private Button squareButton;
    [SerializeField] private Button wideButton;
    [SerializeField] private Button narrowButton;

    private void Start()
    {
        if (planeCalibration == null)
        {
            planeCalibration = FindFirstObjectByType<ARPlaneCalibration>();
        }

        if (lockButton != null)
        {
            lockButton.onClick.AddListener(OnLockClicked);
        }

        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(OnUnlockClicked);
            unlockButton.gameObject.SetActive(false);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }

        if (squareButton != null)
        {
            squareButton.onClick.AddListener(() => planeCalibration.AdjustAspectRatio(1f, 1f));
        }

        if (wideButton != null)
        {
            wideButton.onClick.AddListener(() => planeCalibration.AdjustAspectRatio(1.5f, 1f));
        }

        if (narrowButton != null)
        {
            narrowButton.onClick.AddListener(() => planeCalibration.AdjustAspectRatio(1f, 1.5f));
        }

        UpdateInstructions();
    }

    private void OnLockClicked()
    {
        planeCalibration.LockPlane();

        if (lockButton != null)
        {
            lockButton.gameObject.SetActive(false);
        }

        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(true);
        }

        UpdateInstructions();
    }

    private void OnUnlockClicked()
    {
        planeCalibration.UnlockPlane();

        if (lockButton != null)
        {
            lockButton.gameObject.SetActive(true);
        }

        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(false);
        }

        UpdateInstructions();
    }

    private void OnResetClicked()
    {
        planeCalibration.ResetPlane();

        if (lockButton != null)
        {
            lockButton.gameObject.SetActive(true);
        }

        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(false);
        }

        UpdateInstructions();
    }

    private void UpdateInstructions()
    {
        if (instructionText == null) return;

        if (planeCalibration.IsLocked)
        {
            instructionText.text = "Plane locked! Game ready to start.";
        }
        else
        {
            instructionText.text = "Position plane:\nLeft stick - Move\nRight stick - Rotate/Scale\nTriggers - Up/Down";
        }
    }
}
