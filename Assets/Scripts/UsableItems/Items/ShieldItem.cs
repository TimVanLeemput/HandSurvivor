using UnityEngine;

public class ShieldItem : UsableItemBase
{
    [Header("Shield Settings")]
    [SerializeField] private float _shieldDuration = 5f;
    [SerializeField] private GameObject _shieldVFXPrefab;

    protected override void OnUsed()
    {
        ApplyShieldToNexus();
    }

    private void ApplyShieldToNexus()
    {
        if (Nexus.Instance == null)
        {
            Debug.LogWarning("[ShieldItem] Nexus instance not found!");
            return;
        }

        Nexus.Instance.ApplyShield(_shieldDuration, _shieldVFXPrefab);
        Debug.Log($"[ShieldItem] Applied {_shieldDuration}s shield to Nexus");
    }
}
