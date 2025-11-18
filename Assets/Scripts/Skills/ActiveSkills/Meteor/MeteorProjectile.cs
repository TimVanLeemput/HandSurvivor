using System;
using UnityEngine;
using Oculus.Interaction.HandGrab;
using HandSurvivor.Interfaces;

public class MeteorProjectile : MonoBehaviour, IGravityScalable
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [Header("Damage Settings")]
    private int damage;
    [SerializeField] private float damageRadius = 3f;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float minImpactVelocity = 2f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem smokeTrail;
    [SerializeField] private GameObject craterParticlePrefab;
    [SerializeField] private float craterDuration = 3f;
    [SerializeField] private float meteorDestroyDelay = 1f;

    [Header("Physics")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float gravityMultiplier = 100f;

    public float GravityMultiplier => gravityMultiplier;

    private Rigidbody _rigidbody;
    private Collider _collider;
    private bool _hasExploded = false;
    private bool _canCollide = false;
    private float _spawnTime;
    private const float COLLISION_DELAY = 0.5f;
    private OVRGrabbable _grabbable;
    private Vector3 _lastPosition;
    private Vector3 _throwVelocity;
    private Action _onDestroyed;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _grabbable = GetComponent<OVRGrabbable>();

        if (_rigidbody == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogError)

                Debug.LogError("MeteorProjectile requires a Rigidbody component!");
        }

        if (_collider == null)
        {
            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableDebugLogError)

                Debug.LogError("MeteorProjectile requires a Collider component!");
        }
    }

    private void Start()
    {
        _lastPosition = transform.position;
        _spawnTime = Time.time;
    }

    private void Update()
    {
        // Enable collision after delay
        if (!_canCollide && Time.time >= _spawnTime + COLLISION_DELAY)
        {
            _canCollide = true;
        }

        // Track velocity manually for more accurate throw detection
        if (_grabbable != null && _grabbable.isGrabbed)
        {
            _throwVelocity = (transform.position - _lastPosition) / Time.deltaTime;
        }
        _lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        ApplyScaledGravity();
    }

    public void ApplyScaledGravity()
    {
        // Apply additional gravity to compensate for reduced world gravity
        // Only apply when not grabbed
        if (_rigidbody != null && !_hasExploded && (_grabbable == null || !_grabbable.isGrabbed))
        {
            _rigidbody.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasExploded || !_canCollide)
            return;

        // Check if impact velocity is significant enough to trigger explosion
        float impactVelocity = collision.relativeVelocity.magnitude;

        if (impactVelocity >= minImpactVelocity)
        {
            Vector3 impactPoint = collision.contacts[0].point;
            Explode(impactPoint);
        }
    }

    private void Explode(Vector3 impactPoint)
    {
        _hasExploded = true;

        // Disable physics and grabbing
        if (_collider != null)
            _collider.enabled = false;

        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        // Stop smoke trail
        if (smokeTrail != null)
        {
            smokeTrail.Stop();
        }

        // Deal area damage
        DealAreaDamage(impactPoint);

        // Spawn crater particle effect
        if (craterParticlePrefab != null)
        {
            GameObject crater = Instantiate(craterParticlePrefab, impactPoint, Quaternion.identity);
            Destroy(crater, craterDuration);
        }

        // Notify that meteor is being destroyed
        _onDestroyed?.Invoke();

        // Destroy meteor after delay
        Destroy(gameObject, meteorDestroyDelay);
    }

    private void DealAreaDamage(Vector3 center)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, damageRadius, enemyLayer);

        foreach (Collider col in hitColliders)
        {
            Enemy enemy = col.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // Apply explosion force to ragdoll if available
                RagdollController ragdoll = enemy.GetComponent<RagdollController>();
                if (ragdoll != null)
                {
                    ragdoll.SetRagdoll(true);

                    // Apply force to all ragdoll rigidbodies
                    Rigidbody[] ragdollRigidbodies = enemy.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rb in ragdollRigidbodies)
                    {
                        rb.AddExplosionForce(explosionForce, center, damageRadius);
                    }
                }
            }
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetDamageRadius(float newRadius)
    {
        damageRadius = newRadius;
    }

    public void SetOnDestroyedCallback(Action callback)
    {
        _onDestroyed = callback;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize damage radius in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
