using System;
using UnityEngine;
using UnityEngine.Events;

public class Nexus : MonoBehaviour
{
    public static Nexus Instance;

    [Header("Health")]
    public int HP = 1000;
    public int MaxHP = 1000;

    [Header("Defense")]
    [SerializeField] private int _flatArmor = 0;
    [SerializeField] private float _percentageArmor = 0f;

    [Header("Shield")]
    [SerializeField] private bool _isShieldActive = false;
    [SerializeField] private float _shieldEndTime = 0f;
    [SerializeField] private GameObject _currentShieldVFX;

    [Header("UI")]
    [SerializeField] private NexusLifeDisplay _lifeDisplay;

    [Header("Events")]
    public UnityEvent<int> OnDamageTaken;
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent<float> OnShieldActivated;
    public UnityEvent OnShieldDeactivated;
    public UnityEvent OnNexusDestroyed;

    private int _currentHP;

    private void Awake()
    {
        Instance = this;
        _currentHP = HP;
    }

    private void Start()
    {
        if (_lifeDisplay != null)
            _lifeDisplay.UpdateDisplay(_currentHP, MaxHP);
    }

    private void Update()
    {
        if (_currentHP != HP)
        {
            _currentHP = HP;
            if (_lifeDisplay != null)
                _lifeDisplay.UpdateDisplay(_currentHP, MaxHP);
        }

        if (_isShieldActive && Time.time >= _shieldEndTime)
        {
            DeactivateShield();
        }
    }

    public void TakeDamage(int rawDamage)
    {
        if (_isShieldActive)
            return;

        int damage = CalculateDamageAfterArmor(rawDamage);

        HP -= damage;
        HP = Mathf.Max(HP, 0);

        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(HP, MaxHP);

        if (HP <= 0)
        {
            OnNexusDestroyed?.Invoke();
        }
    }

    private int CalculateDamageAfterArmor(int rawDamage)
    {
        float damage = rawDamage;

        damage -= _flatArmor;
        damage = Mathf.Max(damage, 0);

        damage *= (1f - _percentageArmor);

        return Mathf.CeilToInt(damage);
    }

    public void ApplyShield(float duration, GameObject vfxPrefab = null)
    {
        _isShieldActive = true;
        _shieldEndTime = Time.time + duration;

        if (vfxPrefab != null)
        {
            if (_currentShieldVFX != null)
                Destroy(_currentShieldVFX);

            _currentShieldVFX = Instantiate(vfxPrefab, transform.position, Quaternion.identity, transform);
        }

        OnShieldActivated?.Invoke(duration);
    }

    private void DeactivateShield()
    {
        _isShieldActive = false;

        if (_currentShieldVFX != null)
        {
            Destroy(_currentShieldVFX);
            _currentShieldVFX = null;
        }

        OnShieldDeactivated?.Invoke();
    }

    public void AddArmor(int flat, float percentage)
    {
        _flatArmor += flat;
        _percentageArmor = Mathf.Clamp01(_percentageArmor + percentage);
    }

    public void RestoreHealth(int amount)
    {
        HP += amount;
        HP = Mathf.Min(HP, MaxHP);
        OnHealthChanged?.Invoke(HP, MaxHP);
    }

    public void RestoreHealthPercentage(float percentage)
    {
        int amount = Mathf.CeilToInt(MaxHP * percentage);
        RestoreHealth(amount);
    }

    public bool IsShieldActive() => _isShieldActive;
    public int GetFlatArmor() => _flatArmor;
    public float GetPercentageArmor() => _percentageArmor;
}