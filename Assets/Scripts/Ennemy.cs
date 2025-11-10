using UnityEngine;

public class Ennemy : MonoBehaviour
{
    public int HP = 100;
    public int damage = 10;
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
        }
    }
}