using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WavesManager : MonoBehaviour
{
    public static WavesManager Instance;

    [FormerlySerializedAs("EnnemiesSpawnPoints")]
    public List<Transform> EnemiesSpawnPoints;

    public Level Level;

    public Transform EnemiesParent;

    public List<Enemy> CurrentEnnemies;

    private int _currentWaveIndex = 0;
    private GameObject _lastBoss;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartWave(Level.Waves[_currentWaveIndex]));
    }

    public IEnumerator StartWave(Wave wave)
    {
        float waveTimer = 0;
        float spawnInterval = 1f / wave.SpawnFequency;

        while (waveTimer < wave.WaveDuration)
        {
            Transform spawnPoint = EnemiesSpawnPoints[Random.Range(0, EnemiesSpawnPoints.Count)];

            GameObject go = Instantiate(
                PickRandomEnemy(wave.Enemies),
                spawnPoint.position,
                spawnPoint.rotation,
                EnemiesParent
            );

            SetLayerRecursively(go, LayerMask.NameToLayer("Invisible"));

            GameObject miniatureGo = EnemiesNavMeshSyncManager.Instance.SpawnMiniatureEnemy(go);
            CurrentEnnemies.Add(miniatureGo.GetComponent<Enemy>());
            SetLayerRecursively(miniatureGo, LayerMask.NameToLayer("Enemy"));

            waveTimer += spawnInterval;
            yield return new WaitForSeconds(spawnInterval);
        }

        if (wave.Boss != null)
        {
            Transform spawnPoint = EnemiesSpawnPoints[Random.Range(0, EnemiesSpawnPoints.Count)];

            GameObject go = Instantiate(
                wave.Boss,
                spawnPoint.position,
                spawnPoint.rotation,
                EnemiesParent
            );

            SetLayerRecursively(go, LayerMask.NameToLayer("Invisible"));

            GameObject miniatureGo = EnemiesNavMeshSyncManager.Instance.SpawnMiniatureEnemy(go);
            CurrentEnnemies.Add(miniatureGo.GetComponent<Enemy>());
            SetLayerRecursively(miniatureGo, LayerMask.NameToLayer("Enemy"));
            if (_currentWaveIndex == Level.Waves.Count)
                _lastBoss = go;
        }

        if (_currentWaveIndex == Level.Waves.Count)
        {
            // NO MORE WAVES
            while (_lastBoss != null)
            {
                yield return null;
            }
            // LAST BOSS DEAD: VICTORY
            // TODO: handle it
            Debug.Log("VICTORY");
        }
        else
        {
            _currentWaveIndex++;
            StartCoroutine(StartWave(Level.Waves[_currentWaveIndex]));
        }
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;

        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public static GameObject PickRandomEnemy(Dictionary<GameObject, float> enemies)
    {
        // Tirage aléatoire entre 0 et 1
        float r = Random.value;
        float cumulative = 0f;

        foreach (var kvp in enemies)
        {
            cumulative += kvp.Value; // kvp.Value = probabilité de cet ennemi

            if (r <= cumulative)
            {
                return kvp.Key;
            }
        }

        // Sécurité (au cas où à cause des flottants)
        foreach (var kvp in enemies)
            return kvp.Key;

        return null;
    }
}