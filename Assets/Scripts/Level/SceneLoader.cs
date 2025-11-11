using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using HandSurvivor;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private bool loadOnPlaceLocked = true;
    [SerializeField] private float loadDelay = 0.5f;


    [Header("Events")]
    public UnityEvent<string> OnSceneLoaded;

    private bool isSceneLoaded = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnPlaneLocked(Transform plane)
    {
        if (loadOnPlaceLocked && !string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneWithDelay());
        }
    }

    private IEnumerator LoadSceneWithDelay()
    {
        if (loadDelay > 0)
        {
            yield return new WaitForSeconds(loadDelay);
        }

        LoadScene();
    }

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[SceneLoader] No scene to load!");
            return;
        }

        if (isSceneLoaded)
        {
            Debug.LogWarning("[SceneLoader] Scene already loaded!");
            return;
        }

        StartCoroutine(LoadSceneAsync());
    }
    
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] No scene to load!");
            return;
        }

        if (isSceneLoaded)
        {
            Debug.LogWarning("[SceneLoader] Scene already loaded!");
            return;
        }

        StartCoroutine(LoadSceneAsync());
    }

    public void SetSceneToLoad(string sceneName, bool loadImmediately = false)
    {
        sceneToLoad = sceneName;

        if (loadImmediately && !isSceneLoaded)
        {
            LoadScene();
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isSceneLoaded = true;
        OnSceneLoaded?.Invoke(sceneToLoad);
        Debug.Log($"[SceneLoader] Scene loaded: {sceneToLoad}");
    }

    public void UnloadScene()
    {
        if (!isSceneLoaded || string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneLoader] No scene to unload!");
            return;
        }

        SceneManager.UnloadSceneAsync(sceneToLoad);
        isSceneLoaded = false;
        Debug.Log($"[SceneLoader] Scene unloaded: {sceneToLoad}");
    }
}
