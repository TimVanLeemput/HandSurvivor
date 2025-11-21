using HandSurvivor.Core.Passive;
using HandSurvivor.Upgrades;
using UnityEngine;
using UnityEngine.Events;

public abstract class UsableItemBase : MonoBehaviour, IUpgradeable
{
    [Header("Item Data")]
    [SerializeField] protected UsableItemData _data;

    [Header("Events")]
    public UnityEvent OnPickup;
    public UnityEvent OnUse;
    public UnityEvent OnConsume;

    protected AudioSource _audioSource;

    protected virtual void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }

    public virtual void Pickup()
    {
        if (UsableItemInventory.Instance != null)
        {
            UsableItemInventory.Instance.AddItem(this);
            OnPickup?.Invoke();
        }
    }

    public void Use()
    {
        if (CanUse())
        {
            OnUse?.Invoke();
            OnUsed();
            PlayUseSound();

            if (UsableItemInventory.Instance != null)
            {
                UsableItemInventory.Instance.RemoveItem(_data.ItemId, 1);
                OnConsume?.Invoke();
            }
        }
    }

    protected virtual bool CanUse()
    {
        return true;
    }

    protected abstract void OnUsed();

    protected virtual void PlayUseSound()
    {
        if (_data.UseSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_data.UseSound);
        }
    }

    public UsableItemData GetData() => _data;

    public virtual string GetUpgradeableId()
    {
        return _data != null ? _data.ItemId : string.Empty;
    }

    public virtual void ApplyPassiveUpgrade(PassiveUpgradeData upgrade)
    {
    }
}
