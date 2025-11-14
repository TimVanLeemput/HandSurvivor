using System.Collections.Generic;
using HandSurvivor.Level;
using UnityEngine;

public class LoadScenes : MonoBehaviour
{
    [SerializeField] private List<SceneReference> scenesToLoad = null;

    public void CallLoadScenes()
    {
        SceneLoaderManager.Instance.LoadScenes(scenesToLoad);
    }
}
