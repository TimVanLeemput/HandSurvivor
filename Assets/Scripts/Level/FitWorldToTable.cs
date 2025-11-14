using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;
using HandSurvivor;
using HandSurvivor.Level;

public class FitWorldToTable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorldPlacementReference worldPlacementReference;

    private WorldReference worldReference;

    private void Start()
    {
        if (SceneLoaderManager.Instance != null)
        {
            SceneLoaderManager.Instance.OnLoadingComplete.AddListener(OnScenesLoaded);
        }
        else
        {
            Debug.LogWarning("[FitWorldToTable] SceneLoaderManager not found, attempting immediate fit");
            StartCoroutine(FindAndFitWorld());
        }
    }

    private void OnDestroy()
    {
        if (SceneLoaderManager.Instance != null)
        {
            SceneLoaderManager.Instance.OnLoadingComplete.RemoveListener(OnScenesLoaded);
        }
    }

    private void OnScenesLoaded()
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