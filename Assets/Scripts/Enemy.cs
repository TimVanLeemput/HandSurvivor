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

        while (!CheckDestinationReached(_nexusTransform.position))
        {
            TargetNexus();
            yield return new WaitForSeconds(1);
        }

        _animator.SetTrigger("Attack");
        gameObject.GetComponent<EnemyNavMeshSync>().Attack();

        while (true)
        {
            transform.LookAt(_nexusTransform);
            yield return new WaitForSeconds(1);
        }
    }

    private void TargetNexus()
    {
        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            Vector3 targetPos = GetDestinationNearNexus();
            _navMeshAgent.SetDestination(targetPos);
        }
    }

    private Vector3 GetDestinationNearNexus()
    {
        Vector3 nexusPos = _nexusTransform.position;
        Vector3 fromNexusToEnemy = transform.position - nexusPos;

        // Si l'ennemi est exactement au centre, choisis une direction arbitraire
        if (fromNexusToEnemy.sqrMagnitude < 0.001f)
        {
            fromNexusToEnemy = Vector3.forward;
        }

        Vector3 dir = fromNexusToEnemy.normalized;

        // Point sur le cercle autour du nexus, à distance destinationReachedDistance
        Vector3 targetOnCircle = nexusPos + dir * (destinationReachedDistance / 2f);

        return targetOnCircle;
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
        Destroy(gameObject);
    }

    public void DropXP()
    {
        XPManager.Instance.DropXP(XPAmount, transform.position);
    }

    bool CheckDestinationReached(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) < destinationReachedDistance;
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