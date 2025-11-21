using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UsableItemInventory : MonoBehaviour
{
    public static UsableItemInventory Instance;

    [System.Serializable]
    public class UsableItemStack
    {
        public UsableItemBase ItemInstance;
        public int Count;

        public UsableItemStack(UsableItemBase item, int count)
        {
            ItemInstance = item;
            Count = count;
        }
    }

    [Header("Inventory")]
    [SerializeField] private List<UsableItemStack> _itemStacks = new List<UsableItemStack>();
    private Dictionary<string, UsableItemStack> _itemLookup = new Dictionary<string, UsableItemStack>();

    [Header("Events")]
    public UnityEvent<UsableItemBase, int> OnItemAdded;
    public UnityEvent<UsableItemBase, int> OnItemRemoved;
    public UnityEvent OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(UsableItemBase item)
    {
        if (item == null || item.GetData() == null)
            return;

        string itemId = item.GetData().ItemId;
        UsableItemData data = item.GetData();

        if (_itemLookup.ContainsKey(itemId))
        {
            UsableItemStack stack = _itemLookup[itemId];

            if (data.IsStackable && stack.Count < data.MaxStacks)
            {
                stack.Count++;
                OnItemAdded?.Invoke(item, stack.Count);
                OnInventoryChanged?.Invoke();
                Destroy(item.gameObject);
            }
        }
        else
        {
            item.transform.SetParent(transform);
            item.gameObject.SetActive(false);

            UsableItemStack newStack = new UsableItemStack(item, 1);
            _itemStacks.Add(newStack);
            _itemLookup[itemId] = newStack;

            OnItemAdded?.Invoke(item, 1);
            OnInventoryChanged?.Invoke();
        }
    }

    public bool RemoveItem(string itemId, int count = 1)
    {
        if (!_itemLookup.ContainsKey(itemId))
            return false;

        UsableItemStack stack = _itemLookup[itemId];

        if (stack.Count < count)
            return false;

        stack.Count -= count;

        OnItemRemoved?.Invoke(stack.ItemInstance, stack.Count);
        OnInventoryChanged?.Invoke();

        if (stack.Count <= 0)
        {
            _itemStacks.Remove(stack);
            _itemLookup.Remove(itemId);

            if (stack.ItemInstance != null)
                Destroy(stack.ItemInstance.gameObject);
        }

        return true;
    }

    public int GetItemCount(string itemId)
    {
        if (_itemLookup.ContainsKey(itemId))
            return _itemLookup[itemId].Count;

        return 0;
    }

    public bool HasItem(string itemId)
    {
        return _itemLookup.ContainsKey(itemId) && _itemLookup[itemId].Count > 0;
    }

    public UsableItemBase GetItem(string itemId)
    {
        if (_itemLookup.ContainsKey(itemId))
            return _itemLookup[itemId].ItemInstance;

        return null;
    }

    public List<UsableItemStack> GetAllItems()
    {
        return new List<UsableItemStack>(_itemStacks);
    }
}
