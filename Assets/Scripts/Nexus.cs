using System;
using UnityEngine;

public class Nexus : MonoBehaviour
{
    public static Nexus Instance;
    public int HP = 1000;

    private void Awake()
    {
        Instance = this;
    }
}