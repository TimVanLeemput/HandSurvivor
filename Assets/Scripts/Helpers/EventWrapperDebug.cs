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
    public void TriggerWhenSelect()
    {
        _interactableUnityEventWrapper.WhenSelect.Invoke();
    }
    
    [ButtonMethod]
    public void TriggerWhenUnselect()
    {
        _interactableUnityEventWrapper.WhenUnselect.Invoke();
    }
}
