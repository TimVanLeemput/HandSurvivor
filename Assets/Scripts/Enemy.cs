using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int HP = 100;
    public int damage = 10;
    public int XPAmount = 10;
    public bool canTakeDamage = true;
    public bool isTargeted = false;
    public float destinationReachedDistance = 3;

    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Transform _nexusTransform;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _nexusTransform = Nexus.Instance.transform;
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
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
        Nexus.Instance.HP -= damage;
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
        GetComponent<NavMeshAgent>().isStopped = true;
        if (dropXP)
            XPManager.Instance.DropXP(XPAmount, transform.position);
        StartCoroutine(DeathCoroutine());
    }

    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(2);
        List<SkinnedMeshRenderer> smrs = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
        float t = 0;
        float dissovleDuration = 1;
        while (t < dissovleDuration)
        {
            foreach (SkinnedMeshRenderer smr in smrs)
            {
                smr.material.SetFloat("_Dissolve", t);
            }

            t += Time.deltaTime;
            yield return null;
        }

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
}