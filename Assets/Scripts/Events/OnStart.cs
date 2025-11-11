using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class OnStart : MonoBehaviour
{
    [SerializeField] private float delay = 0f;
    [SerializeField] private UnityEvent onStart = null;

    void Start()
    {
        if (delay > 0f)
        {
            StartCoroutine(InvokeAfterDelay());
        }
        else
        {
            onStart?.Invoke();
        }
    }

    private IEnumerator InvokeAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        onStart?.Invoke();
    }
}
