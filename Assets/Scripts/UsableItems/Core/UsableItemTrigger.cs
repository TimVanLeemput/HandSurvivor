using UnityEngine;
using UnityEngine.Events;

public class UsableItemTrigger : MonoBehaviour
{
    [Header("Item Configuration")]
    [SerializeField] private UsableItemData _itemData;

    [Header("Feedback Events")]
    public UnityEvent OnItemUsedSuccessfully;
    public UnityEvent OnItemNotFound;
    public UnityEvent OnNoInventory;

    public void UseItem()
    {
        if (_itemData == null)
        {
            Debug.LogWarning($"[UsableItemTrigger] No UsableItemData assigned!");
            OnItemNotFound?.Invoke();
            return;
        }

        if (UsableItemInventory.Instance == null)
        {
            Debug.LogWarning($"[UsableItemTrigger] No UsableItemInventory found in scene!");
            OnNoInventory?.Invoke();
            return;
        }

        string itemId = _itemData.ItemId;

        if (!UsableItemInventory.Instance.HasItem(itemId))
        {
            Debug.LogWarning($"[UsableItemTrigger] Item '{_itemData.DisplayName}' not found in inventory!");
            OnItemNotFound?.Invoke();
            return;
        }

        UsableItemBase item = UsableItemInventory.Instance.GetItem(itemId);

        if (item != null)
        {
            item.Use();
            OnItemUsedSuccessfully?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[UsableItemTrigger] Item '{_itemData.DisplayName}' exists but couldn't retrieve instance!");
            OnItemNotFound?.Invoke();
        }
    }

    public void SetItemData(UsableItemData itemData)
    {
        _itemData = itemData;
    }

    public UsableItemData GetItemData() => _itemData;
}
