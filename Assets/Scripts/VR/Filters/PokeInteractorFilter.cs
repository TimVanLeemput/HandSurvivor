using HandSurvivor;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using HandSurvivor.Utilities;

public enum HandFilterType
{
    Both,
    MainHandOnly,
    OffHandOnly
}

public class PokeInteractorFilter : MonoBehaviour, IGameObjectFilter
{
    public HandJointId Joint { get; private set; }
    public HandFilterType HandFilterType { get; private set; }

    private void Awake()
    {
        // Auto-detect the Joint ID from the child HandJoint
        HandJoint handJoint = GetComponentInChildren<HandJoint>();
        if (handJoint != null)
        {
            Joint = handJoint.HandJointId;
            Debug.Log($"[PokeInteractorFilter] Auto-detected Joint ID: {Joint}");
        }
        else
        {
            Debug.LogWarning($"[PokeInteractorFilter] No HandJoint found in children of {gameObject.name}");
        }

        // Auto-detect which hand this interactor is on
        HandType? handType = TargetHandFinder.GetHandTypeFromObject(gameObject);

        if (handType == null)
        {
            Debug.LogWarning($"[PokeInteractorFilter] Could not detect hand type for {gameObject.name} - defaulting to Both");
            HandFilterType = HandFilterType.Both;
            return;
        }

        // Check if this hand is the main hand or off hand
        bool isMainHand = HandSelectionManager.CheckIsMainHand(handType.Value);
        HandFilterType = isMainHand ? HandFilterType.MainHandOnly : HandFilterType.OffHandOnly;

        Debug.Log($"[PokeInteractorFilter] Auto-detected hand type: {handType.Value} ({HandFilterType})", gameObject);
    }

    public bool Filter(GameObject go)
    {
        // This filter is not used - the PokeInteractableHandFilter on the interactable does the filtering
        return true;
    }
}