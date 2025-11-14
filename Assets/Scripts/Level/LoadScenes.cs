using System.Collections.Generic;
using HandSurvivor.Level;
using UnityEngine;

public class LoadScenes : MonoBehaviour
{
    [SerializeField] private List<SceneReference> scenesToLoad = null;
    [SerializeField] private List<SceneReference> scenesToUnload = null;
    [SerializeField] private SceneReference sceneToUnload = null;

    public void CallLoadScenes()
    {
        SceneLoaderManager.Instance.LoadScenes(scenesToLoad);
    }

    public void CallUnloadScene()
    {
        if (sceneToUnload != null)
        {
            SceneLoaderManager.Instance.UnloadScene(sceneToUnload);
        }
    }

    public void CallUnloadScenes()
    {
        SceneLoaderManager.Instance.UnloadScenes(scenesToUnload);
    }
}
