using UnityEngine;
using UnityEngine.Events;

public class OnStart : MonoBehaviour
{
    [SerializeField] private UnityEvent onStart = null;
    void Start()
    {
        onStart?.Invoke();
    }
}
