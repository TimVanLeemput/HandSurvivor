using UnityEngine;

[CreateAssetMenu(fileName = "UsableItem_", menuName = "HandSurvivor/Usable Item Data")]
public class UsableItemData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _itemId;
    [SerializeField] private string _displayName;
    [TextArea(3, 6)]
    [SerializeField] private string _description;
    [SerializeField] private Sprite _itemImage;

    [Header("Stack Settings")]
    [SerializeField] private bool _isStackable = true;
    [SerializeField] private int _maxStacks = 99;

    [Header("Audio")]
    [SerializeField] private AudioClip _useSound;

    public string ItemId => _itemId;
    public string DisplayName => _displayName;
    public string Description => _description;
    public Sprite ItemImage => _itemImage;
    public bool IsStackable => _isStackable;
    public int MaxStacks => _maxStacks;
    public AudioClip UseSound => _useSound;
}
