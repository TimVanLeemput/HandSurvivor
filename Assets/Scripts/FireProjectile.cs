using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    public GameObject Projectile;
    public Transform ProjectileSpawn;
    
    public void Fire()
    {
        GameObject go = Instantiate(Projectile, ProjectileSpawn.position, ProjectileSpawn.rotation);
        go.GetComponent<Projectile>().Damage = GetComponent<Enemy>().damage;
    }
}
