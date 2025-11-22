using UnityEngine;

public class NexusMiniatureRef : MonoBehaviour
{
    public static NexusMiniatureRef Instance;

    private void Awake()
    {
        Instance = this;
    }
}