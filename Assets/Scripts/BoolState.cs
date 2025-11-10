using UnityEngine;
using UnityEngine.Events;

public class BoolState : MonoBehaviour
{
    [SerializeField] private bool _state = false;

    [Header("Events")]
    public UnityEvent OnStateTrue;
    public UnityEvent OnStateFalse;

    public bool State
    {
        get => _state;
        set => _state = value;
    }

    public void SetState(bool state)
    {
        _state = state;
    }

    public void SetTrue()
    {
        _state = true;
    }

    public void SetFalse()
    {
        _state = false;
    }

    public void SetTrueAndCheck()
    {
        _state = true;
        CheckState();
    }

    public void SetFalseAndCheck()
    {
        _state = false;
        CheckState();   
    }

    public void CheckState()
    {
        if (_state)
        {
            OnStateTrue?.Invoke();
        }
        else
        {
            OnStateFalse?.Invoke();
        }
    }
}
