using System.Collections;
using UnityEngine;

public class Ennemy : MonoBehaviour
{
    public int HP = 100;
    public int damage = 10;
    public float speed = 10;
    public bool canTakeDamage = true;

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

    public void Die()
    {
        StartCoroutine(DeathCoroutine());
    }

    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}