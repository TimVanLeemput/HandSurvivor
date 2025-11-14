using HandSurvivor.Level;
using UnityEngine;

public class MainMenuStartButton : MonoBehaviour
{
    [SerializeField] private SceneReference sceneToLoad = null;

    public void LoadScene()
    {
        SceneLoaderManager.Instance.LoadScene(sceneToLoad);
    }
}
