using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;
using HandSurvivor;

public class FitWorldToTable : MonoBehaviour
{
    [Header("NavMesh")] [SerializeField] private bool rebuildNavMesh = true;
    [SerializeField] private float navMeshRebuildDelay = 0.1f;

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

        if (rebuildNavMesh)
        {
            DisableNavMeshAgents();
            // StartCoroutine(RebuildNavMesh());
            // StartCoroutine(EnableNavMesh());
            // worldReference.NavMeshSurface.enabled = true;
        }
    }

    private void DisableNavMeshAgents()
    {
        UnityEngine.AI.NavMeshAgent[] agents = worldReference.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>();
        foreach (UnityEngine.AI.NavMeshAgent agent in agents)
        {
            agent.enabled = false;
        }
    }

    private IEnumerator EnableNavMesh()
    {
        yield return new WaitForSeconds(navMeshRebuildDelay);
        worldReference.NavMeshSurface.enabled = true;
    }

    private IEnumerator RebuildNavMesh()
    {
        yield return new WaitForSeconds(navMeshRebuildDelay);

        NavMeshSurface[] surfaces = worldReference.GetComponentsInChildren<NavMeshSurface>();

        if (surfaces.Length == 0)
        {
            yield break;
        }

        foreach (NavMeshSurface surface in surfaces)
        {
            if (surface == null) continue;

            surface.RemoveData();
            surface.BuildNavMesh();
        }

        Debug.Log($"[FitWorldToTable] NavMesh rebuilt");

        yield return new WaitForEndOfFrame();

        UnityEngine.AI.NavMeshAgent[] agents = worldReference.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>();
        foreach (UnityEngine.AI.NavMeshAgent agent in agents)
        {
            agent.enabled = true;
        }

        Debug.Log($"[FitWorldToTable] NavMeshAgents re-enabled");
    }

    [ContextMenu("Refit World")]
    public void RefitWorld()
    {
        FitWorld();
    }
}