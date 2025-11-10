using System.Collections;
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
        if (nevMeshAgent == null)
            nevMeshAgent = GetComponentInParent<NavMeshAgent>();

        // Récupère tous les rigidbodies du personnage
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
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
            col.enabled = active;
        }
    }

    public void OnGrab()
    {
        nevMeshAgent.enabled = false;
        nevMeshAgent.isStopped = true;
        SetRagdoll(true);
    }

    public void OnRelease()
    {
        StartCoroutine(WaitAndDestroy());
    }

    public IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(2);
        Destroy(animator.gameObject);
    }
}