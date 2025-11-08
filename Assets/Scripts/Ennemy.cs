using UnityEngine;

public class Ennemy : MonoBehaviour
{
    public int HP = 100;
    public int damage = 10;

    public void DealDamage()
    {
        Nexus.Instance.HP -= damage;
    }
}
