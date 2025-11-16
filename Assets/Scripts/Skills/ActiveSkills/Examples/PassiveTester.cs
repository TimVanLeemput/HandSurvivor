using UnityEngine;
using HandSurvivor.Core.Passive;
using HandSurvivor.ActiveSkills;

public class PassiveTester : MonoBehaviour
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [SerializeField] private PassiveUpgradeData testPassive;

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.P))
        {
            // ApplyTestPassive();
        }

        // if (Input.GetKeyDown(KeyCode.I))
        {
            // ShowSkillStats();
        }
    }

    void ApplyTestPassive()
    {
        if (testPassive == null) return;

        PassiveUpgradeManager.Instance.ApplyUpgrade(testPassive);
        if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

            Debug.Log($"Applied passive: {testPassive.displayName}");
    }

    void ShowSkillStats()
    {
        if (ActiveSkillSlotManager.Instance == null) return;

        foreach (ActiveSkillBase skill in
                 ActiveSkillSlotManager.Instance.GetSlottedSkills())
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                Debug.Log($"{skill.Data.displayName}: " +
                      $"Cooldown x{skill.CooldownMultiplier:F2}, " +
                      $"Damage x{skill.DamageMultiplier:F2}, " +
                      $"Size x{skill.SizeMultiplier:F2}");
        }
    }
}