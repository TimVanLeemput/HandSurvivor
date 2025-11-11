using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Random = UnityEngine.Random;

public class WavesManager : MonoBehaviour
{
    public static WavesManager Instance;
    
    public EnnemyPool Ennemies;
    public List<Transform> EnnemiesSpawnPoints;

    public List<Wave> Waves;

    public Transform EnnemiesParent;

    public List<Ennemy> CurrentEnnemies;

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
            
            CurrentEnnemies.Add(go.GetComponent<Ennemy>());

            ennemiesSpawned++;
            yield return new WaitForSeconds(1 / wave.SpawnFequency);
        }
    }
}