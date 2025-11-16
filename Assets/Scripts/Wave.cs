using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Wave", menuName = "Wave")]
public class Wave : ScriptableObject
{
    [Tooltip("Seconds")]
    public float WaveDuration = 60f;

    [Serializable]
    public class EnemyEntry
    {
        public GameObject Enemy;
        [Range(0f, 1f)]
        public float Probability = 1f;
    }

    [Tooltip("Enemies and probability of spawning them")]
    public List<EnemyEntry> EnemyEntries = new List<EnemyEntry>();

    [NonSerialized]
    public Dictionary<GameObject, float> Enemies = new Dictionary<GameObject, float>();

    [Tooltip("Enemies per second")]
    public float SpawnFequency;

    [Tooltip("End Boss")]
    public GameObject Boss;

    private void OnValidate()
    {
        Enemies.Clear();
        foreach (var entry in EnemyEntries)
        {
            if (entry != null && entry.Enemy != null)
            {
                Enemies[entry.Enemy] = entry.Probability;
            }
        }
    }
}