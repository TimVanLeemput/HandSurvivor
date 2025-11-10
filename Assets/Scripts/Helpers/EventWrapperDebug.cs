using MyBox;
using Oculus.Interaction;
using UnityEngine;

public class EventWrapperDebug : MonoBehaviour
{
    private InteractableUnityEventWrapper _interactableUnityEventWrapper = null;
    void Start()
    {
        _interactableUnityEventWrapper = GetComponent<InteractableUnityEventWrapper>();
    }

    [ButtonMethod]
    private void TriggerWhenSelect()
    {
        _interactableUnityEventWrapper.WhenSelect.Invoke();
    }
    
    [ButtonMethod]
    private void TriggerWhenUnselect()
    {
        _interactableUnityEventWrapper.WhenUnselect.Invoke();
    }
}
