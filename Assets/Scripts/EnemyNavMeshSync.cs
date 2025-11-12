using System;
using UnityEngine;

public class EnemyNavMeshSync : MonoBehaviour
{
    public Transform miniatureTransform;
    public Animator animator;
    public Animator miniatureAnimator;

    private void Update()
    {
        miniatureTransform.localPosition = transform.localPosition;
        miniatureTransform.localRotation = transform.localRotation;
        miniatureAnimator.enabled = animator.enabled;
    }
}