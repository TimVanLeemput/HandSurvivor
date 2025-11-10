using UnityEngine;

public class FollowClosestPointOfCollider : MonoBehaviour
{
    public Transform Reference;
    public Collider Target;

    public bool IsActive;

    private void Update()
    {
        if (!IsActive) return;
        
        transform.position = Target.ClosestPoint(Reference.position);
    }
}
