using UnityEngine;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Spawns VFX at surface contact points
    /// </summary>
    public class SpawnGameObject : MonoBehaviour
    {
        [SerializeField] private GameObject prefabToSpawn;
        [SerializeField] private float prefabLifeTime = 2f;

        /// <summary>
        /// Spawn VFX at the hit point
        /// Call this from LaserBeam's onSurfaceContact event
        /// </summary>
        public void SpawnAtRayCastHitPosition(RaycastHit hit)
        {
            if (prefabToSpawn == null) return;

            GameObject vfx = Instantiate(prefabToSpawn, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(vfx, prefabLifeTime);
        }

        public void Spawn()
        {
            Instantiate(prefabToSpawn);
        }
    }
}
