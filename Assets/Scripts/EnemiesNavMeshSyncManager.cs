using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class EnemiesNavMeshSyncManager : MonoBehaviour
{
    public static EnemiesNavMeshSyncManager Instance;
    
    public Transform miniatureEnemiesParent;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject SpawnMiniatureEnemy(GameObject enemy)
    {
        GameObject miniature = Instantiate(enemy, miniatureEnemiesParent);
        Destroy(miniature.GetComponent<NavMeshAgent>());
        Destroy(miniature.GetComponent<EnemyNavMeshSync>());
        EnemyNavMeshSync sync = enemy.GetComponent<EnemyNavMeshSync>();
        sync.animator = enemy.GetComponent<Animator>();
        sync.miniatureAnimator = miniature.GetComponent<Animator>();
        sync.miniatureTransform = miniature.transform;
        InvisibleEnemyRef invisibleEnemyRef = miniature.AddComponent<InvisibleEnemyRef>();
        invisibleEnemyRef.Ref = enemy.GetComponent<Enemy>();

        return miniature;
    }
}