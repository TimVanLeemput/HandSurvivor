using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.AI;

public class RagdollController_Medium : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent nevMeshAgent;
    [SerializeField] private Rigidbody[] ragdollBodies;
    [SerializeField] private Collider[] ragdollColliders;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInParent<Animator>();

        if (ragdollBodies == null || ragdollBodies.Length == 0)
            ragdollBodies = GetComponentsInChildren<Rigidbody>();
        if (ragdollColliders == null || ragdollColliders.Length == 0)
            ragdollColliders = GetComponentsInChildren<Collider>();

        SetRagdoll(false);
    }

    public void SetRagdoll(bool active)
    {
        animator.enabled = !active;

        foreach (var rb in ragdollBodies)
            rb.isKinematic = !active;
        

        foreach (var col in ragdollColliders)
        {
            if (col.gameObject == this.gameObject) continue;
            col.enabled = active;
        }
    }

    [ButtonMethod]
    public void SetRagdollTrue()
    {
        SetRagdoll(true);
    }

    [ButtonMethod]
    public void SetRagdollFalse()
    {
        SetRagdoll(false);
    }

    public void OnGrab()
    {
        Destroy(animator.GetComponent<InvisibleEnemyRef>().Ref.gameObject);
        SetRagdoll(true);
        animator.GetComponent<Enemy>().DropXP();
    }

    public void OnRelease()
    {
        animator.GetComponent<Enemy>().Die(false);
    }
}