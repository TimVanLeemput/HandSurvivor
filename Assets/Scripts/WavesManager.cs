using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class WavesManager : MonoBehaviour
{
    public EnnemyPool Ennemies;
    public List<Transform> EnnemiesSpawnPoints;

    public List<Wave> Waves;

    public Transform EnnemiesParent;

    private int _currentWaveIndex = 0;

    private void Start()
    {
        StartCoroutine(StartWave(Waves[_currentWaveIndex]));
    }

    public IEnumerator StartWave(Wave wave)
    {
        int ennemiesSpawned = 0;
        while (ennemiesSpawned < wave.EnnemiesNumber)
        {
            Transform spawnPoint = EnnemiesSpawnPoints[Random.Range(0, EnnemiesSpawnPoints.Count)];
        
            GameObject go = Instantiate(
                Ennemies.Ennemies[Random.Range(0, Ennemies.Ennemies.Count)],
                spawnPoint.position,
                spawnPoint.rotation,
                EnnemiesParent
            );
            
            var graphAgent = go.GetComponent<BehaviorGraphAgent>();
            graphAgent.SetVariableValue("Target", Nexus.Instance.gameObject);
            graphAgent.SetVariableValue("Speed", go.GetComponent<Ennemy>().speed);
            

            ennemiesSpawned++;
            yield return new WaitForSeconds(1 / wave.SpawnFequency);
        }
    }
}