using HandSurvivor;
using Oculus.Interaction;
using Oculus.Interaction.Locomotion;
using UnityEngine;

public class WristMenuToggle : MonoBehaviour
{
    [Header("Menu")] [SerializeField] private GameObject _menu;
    [SerializeField] private GlobalBoolStateData _canActivateWristMenuGlobalState = null;

    [Header("Wrist Angle Active State")] [SerializeField]
    private WristAngleActiveState _wristAngleActiveState;

    [Header("Debug")] [SerializeField] private bool _showDebugLogs = false;

    private bool _wasActive = false;

    private void Start()
    {
        if (_wristAngleActiveState == null)
        {
            Debug.LogError($"WristAngleActiveState not assigned on {gameObject.name}");
            enabled = false;
            return;
        }

        if (_menu == null)
        {
            Debug.LogError($"Menu GameObject not assigned on {gameObject.name}");
            enabled = false;
            return;
        }

        _menu.SetActive(false);
    }

    private void Update()
    {
        bool isActive = _wristAngleActiveState.Active;
        if (_canActivateWristMenuGlobalState.State)
        {
            if (isActive && !_wasActive)
            {
                _menu.SetActive(true);
                if (_showDebugLogs)
                {
                    Debug.Log("<color=green>Wrist menu ACTIVATED</color>");
                }
            }
            else if (!isActive && _wasActive)
            {
                _menu.SetActive(false);
                if (_showDebugLogs)
                {
                    Debug.Log("<color=red>Wrist menu DEACTIVATED</color>");
                }
            }
        }

        _wasActive = isActive;
    }
}