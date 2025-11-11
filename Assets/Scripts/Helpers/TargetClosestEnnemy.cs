using UnityEngine;

public class TargetClosestEnnemy : MonoBehaviour
{
    public Transform Reference;
    public Ennemy Target;
    public int DPS = 100;
    private Ennemy previousTarget;

    public bool IsActive;

    private void Update()
    {
        if (!IsActive) return;

        if (previousTarget != null)
            previousTarget.isTargeted = false;

        Target = FindClosestUntargetedEnemy();
        if (Target != null)
        {
            Target.isTargeted = true;
            Target.TakeDamage(Mathf.RoundToInt(DPS * Time.deltaTime));
            transform.position = Target.transform.position;
            previousTarget = Target;
        }
    }

    private Ennemy FindClosestUntargetedEnemy()
    {
        var enemies = WavesManager.Instance.CurrentEnnemies;

        if (enemies == null || enemies.Count == 0)
            return null;

        Ennemy closest = null;
        float minDistanceSqr = float.MaxValue;
        Vector3 referencePos = Reference != null ? Reference.position : transform.position;

        foreach (var enemy in enemies)
        {
            // Vérifier que l'ennemi existe et n'est pas ciblé
            if (enemy == null || enemy.isTargeted)
                continue;

            // Utiliser la distance au carré pour éviter les calculs de racine carrée
            float distanceSqr = (enemy.transform.position - referencePos).sqrMagnitude;

            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                closest = enemy;
            }
        }

        return closest;
    }
}