using System;
using Unity.Behavior;
using UnityEngine;

public class EnemyNavMeshSync : MonoBehaviour
{
    public Transform miniatureTransform;
    public Animator animator;
    public Animator miniatureAnimator;

    private BlackboardVariable<Attack> attack;
    private BehaviorGraphAgent behaviorGraphAgent;

    private void Start()
    {
        behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        if (behaviorGraphAgent.BlackboardReference.GetVariable("Attack", out attack))
        {
            attack.Value.Event += OnAttack;
        }
    }

    private void OnAttack()
    {
        miniatureAnimator.SetTrigger("Attack");
    }

    private void Update()
    {
        miniatureTransform.localPosition = transform.localPosition;
        miniatureTransform.localRotation = transform.localRotation;
        miniatureAnimator.enabled = animator.enabled;
    }
    
    private void OnDestroy()
    {
        if (attack != null && attack.Value != null)
        {
            attack.Value.Event -= OnAttack;
        }
    }
}