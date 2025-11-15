using System.Collections;
using UnityEngine;

public class UFOAttractor : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float collectDuration = 2f;
    
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(AttractCoroutine(other));
    }
    
    private IEnumerator AttractCoroutine(Collider other)
    {
        Enemy enemy = FindEnemyInParents(other.transform);
        Destroy(enemy.GetComponent<InvisibleEnemyRef>().Ref.gameObject);
        Transform enemyTransform = enemy.transform;
        enemy.GetComponentInChildren<RagdollController>()?.SetRagdoll(true);
        enemy.GetComponentInChildren<RagdollController_Medium>()?.SetRagdoll(true);

        DisableAllChildColliders(enemy.gameObject);
        float elapsed = 0f;

        while (elapsed < collectDuration && enemyTransform != null)
        {
            elapsed += Time.deltaTime;

            float remaining = Mathf.Max(collectDuration - elapsed, 0.0001f);

            // On recalcule la direction vers le UFOAttractor à chaque frame
            Vector3 toTarget = target.position - enemyTransform.position;

            // Vitesse nécessaire pour atteindre la cible dans le temps restant
            Vector3 velocity = toTarget / remaining;

            enemyTransform.position += velocity * Time.deltaTime;

            yield return null;
        }

        if (enemyTransform != null)
            enemyTransform.position = target.position;

        yield return null;
        Destroy(enemyTransform.gameObject);
    }
    
    private Enemy FindEnemyInParents(Transform start)
    {
        Transform current = start;

        while (current != null)
        {
            Enemy enemy = current.GetComponent<Enemy>();
            if (enemy != null)
                return enemy;

            current = current.parent;
        }

        return null;
    }
    
    private void DisableAllChildColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        Rigidbody[] rbs = root.GetComponentsInChildren<Rigidbody>(includeInactive: true);
        foreach (var rb in rbs)
        {
            rb.useGravity = false;
        }
    }
}
