using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalBoolState", menuName = "HandSurvivor/Global Bool State Data")]
public class GlobalBoolStateData : ScriptableObject
{
    [SerializeField] private bool _state = false;

    public event Action<bool> OnStateChanged;

    public bool State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                OnStateChanged?.Invoke(_state);
            }
        }
    }

    public void SetState(bool state)
    {
        State = state;
    }

    public void SetTrue()
    {
        State = true;
    }

    public void SetFalse()
    {
        State = false;
    }

    public void SetTrueAndCheck()
    {
        _state = true;
        OnStateChanged?.Invoke(_state);
    }

    public void SetFalseAndCheck()
    {
        _state = false;
        OnStateChanged?.Invoke(_state);
    }

    public void Toggle()
    {
        State = !_state;
    }

    public void ResetState()
    {
        State = false;
    }
}
