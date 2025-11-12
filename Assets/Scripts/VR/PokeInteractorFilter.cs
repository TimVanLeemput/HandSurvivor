using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

public class ThumbOnlyInteractorFilter : MonoBehaviour, IGameObjectFilter
{
    [SerializeField] private HandJointId _handJointId = HandJointId.Invalid;
    
    public bool Filter(GameObject go)
    {
        if (go.GetComponentInChildren<HandJoint>() == null)
            return false;
        else
            return go.GetComponentInChildren<HandJoint>()?.HandJointId == _handJointId;
    }
}