using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Wave")]
public class Wave : ScriptableObject
{
    public int EnnemiesNumber = 50;
    [Tooltip("Per second")]
    public float SpawnFequency;
}
