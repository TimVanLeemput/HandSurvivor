using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnnemyPool", menuName = "EnnemyPool")]
public class EnnemyPool : ScriptableObject
{
    public List<GameObject> Ennemies;
}
