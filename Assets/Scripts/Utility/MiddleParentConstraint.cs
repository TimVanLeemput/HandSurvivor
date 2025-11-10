using System;
using UnityEngine;

public class MiddleParentConstraint : MonoBehaviour
{
    public Transform Parent1;
    public Transform Parent2;

    public bool IsActive;

    private void Update()
    {
        if (!IsActive) return;
        
        transform.position = (Parent1.position + Parent2.position) / 2;
    }
}
