using System.Collections.Generic;
using UnityEngine;

public class EnemyNavMeshSyncManager : MonoBehaviour
{
    private static EnemyNavMeshSyncManager instance;
    private List<EnemyNavMeshSync> enemies = new List<EnemyNavMeshSync>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public static void Register(EnemyNavMeshSync enemy)
    {
        if (instance == null)
        {
            GameObject managerObj = new GameObject("EnemyNavMeshSyncManager");
            instance = managerObj.AddComponent<EnemyNavMeshSyncManager>();
        }
        instance.enemies.Add(enemy);
    }

    public static void Unregister(EnemyNavMeshSync enemy)
    {
        if (instance != null)
        {
            instance.enemies.Remove(enemy);
        }
    }

    private void Update()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyNavMeshSync enemy = enemies[i];
            if (enemy != null && enemy.enabled)
            {
                enemy.SyncTransforms();
            }
        }
    }
}
