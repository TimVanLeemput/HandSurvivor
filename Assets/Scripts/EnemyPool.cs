using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPool", menuName = "EnemyPool")]
public class EnemyPool : ScriptableObject
{
    public List<GameObject> Enemies;
}
