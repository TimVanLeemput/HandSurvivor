using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UsableItemInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _itemSlotPrefab;
    [SerializeField] private Transform _itemSlotContainer;

    [Header("Settings")]
    [SerializeField] private bool _updateAutomatically = true;

    private Dictionary<string, UsableItemSlotUI> _activeSlots = new Dictionary<string, UsableItemSlotUI>();

    private void Start()
    {
        if (UsableItemInventory.Instance != null)
        {
            UsableItemInventory.Instance.OnInventoryChanged.AddListener(RefreshUI);
            RefreshUI();
        }
    }

    private void OnDestroy()
    {
        if (UsableItemInventory.Instance != null)
        {
            UsableItemInventory.Instance.OnInventoryChanged.RemoveListener(RefreshUI);
        }
    }

    public void RefreshUI()
    {
        if (!_updateAutomatically)
            return;

        if (UsableItemInventory.Instance == null)
            return;

        List<UsableItemInventory.UsableItemStack> allItems = UsableItemInventory.Instance.GetAllItems();

        ClearOldSlots(allItems);

        foreach (UsableItemInventory.UsableItemStack stack in allItems)
        {
            string itemId = stack.ItemInstance.GetData().ItemId;

            if (_activeSlots.ContainsKey(itemId))
            {
                _activeSlots[itemId].UpdateCount(stack.Count);
            }
            else
            {
                CreateItemSlot(stack);
            }
        }
    }

    private void ClearOldSlots(List<UsableItemInventory.UsableItemStack> currentItems)
    {
        List<string> itemsToRemove = new List<string>();

        foreach (string itemId in _activeSlots.Keys)
        {
            bool stillExists = false;
            foreach (UsableItemInventory.UsableItemStack stack in currentItems)
            {
                if (stack.ItemInstance.GetData().ItemId == itemId)
                {
                    stillExists = true;
                    break;
                }
            }

            if (!stillExists)
            {
                itemsToRemove.Add(itemId);
            }
        }

        foreach (string itemId in itemsToRemove)
        {
            if (_activeSlots.ContainsKey(itemId))
            {
                Destroy(_activeSlots[itemId].gameObject);
                _activeSlots.Remove(itemId);
            }
        }
    }

    private void CreateItemSlot(UsableItemInventory.UsableItemStack stack)
    {
        if (_itemSlotPrefab == null || _itemSlotContainer == null)
        {
            Debug.LogError("[UsableItemInventoryUI] ItemSlotPrefab or Container not assigned!");
            return;
        }

        GameObject slotObj = Instantiate(_itemSlotPrefab, _itemSlotContainer);
        UsableItemSlotUI slotUI = slotObj.GetComponent<UsableItemSlotUI>();

        if (slotUI != null)
        {
            UsableItemData data = stack.ItemInstance.GetData();
            slotUI.Initialize(data.ItemImage, data.DisplayName, stack.Count);
            _activeSlots[data.ItemId] = slotUI;
        }
        else
        {
            Debug.LogError("[UsableItemInventoryUI] ItemSlotPrefab missing UsableItemSlotUI component!");
            Destroy(slotObj);
        }
    }
}
