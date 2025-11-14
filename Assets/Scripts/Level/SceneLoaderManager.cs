using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HandSurvivor.Level
{
    /// <summary>
    /// Manages sequential additive scene loading with fade transitions and optional loading screen
    /// </summary>
    public class SceneLoaderManager : MonoBehaviour
    {
        public static SceneLoaderManager Instance { get; private set; }

        [Header("Scene Loading Configuration")]
        [SerializeField] private List<SceneReference> scenesToLoad = new List<SceneReference>();

        [Header("Loading Screen (Optional)")]
        [SerializeField] private SceneReference loadingScreenScene;
        [SerializeField] private bool useLoadingScreen = false;

        [Header("Fade Settings")]
        [SerializeField] private bool useFade = true;
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("Auto Load on Start")]
        [SerializeField] private bool loadScenesOnStart = true;

        [Header("Events")]
        public UnityEvent<float> OnLoadingProgress;
        public UnityEvent OnLoadingComplete;

        private List<string> loadedScenes = new List<string>();
        private bool isLoading = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Ensure this is on a root GameObject
            if (transform.parent != null)
            {
                Debug.LogWarning("SceneLoaderManager: Component must be on a root GameObject for DontDestroyOnLoad to work. Reparenting...");
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (loadScenesOnStart)
            {
                LoadScenes();
            }
        }

        /// <summary>
        /// Begin loading all configured scenes sequentially
        /// </summary>
        public void LoadScenes()
        {
            if (isLoading)
            {
                Debug.LogWarning("SceneLoaderManager: Already loading scenes");
                return;
            }

            if (scenesToLoad == null || scenesToLoad.Count == 0)
            {
                Debug.LogWarning("SceneLoaderManager: No scenes configured to load");
                return;
            }

            StartCoroutine(LoadScenesSequence(scenesToLoad));
        }

        /// <summary>
        /// Load a single scene additively with fade transition
        /// </summary>
        public void LoadScene(SceneReference sceneToLoad)
        {
            if (sceneToLoad == null || !sceneToLoad.IsValid)
            {
                Debug.LogError("SceneLoaderManager: Invalid scene reference provided");
                return;
            }

            LoadScenes(new List<SceneReference> { sceneToLoad });
        }

        /// <summary>
        /// Load multiple scenes sequentially with fade transition
        /// </summary>
        public void LoadScenes(List<SceneReference> scenesToLoadOverride)
        {
            if (isLoading)
            {
                Debug.LogWarning("SceneLoaderManager: Already loading scenes");
                return;
            }

            if (scenesToLoadOverride == null || scenesToLoadOverride.Count == 0)
            {
                Debug.LogWarning("SceneLoaderManager: No scenes provided to load");
                return;
            }

            StartCoroutine(LoadScenesSequence(scenesToLoadOverride));
        }

        private IEnumerator LoadScenesSequence(List<SceneReference> scenesToLoadList)
        {
            isLoading = true;

            // Fade out
            yield return FadeOut();

            // Load loading screen if configured
            string loadingScreenScenePath = null;
            if (useLoadingScreen && loadingScreenScene != null && loadingScreenScene.IsValid)
            {
                loadingScreenScenePath = loadingScreenScene.ScenePath;
                yield return LoadSceneAsync(loadingScreenScenePath, true);
            }

            // Load all scenes sequentially
            int totalScenes = scenesToLoadList.Count;
            for (int i = 0; i < totalScenes; i++)
            {
                SceneReference sceneRef = scenesToLoadList[i];

                if (sceneRef == null || !sceneRef.IsValid)
                {
                    Debug.LogWarning($"SceneLoaderManager: Scene at index {i} is invalid, skipping"); 
                    continue;
                }

                float progressBase = (float)i / totalScenes;
                float progressRange = 1f / totalScenes;

                yield return LoadSceneAsync(sceneRef.ScenePath, false, progressBase, progressRange);
            }

            // Unload loading screen if it was loaded
            if (!string.IsNullOrEmpty(loadingScreenScenePath))
            {
                yield return UnloadSceneAsync(loadingScreenScenePath);
            }

            // Final progress update
            OnLoadingProgress?.Invoke(1f);

            // Fade in
            yield return FadeIn();

            // Loading complete
            OnLoadingComplete?.Invoke();
            isLoading = false;
        }

        private IEnumerator LoadSceneAsync(string scenePath, bool isLoadingScreen, float progressBase = 0f, float progressRange = 1f)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                Debug.LogWarning($"SceneLoaderManager: Scene '{sceneName}' is already loaded, skipping");  // WARNING - MIGHT NEED TO REMOVE IF WE WANT TO RELOAD SAME LEVEL?
                yield break;
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

            if (asyncLoad == null)
            {
                Debug.LogError($"SceneLoaderManager: Failed to start loading scene: {scenePath}");
                yield break;
            }

            while (!asyncLoad.isDone)
            {
                float progress = progressBase + (asyncLoad.progress * progressRange);

                if (!isLoadingScreen)
                {
                    OnLoadingProgress?.Invoke(progress);
                }

                yield return null;
            }

            loadedScenes.Add(sceneName);
            Debug.Log($"SceneLoaderManager: Loaded scene '{sceneName}'");
        }

        public void UnloadScene(SceneReference sceneToUnload)
        {
            if (sceneToUnload == null || !sceneToUnload.IsValid)
            {
                Debug.LogError("SceneLoaderManager: Invalid scene reference provided for unload");
                return;
            }

            if (useFade)
            {
                StartCoroutine(UnloadSceneWithFadeAsync(sceneToUnload.ScenePath));
            }
            else
            {
                StartCoroutine(UnloadSceneAsync(sceneToUnload.ScenePath));
            }
        }

        public void UnloadScenes(List<SceneReference> scenesToUnload)
        {
            if (scenesToUnload == null || scenesToUnload.Count == 0)
            {
                Debug.LogWarning("SceneLoaderManager: No scenes provided to unload");
                return;
            }

            StartCoroutine(UnloadScenesSequence(scenesToUnload));
        }

        private IEnumerator UnloadScenesSequence(List<SceneReference> scenesToUnloadList)
        {
            if (useFade)
            {
                yield return FadeOut();
            }

            foreach (SceneReference sceneRef in scenesToUnloadList)
            {
                if (sceneRef == null || !sceneRef.IsValid)
                {
                    Debug.LogWarning("SceneLoaderManager: Invalid scene reference in unload list, skipping");
                    continue;
                }

                yield return UnloadSceneAsync(sceneRef.ScenePath);
            }

            if (useFade)
            {
                yield return FadeIn();
            }
        }

        private IEnumerator UnloadSceneWithFadeAsync(string scenePath)
        {
            yield return FadeOut();
            yield return UnloadSceneAsync(scenePath);
            yield return FadeIn();
        }

        public void UnloadScene(string scenePath)
        {
            StartCoroutine(UnloadSceneAsync(scenePath));
        }

        private IEnumerator UnloadSceneAsync(string scenePath)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

            if (asyncUnload == null)
            {
                Debug.LogError($"SceneLoaderManager: Failed to start unloading scene: {scenePath}");
                yield break;
            }

            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            loadedScenes.Remove(sceneName);
            Debug.Log($"SceneLoaderManager: Unloaded scene '{sceneName}'");
        }

        private IEnumerator FadeOut()
        {
            if (OVRScreenFade.instance == null)
            {
                Debug.LogWarning("SceneLoaderManager: OVRScreenFade not available");
                yield break;
            }

            OVRScreenFade.instance.fadeColor = fadeColor;
            OVRScreenFade.instance.fadeTime = fadeOutDuration;
            OVRScreenFade.instance.FadeOut();

            yield return new WaitForSeconds(fadeOutDuration);
        }

        private IEnumerator FadeIn()
        {
            if (OVRScreenFade.instance == null)
            {
                Debug.LogWarning("SceneLoaderManager: OVRScreenFade not available");
                yield break;
            }

            OVRScreenFade.instance.fadeColor = fadeColor;
            OVRScreenFade.instance.fadeTime = fadeInDuration;
            OVRScreenFade.instance.FadeIn();

            yield return new WaitForSeconds(fadeInDuration);
        }

        /// <summary>
        /// Unload all scenes that were loaded by this manager
        /// </summary>
        public void UnloadAllScenes()
        {
            StartCoroutine(UnloadAllScenesSequence());
        }

        private IEnumerator UnloadAllScenesSequence()
        {
            yield return FadeOut();

            List<string> scenesToUnload = new List<string>(loadedScenes);

            foreach (string sceneName in scenesToUnload)
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
                if (asyncUnload != null)
                {
                    yield return asyncUnload;
                }
            }

            loadedScenes.Clear();

            yield return FadeIn();
        }
    }
}
