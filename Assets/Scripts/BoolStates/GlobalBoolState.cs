using MyBox;
using UnityEngine;
using UnityEngine.Events;

public class GlobalBoolState : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [SerializeField] private GlobalBoolStateData globalBoolStateData;

    [Header("Auto Check Settings")] [SerializeField]
    private bool checkOnStart = true;

    [Header("Events")] public UnityEvent OnStateTrue;
    public UnityEvent OnStateFalse;

    public bool State
    {
        get => globalBoolStateData != null ? globalBoolStateData.State : false;
        set
        {
            if (globalBoolStateData != null)
            {
                globalBoolStateData.State = value;
            }
        }
    }

    private void OnEnable()
    {
        if (globalBoolStateData != null)
        {
            globalBoolStateData.OnStateChanged += OnStateChanged;
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[GlobalBoolState] Subscribed to {globalBoolStateData.name} OnStateChanged event", this);
        }
        else
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogWarning("[GlobalBoolState] GlobalBoolStateData is null!", this);
        }

        if (checkOnStart)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[GlobalBoolState] CheckOnStart enabled, checking state", this);
            CheckState();
        }
    }
    //
    // private void OnDisable()
    // {
    //     if (globalBoolStateData != null)
    //     {
    //         globalBoolStateData.OnStateChanged -= OnStateChanged;
    //         if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
             // Debug.Log($"[GlobalBoolState] Unsubscribed from {globalBoolStateData.name} OnStateChanged event", this);
    //     }
    // }

    private void OnStateChanged(bool newState)
    {
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

            Debug.Log($"[GlobalBoolState] OnStateChanged triggered with state: {newState}", this);
        CheckState();
    }

    public void SetState(bool state)
    {
        if (globalBoolStateData != null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[GlobalBoolState] SetState called with: {state}", this);
            globalBoolStateData.SetState(state);
        }
    }

    public void SetTrue()
    {
        if (globalBoolStateData != null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[GlobalBoolState] SetTrue called", this);
            globalBoolStateData.SetTrue();
        }
    }

    public void SetFalse()
    {
        if (globalBoolStateData != null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[GlobalBoolState] SetFalse called", this);
            globalBoolStateData.SetFalse();
        }
    }

    public void Toggle()
    {
        if (globalBoolStateData != null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[GlobalBoolState] Toggle called", this);
            globalBoolStateData.Toggle();
        }
    }

    public void SetTrueAndCheck()
    {
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

            Debug.Log($"[GlobalBoolState] SetTrueAndCheck called", this);
        SetTrue();
        CheckState();
    }

    public void SetFalseAndCheck()
    {
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

            Debug.Log($"[GlobalBoolState] SetFalseAndCheck called", this);
        SetFalse();
        CheckState();
    }

    public void ToggleAndCheck()
    {
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

            Debug.Log($"[GlobalBoolState] ToggleAndCheck called", this);
        Toggle();
        CheckState();
    }

    [ButtonMethod, ContextMenu("Check States")]
    public void CheckState()
    {
        if (globalBoolStateData == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogWarning("[GlobalBoolState] Cannot CheckState - GlobalBoolStateData is null!", this);
            return;
        }

        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)


            Debug.Log($"[GlobalBoolState] CheckState called - Current state: {globalBoolStateData.State}", this);

        if (globalBoolStateData.State)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[GlobalBoolState] Invoking OnStateTrue event", this);
            OnStateTrue?.Invoke();
        }
        else
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"[GlobalBoolState] Invoking OnStateFalse event", this);
            OnStateFalse?.Invoke();
        }
    }
}