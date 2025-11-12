using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WavesManager : MonoBehaviour
{
    public static WavesManager Instance;
    
    [FormerlySerializedAs("Ennemies")] public EnemyPool enemies;
    [FormerlySerializedAs("EnnemiesSpawnPoints")] public List<Transform> EnemiesSpawnPoints;

    public List<Wave> Waves;

    [FormerlySerializedAs("EnnemiesParent")] public Transform EnemiesParent;

    public List<Enemy> CurrentEnnemies;

    private int _currentWaveIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartWave(Waves[_currentWaveIndex]));
    }

    public IEnumerator StartWave(Wave wave)
    {
        int ennemiesSpawned = 0;
        while (ennemiesSpawned < wave.EnemiesNumber)
        {
            Transform spawnPoint = EnemiesSpawnPoints[Random.Range(0, EnemiesSpawnPoints.Count)];
        
            GameObject go = Instantiate(
                enemies.Enemies[Random.Range(0, enemies.Enemies.Count)],
                spawnPoint.position,
                spawnPoint.rotation,
                EnemiesParent
            );
            
            var graphAgent = go.GetComponent<BehaviorGraphAgent>();
            graphAgent.SetVariableValue("Target", Nexus.Instance.gameObject);
            graphAgent.SetVariableValue("Speed", go.GetComponent<Enemy>().speed);
            
            CurrentEnnemies.Add(go.GetComponent<Enemy>());

            EnemiesNavMeshSyncManager.Instance.SpawnMiniatureEnemy(go);
            
            ennemiesSpawned++;
            yield return new WaitForSeconds(1 / wave.SpawnFequency);
        }
    }
}