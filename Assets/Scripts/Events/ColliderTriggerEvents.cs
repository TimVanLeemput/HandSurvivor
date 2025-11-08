using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ColliderTriggerEvents : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public UnityEvent<Collider> onTriggerStay;
    public UnityEvent<Collider> onTriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldTrigger(other))
        {
            onTriggerEnter?.Invoke();
            OnFilteredTriggerEnter(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (ShouldTrigger(other))
        {
            onTriggerStay?.Invoke(other);
            OnFilteredTriggerStay(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ShouldTrigger(other))
        {
            onTriggerExit?.Invoke(other);
            OnFilteredTriggerExit(other);
        }
    }

    protected virtual bool ShouldTrigger(Collider other)
    {
        return true;
    }

    protected virtual void OnFilteredTriggerEnter(Collider other)
    {
    }

    protected virtual void OnFilteredTriggerStay(Collider other)
    {
    }

    protected virtual void OnFilteredTriggerExit(Collider other)
    {
    }
}