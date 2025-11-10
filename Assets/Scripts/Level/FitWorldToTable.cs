using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;
using HandSurvivor;

public class FitWorldToTable : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float tablePadding = 0.05f;
    [SerializeField] private float verticalOffset = 0.01f;
    private float _reversedWorldScale = 100f; // Matching 0.01 world scale 

    [Header("NavMesh")]
    [SerializeField] private bool rebuildNavMesh = true;
    [SerializeField] private float navMeshRebuildDelay = 0.1f;

    [Header("References")]
    [SerializeField] private TableCalibrationManager calibrationManager;
    [SerializeField] private SceneLoader sceneLoader;

    private WorldReference worldReference;

    private void Start()
    {
        if (calibrationManager == null)
        {
            calibrationManager = FindFirstObjectByType<TableCalibrationManager>();
        }

        if (sceneLoader == null)
        {
            sceneLoader = FindFirstObjectByType<SceneLoader>();
        }

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
        if (worldReference == null || !calibrationManager.IsCalibrated)
        {
            return;
        }

        Bounds tableBounds = calibrationManager.GetTableBounds();
        Vector3 tableCenter = calibrationManager.GetTableCenter();

        float usableWidth = tableBounds.size.x - (tablePadding * 2);
        float usableDepth = tableBounds.size.z - (tablePadding * 2);

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
            tableCenter.x,
            tableCenter.y + verticalOffset,
            tableCenter.z
        );

        Debug.Log($"[FitWorldToTable] Fitted world: Scale({scaleX:F2}, {scaleZ:F2})");

        if (rebuildNavMesh)
        {
            StartCoroutine(RebuildNavMesh());
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
    }

    [ContextMenu("Refit World")]
    public void RefitWorld()
    {
        FitWorld();
    }
}
