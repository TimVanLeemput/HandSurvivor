using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UsableItemSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _countText;

    [Header("Visual Settings")]
    [SerializeField] private bool _showNameText = true;
    [SerializeField] private bool _showCountText = true;

    public void Initialize(Sprite icon, string itemName, int count)
    {
        if (_itemIcon != null && icon != null)
            _itemIcon.sprite = icon;

        if (_itemNameText != null && _showNameText)
        {
            _itemNameText.text = itemName;
            _itemNameText.gameObject.SetActive(true);
        }
        else if (_itemNameText != null)
        {
            _itemNameText.gameObject.SetActive(false);
        }

        UpdateCount(count);
    }

    public void UpdateCount(int count)
    {
        if (_countText != null && _showCountText)
        {
            _countText.text = count.ToString();
            _countText.gameObject.SetActive(true);
        }
        else if (_countText != null)
        {
            _countText.gameObject.SetActive(false);
        }
    }
}
