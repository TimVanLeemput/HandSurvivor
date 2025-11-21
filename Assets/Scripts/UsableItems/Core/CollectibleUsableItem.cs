using HandSurvivor;
using MyBox;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CollectibleUsableItem : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

    [Header("Usable Item Configuration")]
    [SerializeField] private UsableItemData _usableItemData;
    [SerializeField] private GameObject _usableItemPrefab;

    [Header("Pickup Settings")]
    [Tooltip("Automatically pickup on collision (true) or require grab interaction (false)")]
    [SerializeField] private bool _autoPickupOnCollision = true;

    [Header("Which layers can pick up?")]
    [SerializeField] private LayerMask _layers;

    [Header("Which hand can pick up?")]
    [SerializeField] private bool _mainHandCanPickup = true;
    [SerializeField] private bool _offHandCanPickup = true;

    [Header("Visual")]
    [SerializeField] private float _rotationSpeed = 50f;
    [SerializeField] private bool _shouldBobUpAndDown = true;
    [SerializeField] private float _bobSpeed = 2f;
    [SerializeField] private float _bobAmount = 0.2f;
    [SerializeField] private GameObject _visualMesh;

    [Header("Effects")]
    [SerializeField] private GameObject _auraEffectPrefab;
    [SerializeField] private AudioClip _ambientSound;

    [Header("Events")]
    public UnityEvent OnPickedUp;

    private Vector3 _startPosition;
    private float _bobOffset;
    private GameObject _auraEffect;
    private AudioSource _audioSource;
    private bool _hasBeenCollected = false;

    private void Awake()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;

        _startPosition = transform.position;
        _bobOffset = Random.Range(0f, Mathf.PI * 2f);

        if (_auraEffectPrefab != null)
        {
            _auraEffect = Instantiate(_auraEffectPrefab, transform.position, Quaternion.identity, transform);
        }

        if (_ambientSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = _ambientSound;
            _audioSource.loop = true;
            _audioSource.spatialBlend = 1f;
            _audioSource.Play();
        }
    }

    private void Update()
    {
        if (_hasBeenCollected)
            return;

        if (_visualMesh != null)
        {
            _visualMesh.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
        }

        if (_shouldBobUpAndDown)
        {
            float newY = _startPosition.y + Mathf.Sin((Time.time + _bobOffset) * _bobSpeed) * _bobAmount;
            Vector3 newPos = transform.position;
            newPos.y = newY;
            transform.position = newPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
            Debug.Log($"[CollectibleUsableItem] OnTriggerEnter - Object: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}", other.gameObject);

        if (!_autoPickupOnCollision || _hasBeenCollected)
        {
            if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[CollectibleUsableItem] Skipped - autoPickup: {_autoPickupOnCollision}, collected: {_hasBeenCollected}");
            return;
        }

        bool isPlayer = ((1 << other.gameObject.layer) & _layers) != 0;
        if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
            Debug.Log($"[CollectibleUsableItem] Layer check - isPlayer: {isPlayer}, layers mask: {_layers.value}");

        if (!isPlayer)
            return;

        bool isMainHand = HandSelectionManager.IsMainHandObject(other.gameObject);
        bool isOffHand = HandSelectionManager.IsOffHandObject(other.gameObject);

        if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
            Debug.Log($"[CollectibleUsableItem] isMainHand: {isMainHand}, isOffHand: {isOffHand}");

        if ((isMainHand && _mainHandCanPickup) || (isOffHand && _offHandCanPickup))
        {
            if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[CollectibleUsableItem] ✓ Pickup allowed! Calling Pickup()");
            Pickup();
        }
        else
        {
            if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[CollectibleUsableItem] ✗ Pickup denied - hand not allowed");
        }
    }

    [ButtonMethod]
    public void Pickup()
    {
        if (_hasBeenCollected)
            return;

        _hasBeenCollected = true;

        if (_usableItemData == null)
        {
            if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.LogError("[CollectibleUsableItem] No UsableItemData assigned!");
            Destroy(gameObject);
            return;
        }

        if (_usableItemPrefab == null)
        {
            if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.LogError("[CollectibleUsableItem] No UsableItem prefab assigned!");
            Destroy(gameObject);
            return;
        }

        if (UsableItemInventory.Instance == null)
        {
            if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.LogError("[CollectibleUsableItem] No UsableItemInventory found in scene!");
            Destroy(gameObject);
            return;
        }

        GameObject usableItemObj = Instantiate(_usableItemPrefab);
        UsableItemBase usableItem = usableItemObj.GetComponent<UsableItemBase>();

        if (usableItem == null)
        {
            if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.LogError($"[CollectibleUsableItem] UsableItem prefab does not have a UsableItemBase component!");
            Destroy(usableItemObj);
            Destroy(gameObject);
            return;
        }

        usableItem.Pickup();

        OnPickedUp?.Invoke();

        if (_showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
            Debug.Log($"[CollectibleUsableItem] Picked up: {_usableItemData.DisplayName}");

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_auraEffect != null)
            Destroy(_auraEffect);
    }

    private void OnDrawGizmosSelected()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
