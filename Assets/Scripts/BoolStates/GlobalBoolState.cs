using MyBox;
using UnityEngine;
using UnityEngine.Events;

public class GlobalBoolState : MonoBehaviour
{
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
            Debug.Log($"[GlobalBoolState] Subscribed to {globalBoolStateData.name} OnStateChanged event", this);
        }
        else
        {
            Debug.LogWarning("[GlobalBoolState] GlobalBoolStateData is null!", this);
        }

        if (checkOnStart)
        {
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
    //         Debug.Log($"[GlobalBoolState] Unsubscribed from {globalBoolStateData.name} OnStateChanged event", this);
    //     }
    // }

    private void OnStateChanged(bool newState)
    {
        Debug.Log($"[GlobalBoolState] OnStateChanged triggered with state: {newState}", this);
        CheckState();
    }

    public void SetState(bool state)
    {
        if (globalBoolStateData != null)
        {
            Debug.Log($"[GlobalBoolState] SetState called with: {state}", this);
            globalBoolStateData.SetState(state);
        }
    }

    public void SetTrue()
    {
        if (globalBoolStateData != null)
        {
            Debug.Log($"[GlobalBoolState] SetTrue called", this);
            globalBoolStateData.SetTrue();
        }
    }

    public void SetFalse()
    {
        if (globalBoolStateData != null)
        {
            Debug.Log($"[GlobalBoolState] SetFalse called", this);
            globalBoolStateData.SetFalse();
        }
    }

    public void Toggle()
    {
        if (globalBoolStateData != null)
        {
            Debug.Log($"[GlobalBoolState] Toggle called", this);
            globalBoolStateData.Toggle();
        }
    }

    public void SetTrueAndCheck()
    {
        Debug.Log($"[GlobalBoolState] SetTrueAndCheck called", this);
        SetTrue();
        CheckState();
    }

    public void SetFalseAndCheck()
    {
        Debug.Log($"[GlobalBoolState] SetFalseAndCheck called", this);
        SetFalse();
        CheckState();
    }

    public void ToggleAndCheck()
    {
        Debug.Log($"[GlobalBoolState] ToggleAndCheck called", this);
        Toggle();
        CheckState();
    }

    [ButtonMethod, ContextMenu("Check States")]
    public void CheckState()
    {
        if (globalBoolStateData == null)
        {
            Debug.LogWarning("[GlobalBoolState] Cannot CheckState - GlobalBoolStateData is null!", this);
            return;
        }

        Debug.Log($"[GlobalBoolState] CheckState called - Current state: {globalBoolStateData.State}", this);

        if (globalBoolStateData.State)
        {
            Debug.Log($"[GlobalBoolState] Invoking OnStateTrue event", this);
            OnStateTrue?.Invoke();
        }
        else
        {
            Debug.Log($"[GlobalBoolState] Invoking OnStateFalse event", this);
            OnStateFalse?.Invoke();
        }
    }
}