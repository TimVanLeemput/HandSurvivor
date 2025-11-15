using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody[] ragdollBodies;
    [SerializeField] private Collider[] ragdollColliders;

    private bool isRagdoll = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInParent<Animator>();

        // Récupère tous les rigidbodies du personnage
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        SetRagdoll(false);
    }

    public void SetRagdoll(bool active)
    {
        isRagdoll = active;
        animator.enabled = !active;

        foreach (var rb in ragdollBodies)
            rb.isKinematic = !active;

        foreach (var col in ragdollColliders)
        {
            if (col.gameObject == this.gameObject) continue; // ignore le collider principal
            col.enabled = active;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isRagdoll) return;
        Destroy(animator.GetComponent<InvisibleEnemyRef>().Ref.gameObject);

        Rigidbody rb = collision.rigidbody;
        Vector3 direction = collision.contacts[0].normal * -1f; // direction d'impact
        rb.AddForce(direction * 50f, ForceMode.Impulse);
        SetRagdoll(true);

        animator.GetComponent<Enemy>().Die();
    }
}