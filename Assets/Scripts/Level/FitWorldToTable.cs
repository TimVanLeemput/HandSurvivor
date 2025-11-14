using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;
using HandSurvivor;

public class FitWorldToTable : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private SceneLoader sceneLoader;

    [SerializeField] private WorldPlacementReference worldPlacementReference;

    private WorldReference worldReference;

    private void Start()
    {
        if (sceneLoader != null)
        {
            sceneLoader.OnSceneLoaded.AddListener(OnSceneLoaded);
        }
    }

    private void OnDestroy()
    {
        if (sceneLoader != null)
        {
            sceneLoader.OnSceneLoaded.RemoveListener(OnSceneLoaded);
        }
    }

    private void OnSceneLoaded(string sceneName)
    {
        StartCoroutine(FindAndFitWorld());
    }

    private IEnumerator FindAndFitWorld()
    {
        yield return null;

        WorldReference[] refs = FindObjectsByType<WorldReference>(FindObjectsSortMode.None);

        if (refs.Length == 0)
        {
            Debug.LogError("[FitWorldToTable] No WorldReference found!");
            yield break;
        }

        worldReference = refs[0];

        FitWorld();
    }

    private void FitWorld()
    {
        if (worldReference == null || worldPlacementReference == null)
        {
            return;
        }

        worldReference.transform.position = worldPlacementReference.transform.position;
        worldReference.transform.rotation = worldPlacementReference.transform.rotation;
        worldReference.transform.localScale = worldPlacementReference.transform.localScale / 100;
    }

    [ContextMenu("Refit World")]
    public void RefitWorld()
    {
        FitWorld();
    }
}