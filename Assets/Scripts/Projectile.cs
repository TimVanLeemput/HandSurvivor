using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 10f;
    [Range(0,1)] public float Parabol;
    public int Damage = 10;
    
    private void Start()
    {
        StartCoroutine(AimTarget());
    }

    private IEnumerator AimTarget()
    {
        Transform Target = NexusMiniatureRef.Instance.transform;
        Vector3 startPos = transform.position;
        Vector3 targetPos = Target.position;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / Speed;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Mouvement linéaire de base
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // Calcul de l'arc parabolique
            // On utilise une fraction de la distance (ex: 0.5) pour déterminer la hauteur max de l'arc
            float height = Mathf.Sin(t * Mathf.PI) * (distance * 0.5f);
                
            // Application du facteur Parabol (0 = tout droit, 1 = courbe complète)
            currentPos.y += height * Parabol;

            // Rotation pour regarder vers la prochaine position (comme une flèche)
            transform.LookAt(currentPos);

            transform.position = currentPos;
            yield return null;
        }

        // Assurer la position finale exacte
        transform.position = targetPos;
        Nexus.Instance.TakeDamage(Damage);
        Destroy(gameObject);
    }
}
