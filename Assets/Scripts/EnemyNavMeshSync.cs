using System;
using Unity.Behavior;
using UnityEngine;

public class EnemyNavMeshSync : MonoBehaviour
{
    public Transform miniatureTransform;
    public Animator animator;
    public Animator miniatureAnimator;

    private void OnEnable()
    {
        EnemyNavMeshSyncManager.Register(this);
    }

    private void OnDisable()
    {
        EnemyNavMeshSyncManager.Unregister(this);
    }

    public void Attack()
    {
        miniatureAnimator.SetTrigger("Attack");
    }

    public void SyncTransforms()
    {
        miniatureTransform.localPosition = transform.localPosition;
        miniatureTransform.localRotation = transform.localRotation;
        miniatureAnimator.enabled = animator.enabled;
    }
}