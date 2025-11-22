using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using HandSurvivor.Stats;
using MyBox;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    private static readonly int DissolvePropertyID = Shader.PropertyToID("_Dissolve");
    public EnemyType MyEnemyType;
    public int HP = 100;
    public int damage = 10;
    public int XPAmount = 10;
    public bool canTakeDamage = true;
    public bool isTargeted = false;
    public float destinationReachedDistance = 3;

    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Transform _nexusTransform;
    private Rigidbody _rigidbody;
    private Collider _collider;

    // Reserved ring slot info (managed by NexusRingPositionsManager)
    private int _ringSlotIndex = -1;
    private Vector3 _assignedDestination;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _nexusTransform = Nexus.Instance.transform;
    }

    private void Start()
    {
        if (_navMeshAgent != null)
            StartCoroutine(TargetNexusCoroutine());
    }

    private IEnumerator TargetNexusCoroutine()
    {
        while (_navMeshAgent == null || !_navMeshAgent.isOnNavMesh)
        {
            yield return null;
        }

        // Try to acquire a free spot on the circle around the Nexus
        Vector3 targetPos = _nexusTransform.position;
        bool acquired = false;
        if (NexusRingPositionsManager.Instance != null)
        {
            if (NexusRingPositionsManager.Instance.AcquireNearestPosition(transform.position, destinationReachedDistance, out _ringSlotIndex, out _assignedDestination))
            {
                targetPos = _assignedDestination;
                acquired = true;
            }
        }

        _navMeshAgent.stoppingDistance = 0;
        _navMeshAgent.SetDestination(targetPos);

        while (!CheckDestinationReached(targetPos))
        {
            yield return null;
        }

        _navMeshAgent.enabled = false;
        _animator.SetTrigger("Attack");
        gameObject.GetComponent<EnemyNavMeshSync>().Attack();
        transform.LookAt(_nexusTransform);
    }

    public void DealDamage()
    {
        Nexus.Instance.TakeDamage(damage);

        // Track nexus damage for stats
        if (PlayerStatsManager.Instance != null)
            PlayerStatsManager.Instance.RecordNexusDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP < 1 && canTakeDamage)
        {
            _animator.SetTrigger("Death");
            HP = 0;
            canTakeDamage = false;
            Die();
        }
    }

    public void Die(bool dropXP = true)
    {
        if (dropXP)
            XPManager.Instance.DropXP(XPAmount, transform.position);

        // Track kill for stats
        if (PlayerStatsManager.Instance != null)
            PlayerStatsManager.Instance.RecordKill(gameObject.name, XPAmount);

        StartCoroutine(DeathCoroutine());
    }

    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(2);
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        float t = 0;
        float dissovleDuration = 1;
        while (t < dissovleDuration)
        {
            for (int i = 0; i < smrs.Length; i++)
            {
                smrs[i].material.SetFloat(DissolvePropertyID, t);
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (GetComponent<InvisibleEnemyRef>().Ref != null)
            Destroy(GetComponent<InvisibleEnemyRef>().Ref.gameObject);
        WavesManager.Instance.CurrentEnnemies.Remove(this);

        // Release reserved ring slot before destroying
        ReleaseReservedRingSlot();

        Destroy(gameObject);
    }

    public void DropXP()
    {
        XPManager.Instance.DropXP(XPAmount, transform.position);
    }

    private void ReleaseReservedRingSlot()
    {
        if (_ringSlotIndex >= 0 && NexusRingPositionsManager.Instance != null)
        {
            NexusRingPositionsManager.Instance.ReleasePosition(_ringSlotIndex);
            _ringSlotIndex = -1;
        }
    }

    private void OnDestroy()
    {
        // Safety: ensure slot is freed if object is destroyed unexpectedly
        ReleaseReservedRingSlot();
    }

    bool CheckDestinationReached(Vector3 target)
    {
        Vector3 pos = transform.position;
        pos.y = target.y;
        return Vector3.Distance(pos, target) < 1;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (MyEnemyType != EnemyType.small) return;

        if (GetComponent<InvisibleEnemyRef>().Ref != null)
            Destroy(GetComponent<InvisibleEnemyRef>().Ref.gameObject);

        _rigidbody.isKinematic = false;
        
        float _multiplier = 0.5f;

        Vector3 force = new Vector3(Random.Range(-1f,1f), Random.Range(-0.5f,0.5f), Random.Range(-1f,1f));
        _rigidbody.AddForce(force * _multiplier, ForceMode.Impulse);
        _collider.enabled = false;
        _animator.SetTrigger("Floating");
        Die();
    }

    public void OnGrab()
    {
        XPManager.Instance.DropXP(XPAmount, transform.position);
        if (MyEnemyType != EnemyType.medium) return;

        if (GetComponent<InvisibleEnemyRef>().Ref != null)
            Destroy(GetComponent<InvisibleEnemyRef>().Ref.gameObject);

        _rigidbody.isKinematic = false;
        _animator.SetTrigger("Hanging");
    }
    
    public void OnRelease()
    {
        Die(false);
    }

    public enum EnemyType
    {
        small,
        medium,
        large,
        flying
    }
}