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

    public void SpawnMiniatureEnemy(GameObject ennemy)
    {
        GameObject miniature = Instantiate(ennemy, miniatureEnemiesParent);
        Destroy(miniature.GetComponent<NavMeshAgent>());
        Destroy(miniature.GetComponent<BehaviorGraphAgent>());
        Destroy(miniature.GetComponent<EnemyNavMeshSync>());
        EnemyNavMeshSync sync = ennemy.GetComponent<EnemyNavMeshSync>();
        sync.animator = ennemy.GetComponent<Animator>();
        sync.miniatureAnimator = miniature.GetComponent<Animator>();
        sync.miniatureTransform = miniature.transform;
    }
}