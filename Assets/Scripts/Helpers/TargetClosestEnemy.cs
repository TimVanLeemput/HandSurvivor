using HandSurvivor.ActiveSkills;
using HandSurvivor.Stats;
using UnityEngine;

public class TargetClosestEnemy : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         public Transform Reference;
    public Enemy Target;
    public int DPS = 100;
    private Enemy previousTarget;
    private ActiveSkillBase activeSkill;

    public bool IsActive;

    private float targetUpdateTimer = 0f;
    private const float TARGET_UPDATE_INTERVAL = 0.1f;

    private void Start()
    {
        activeSkill = GetComponentInParent<ActiveSkillBase>();
        if (activeSkill == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.LogError("[TargetClosestEnnemy] No ActiveSkillBase found in parent");
        }
    }

    private void Update()
    {
        if (!IsActive) return;

        targetUpdateTimer += Time.deltaTime;
        if (targetUpdateTimer >= TARGET_UPDATE_INTERVAL)
        {
            targetUpdateTimer = 0f;

            if (previousTarget != null)
                previousTarget.isTargeted = false;

            Target = FindClosestUntargetedEnemy();
            if (Target != null)
            {
                Target.isTargeted = true;
                previousTarget = Target;
            }
        }

        if (Target != null)
        {
            int damagePerFrame = Mathf.RoundToInt(GetModifiedDPS() * Time.deltaTime);
            Target.TakeDamage(damagePerFrame);

            // Track damage for stats
            if (PlayerStatsManager.Instance != null && activeSkill != null && activeSkill.Data != null)
                PlayerStatsManager.Instance.RecordDamage(activeSkill.Data.activeSkillId, damagePerFrame, Target.name);

            transform.position = Target.transform.position;
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

    private Enemy FindClosestUntargetedEnemy()
    {
        var enemies = WavesManager.Instance.CurrentEnnemies;

        if (enemies == null || enemies.Count == 0)
            return null;

        Enemy closest = null;
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