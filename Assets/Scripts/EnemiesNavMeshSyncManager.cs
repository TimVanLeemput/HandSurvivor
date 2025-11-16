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
        RemoveAllPhysicsComponents(enemy);
        if (enemy.GetComponentInChildren<RagdollController>() != null)
            Destroy(enemy.GetComponentInChildren<RagdollController>());
        if(enemy.GetComponentInChildren<RagdollController_Medium>() != null)
            Destroy(enemy.GetComponentInChildren<RagdollController_Medium>());

        return miniature;
    }
    
    public static void RemoveAllPhysicsComponents(GameObject root)
    {
        if (root == null) return;

        var colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (var col in colliders)
        {
            Destroy(col);
        }
        
        var characterJoints = root.GetComponentsInChildren<CharacterJoint>(includeInactive: true);
        foreach (var characterJoint in characterJoints)
        {
            Destroy(characterJoint);
        }

        var rigidbodies = root.GetComponentsInChildren<Rigidbody>(includeInactive: true);
        foreach (var rb in rigidbodies)
        {
            Destroy(rb);
        }
    }
}