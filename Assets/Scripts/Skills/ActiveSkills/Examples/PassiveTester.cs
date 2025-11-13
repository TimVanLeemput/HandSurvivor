using UnityEngine;
using HandSurvivor.Core.Passive;
using HandSurvivor.ActiveSkills;

public class PassiveTester : MonoBehaviour
{
    [SerializeField] private PassiveUpgradeData testPassive;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ApplyTestPassive();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowSkillStats();
        }
    }

    void ApplyTestPassive()
    {
        if (testPassive == null) return;

        PassiveUpgradeManager.Instance.ApplyUpgrade(testPassive);
        Debug.Log($"Applied passive: {testPassive.displayName}");
    }

    void ShowSkillStats()
    {
        if (ActiveSkillSlotManager.Instance == null) return;

        foreach (ActiveSkillBase skill in
                 ActiveSkillSlotManager.Instance.GetSlottedSkills())
        {
            Debug.Log($"{skill.Data.displayName}: " +
                      $"Cooldown x{skill.CooldownMultiplier:F2}, " +
                      $"Damage x{skill.DamageMultiplier:F2}, " +
                      $"Size x{skill.SizeMultiplier:F2}");
        }
    }
}