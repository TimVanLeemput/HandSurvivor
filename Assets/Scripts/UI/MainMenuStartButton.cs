using System.Collections.Generic;
using HandSurvivor.Level;
using UnityEngine;

public class MainMenuStartButton : MonoBehaviour
{
    [SerializeField] private List<SceneReference> scenesToLoad = null;

    public void LoadScenes()
    {
        SceneLoaderManager.Instance.LoadScenes(scenesToLoad);
    }
}
