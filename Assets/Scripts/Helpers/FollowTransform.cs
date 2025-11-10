using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform Target;

    public bool IsActive;

    private void Update()
    {
        if (!IsActive) return;
        
        transform.position = Target.position;
    }
}
