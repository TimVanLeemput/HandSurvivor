using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Events;

public class GlobalBoolStateChecker : MonoBehaviour
{
    [SerializeField] private List<GlobalBoolStateData> globalBoolStates = new List<GlobalBoolStateData>();

    [Header("Auto Check Settings")] [SerializeField]
    private bool checkOnStart = true;

    [Header("Events")] public UnityEvent OnAllTrue;
    public UnityEvent OnAllFalse;
    public UnityEvent OnNotAllTrue;
    public UnityEvent OnNotAllFalse;

    public List<GlobalBoolStateData> GlobalBoolStates => globalBoolStates;

    private void OnEnable()
    {
        foreach (GlobalBoolStateData globalBoolState in globalBoolStates)
        {
            if (globalBoolState != null)
            {
                globalBoolState.OnStateChanged += OnStateChanged;
            }
        }

        if (checkOnStart)
        {
            CheckStates();
        }
    }

    private void OnDisable()
    {
        foreach (GlobalBoolStateData globalBoolState in globalBoolStates)
        {
            if (globalBoolState != null)
            {
                globalBoolState.OnStateChanged -= OnStateChanged;
            }
        }
    }

    private void OnStateChanged(bool newState)
    {
        CheckStates();
    }

    [ButtonMethod, ContextMenu("Check States")]
    public void CheckStates()
    {
        if (globalBoolStates == null || globalBoolStates.Count == 0)
        {
            return;
        }

        bool allTrue = true;
        bool allFalse = true;

        foreach (GlobalBoolStateData globalBoolState in globalBoolStates)
        {
            if (globalBoolState == null)
            {
                continue;
            }

            if (globalBoolState.State)
            {
                allFalse = false;
            }
            else
            {
                allTrue = false;
            }
        }

        if (allTrue)
        {
            OnAllTrue?.Invoke();
        }
        else
        {
            OnNotAllTrue?.Invoke();
        }

        if (allFalse)
        {
            OnAllFalse?.Invoke();
        }
        else
        {
            OnNotAllFalse?.Invoke();
        }
    }
}