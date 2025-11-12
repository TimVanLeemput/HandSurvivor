using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Wave", menuName = "Wave")]
public class Wave : ScriptableObject
{
    [FormerlySerializedAs("EnnemiesNumber")] public int EnemiesNumber = 50;
    [Tooltip("Per second")]
    public float SpawnFequency;
}
