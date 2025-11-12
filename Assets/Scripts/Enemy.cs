using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int HP = 100;
    public int damage = 10;
    public float speed = 10;
    public int XPAmount = 10;
    public bool canTakeDamage = true;
    public bool isTargeted = false;

    public Animator deathAnimator;

    public void DealDamage()
    {
        Nexus.Instance.HP -= damage;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP < 1 && canTakeDamage)
        {
            deathAnimator.SetTrigger("Death");
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
}