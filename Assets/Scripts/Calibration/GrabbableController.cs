using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class GrabbableController : MonoBehaviour
{
    private HandGrabInteractable _handGrabInteractable;

    private void Start()
    {
        if (_handGrabInteractable == null)
        {
            _handGrabInteractable = GetComponent<HandGrabInteractable>();
        }
    }

    private void SetGrabbable(bool isGrabbable)
    {
        if (_handGrabInteractable != null)
        {
            _handGrabInteractable.enabled = isGrabbable;
        }
    }

    public void EnableGrabbing()
    {
        SetGrabbable(true);
    }

    public void DisableGrabbing()
    {
        SetGrabbable(false);
    }
}
