using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;
using HandSurvivor;

public class FitWorldToTable : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float tablePadding = 0.05f;
    [SerializeField] private float verticalOffset = 0.01f;
    private float _reversedWorldScale = 100f;

    [Header("NavMesh")]
    [SerializeField] private bool rebuildNavMesh = true;
    [SerializeField] private float navMeshRebuildDelay = 0.1f;

    [Header("References")]
    [SerializeField] private ARPlaneCalibration planeCalibration;
    [SerializeField] private SceneLoader sceneLoader;

    private WorldReference worldReference;

    private void Start()
    {
        if (planeCalibration == null)
        {
            planeCalibration = FindFirstObjectByType<ARPlaneCalibration>();
        }

        if (sceneLoader == null)
        {
            sceneLoader = FindFirstObjectByType<SceneLoader>();
        }

        if (planeCalibration != null)
        {
            planeCalibration.OnPlaneLocked.AddListener(OnPlaneLocked);
        }

        if (sceneLoader != null)
        {
            sceneLoader.OnSceneLoaded.AddListener(OnSceneLoaded);
        }
    }

    private void OnDestroy()
    {
        if (planeCalibration != null)
        {
            planeCalibration.OnPlaneLocked.RemoveListener(OnPlaneLocked);
        }

        if (sceneLoader != null)
        {
            sceneLoader.OnSceneLoaded.RemoveListener(OnSceneLoaded);
        }
    }

    private void OnPlaneLocked(Transform plane)
    {
        if (worldReference != null)
        {
            FitWorld();
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

        if (planeCalibration != null && planeCalibration.IsLocked)
        {
            FitWorld();
        }
    }

    private void FitWorld()
    {
        if (worldReference == null || planeCalibration == null || !planeCalibration.IsLocked)
        {
            return;
        }

        Bounds planeBounds = planeCalibration.PlaneBounds;
        Vector3 planeCenter = planeCalibration.PlaneTransform.position;

        float usableWidth = planeBounds.size.x - (tablePadding * 2);
        float usableDepth = planeBounds.size.z - (tablePadding * 2);

        worldReference.GetWorldPlaneBounds();

        Vector3 planeBottomLeft = worldReference.BottomLeftCorner;
        Vector3 planeTopRight = worldReference.TopRightCorner;

        float planeWidth = Vector3.Distance(
            new Vector3(planeBottomLeft.x, 0, 0),
            new Vector3(planeTopRight.x, 0, 0)
        );
        float planeDepth = Vector3.Distance(
            new Vector3(0, 0, planeBottomLeft.z),
            new Vector3(0, 0, planeTopRight.z)
        );

        float scaleX = usableWidth / planeWidth;
        float scaleZ = usableDepth / planeDepth;

        worldReference.transform.localScale = new Vector3(scaleX/_reversedWorldScale, 1f/_reversedWorldScale, scaleZ/_reversedWorldScale);

        worldReference.transform.position = new Vector3(
            planeCenter.x,
            planeCenter.y + verticalOffset,
            planeCenter.z
        );

        worldReference.transform.rotation = planeCalibration.PlaneTransform.rotation;

        Debug.Log($"[FitWorldToTable] Fitted world: Scale({scaleX:F2}, {scaleZ:F2})");

        if (rebuildNavMesh)
        {
            DisableNavMeshAgents();
            StartCoroutine(RebuildNavMesh());
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
