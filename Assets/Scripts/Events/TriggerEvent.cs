using MyBox;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent triggerEvent = null;

    [ButtonMethod]
    public void Trigger()
    {
        triggerEvent?.Invoke();
    }
}
