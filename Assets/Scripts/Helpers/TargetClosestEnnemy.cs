using HandSurvivor.ActiveSkills;
using UnityEngine;

public class TargetClosestEnnemy : MonoBehaviour
{
    public Transform Reference;
    public Ennemy Target;
    private Ennemy previousTarget;
    private ActiveSkillBase activeSkill;

    public bool IsActive;

    private void Start()
    {
        activeSkill = GetComponentInParent<ActiveSkillBase>();
        if (activeSkill == null)
        {
            Debug.LogError("[TargetClosestEnnemy] No ActiveSkillBase found in parent");
        }
    }

    private void Update()
    {
        if (!IsActive) return;

        if (previousTarget != null)
            previousTarget.isTargeted = false;

        Target = FindClosestUntargetedEnemy();
        if (Target != null)
        {
            Target.isTargeted = true;
            int damagePerFrame = Mathf.RoundToInt(GetModifiedDPS() * Time.deltaTime);
            Target.TakeDamage(damagePerFrame);
            transform.position = Target.transform.position;
            previousTarget = Target;
        }
    }

    private float GetModifiedDPS()
    {
        if (activeSkill != null && activeSkill.Data != null)
        {
            return activeSkill.GetModifiedDamage(activeSkill.Data.damage);
        }
        return 100f;
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