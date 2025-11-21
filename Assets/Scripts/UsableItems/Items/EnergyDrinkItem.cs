using UnityEngine;
using System.Reflection;
using HandSurvivor.Interfaces;

public class EnergyDrinkItem : UsableItemBase
{
    [Header("Nexus HP Restore")]
    [SerializeField] private HPRestoreType _hpRestoreType = HPRestoreType.Fixed;
    [SerializeField] private int _fixedHPRestore = 100;
    [SerializeField] private float _percentageHPRestore = 0.25f;

    [Header("Charge Refill")]
    [SerializeField] private bool _refillAllCharges = true;

    public enum HPRestoreType
    {
        Fixed,
        Percentage
    }

    protected override void OnUsed()
    {
        RestoreNexusHP();

        if (_refillAllCharges)
            RefillAllCharges();
    }

    private void RestoreNexusHP()
    {
        if (Nexus.Instance == null)
        {
            Debug.LogWarning("[EnergyDrinkItem] Nexus instance not found!");
            return;
        }

        switch (_hpRestoreType)
        {
            case HPRestoreType.Fixed:
                Nexus.Instance.RestoreHealth(_fixedHPRestore);
                Debug.Log($"[EnergyDrinkItem] Restored {_fixedHPRestore} HP to Nexus");
                break;

            case HPRestoreType.Percentage:
                Nexus.Instance.RestoreHealthPercentage(_percentageHPRestore);
                int amount = Mathf.CeilToInt(Nexus.Instance.MaxHP * _percentageHPRestore);
                Debug.Log($"[EnergyDrinkItem] Restored {amount} HP ({_percentageHPRestore * 100}%) to Nexus");
                break;
        }
    }

    private void RefillAllCharges()
    {
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        int refillCount = 0;

        foreach (MonoBehaviour mb in allMonoBehaviours)
        {
            if (mb is IChargeableSkill)
            {
                FieldInfo currentChargesField = mb.GetType().GetField("currentCharges", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo maxChargesField = mb.GetType().GetField("maxCharges", BindingFlags.NonPublic | BindingFlags.Instance);

                if (currentChargesField != null && maxChargesField != null)
                {
                    int maxCharges = (int)maxChargesField.GetValue(mb);
                    currentChargesField.SetValue(mb, maxCharges);
                    refillCount++;
                }
            }
        }

        if (refillCount > 0)
            Debug.Log($"[EnergyDrinkItem] Refilled charges for {refillCount} skills");
        else
            Debug.Log("[EnergyDrinkItem] No chargeable skills found to refill");
    }
}
