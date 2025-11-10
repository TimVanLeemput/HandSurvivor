using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoolStateChecker : MonoBehaviour
{
    [SerializeField] private List<BoolState> _boolStates = new List<BoolState>();

    [Header("Events")]
    public UnityEvent OnAllTrue;
    public UnityEvent OnAllFalse;
    public UnityEvent OnNotAllTrue;
    public UnityEvent OnNotAllFalse;

    public List<BoolState> BoolStates => _boolStates;

    public void CheckStates()
    {
        if (_boolStates == null || _boolStates.Count == 0)
        {
            return;
        }

        bool allTrue = true;
        bool allFalse = true;

        foreach (BoolState boolState in _boolStates)
        {
            if (boolState == null)
            {
                continue;
            }

            if (boolState.State)
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
