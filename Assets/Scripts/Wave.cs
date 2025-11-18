using System;
using System.Collections.Generic;
using UnityEngine;

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
    public List<EnemyEntry> Enemies = new List<EnemyEntry>();

    [Tooltip("Enemies per second")]
    public float SpawnFequency;

    [Tooltip("End Boss")]
    public GameObject Boss;
}