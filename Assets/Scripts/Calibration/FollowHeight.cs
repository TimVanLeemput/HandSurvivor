using System;
using UnityEngine;

public class FollowHeight : MonoBehaviour
{
    public GameObject Target = null;
    public bool ShouldFollow = false;

    private void Update()
    {
        if (ShouldFollow)
        {
            transform.localPosition = new Vector3(
                transform.position.x,
                Target.transform.localPosition.y,
                transform.position.z);
        }
    }

    public void ToggleFollow()
    {
        ShouldFollow = !ShouldFollow;
    }
}