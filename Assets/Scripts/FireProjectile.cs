using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    public GameObject Projectile;
    public Transform ProjectileSpawn;
    
    public void Fire()
    {
        if (GetComponent<InvisibleEnemyRef>() == null)
            return;
        GameObject go = Instantiate(Projectile, ProjectileSpawn);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
            
        go.GetComponent<Projectile>().Damage = GetComponent<Enemy>().damage;
    }
}
