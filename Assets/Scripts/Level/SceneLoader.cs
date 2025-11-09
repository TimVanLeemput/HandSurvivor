using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Unity.AI.Navigation;
using HandSurvivor;

/// <summary>
/// Modular scene loader that persists across scenes and handles loading gameplay scenes.
/// Automatically fits the loaded scene's WorldReference to the calibrated table surface.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField]
    [Tooltip("The scene to load (set in inspector)")]
    private string sceneToLoad;

    [SerializeField]
    [Tooltip("Load the scene automatically when table is calibrated")]
    private bool loadOnTableCalibrated = true;

    [SerializeField]
    [Tooltip("Delay (in seconds) before loading the scene after calibration")]
    private float loadDelay = 0.5f;

    [Header("World Fitting Settings")]
    [SerializeField]
    [Tooltip("Padding (in meters) to leave around the world edges")]
    private float tablePadding = 0.05f;

    [SerializeField]
    [Tooltip("Preserve the world's aspect ratio when scaling (if false, stretches to fill entire table)")]
    private bool preserveAspectRatio = true;

    [SerializeField]
    [Tooltip("Vertical offset above the table surface (in meters)")]
    private float verticalOffset = 0.01f;

    [SerializeField]
    [Tooltip("Rotation offset to apply to the world (useful for reorienting the game)")]
    private Vector3 worldRotationOffset = Vector3.zero;

    [Header("NavMesh Settings")]
    [SerializeField]
    [Tooltip("Automatically rebuild NavMesh after scaling the world")]
    private bool rebuildNavMeshAfterScaling = true;

    [SerializeField]
    [Tooltip("Delay (in seconds) before rebuilding NavMesh to allow physics to settle")]
    private float navMeshRebuildDelay = 0.1f;

    [Header("References")]
    [SerializeField]
    [Tooltip("Reference to the TableCalibrationManager in the scene")]
    private TableCalibrationManager calibrationManager;

    // Runtime state
    private bool isSceneLoaded = false;
    private WorldReference loadedWorldReference;

    private void Awake()
    {
        // Make this persistent across scene loads
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Auto-find calibration manager if not assigned
        if (calibrationManager == null)
        {
            calibrationManager = FindFirstObjectByType<TableCalibrationManager>();
        }

        if (calibrationManager == null)
        {
            Debug.LogError("[SceneLoader] TableCalibrationManager not found! Please assign it in the inspector.");
            return;
        }

        // Subscribe to calibration event
        calibrationManager.OnTableCalibrated.AddListener(OnTableCalibrated);

        // If table is already calibrated, trigger the event manually
        if (calibrationManager.IsCalibrated && loadOnTableCalibrated)
        {
            OnTableCalibrated(calibrationManager.CalibratedTable);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (calibrationManager != null)
        {
            calibrationManager.OnTableCalibrated.RemoveListener(OnTableCalibrated);
        }
    }

    /// <summary>
    /// Called when the table has been calibrated
    /// </summary>
    private void OnTableCalibrated(Meta.XR.MRUtilityKit.MRUKAnchor table)
    {
        Debug.Log($"[SceneLoader] Table calibrated! Loading scene: {sceneToLoad}");

        if (loadOnTableCalibrated && !string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneWithDelay());
        }
    }

    /// <summary>
    /// Loads the scene after a delay
    /// </summary>
    private IEnumerator LoadSceneWithDelay()
    {
        if (loadDelay > 0)
        {
            yield return new WaitForSeconds(loadDelay);
        }

        LoadScene();
    }

    /// <summary>
    /// Manually load the configured scene (useful for button triggers or debugging)
    /// </summary>
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[SceneLoader] No scene configured to load!");
            return;
        }

        if (isSceneLoaded)
        {
            Debug.LogWarning("[SceneLoader] Scene already loaded!");
            return;
        }

        if (!calibrationManager.IsCalibrated)
        {
            Debug.LogWarning("[SceneLoader] Cannot load scene - table not calibrated yet!");
            return;
        }

        Debug.Log($"[SceneLoader] Manually loading scene: {sceneToLoad}");
        StartCoroutine(LoadSceneAsync());
    }

    /// <summary>
    /// Change the scene to load and optionally load it immediately
    /// </summary>
    public void SetSceneToLoad(string sceneName, bool loadImmediately = false)
    {
        sceneToLoad = sceneName;
        Debug.Log($"[SceneLoader] Scene to load set to: {sceneName}");

        if (loadImmediately && !isSceneLoaded)
        {
            LoadScene();
        }
    }

    /// <summary>
    /// Loads the scene asynchronously and fits the world to the table
    /// </summary>
    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        // Wait for scene to finish loading
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isSceneLoaded = true;
        Debug.Log($"[SceneLoader] Scene loaded: {sceneToLoad}");

        // Find the WorldReference in the loaded scene
        yield return StartCoroutine(FindAndFitWorldReference());
    }

    /// <summary>
    /// Finds the WorldReference in the loaded scene and fits it to the table
    /// </summary>
    private IEnumerator FindAndFitWorldReference()
    {
        // Wait a frame for all objects to initialize
        yield return null;

        // Find WorldReference in all loaded scenes
        WorldReference[] allWorldReferences = FindObjectsByType<WorldReference>(FindObjectsSortMode.None);

        if (allWorldReferences.Length == 0)
        {
            Debug.LogError("[SceneLoader] No WorldReference found in the loaded scene!");
            yield break;
        }

        // Use the first WorldReference found
        loadedWorldReference = allWorldReferences[0];
        Debug.Log($"[SceneLoader] Found WorldReference: {loadedWorldReference.gameObject.name}");

        // Fit the world to the table
        FitWorldToTable();
    }

    /// <summary>
    /// Positions and scales the WorldReference to fit exactly on the calibrated table
    /// </summary>
    private void FitWorldToTable()
    {
        if (loadedWorldReference == null)
        {
            Debug.LogError("[SceneLoader] Cannot fit world - WorldReference is null!");
            return;
        }

        if (!calibrationManager.IsCalibrated)
        {
            Debug.LogError("[SceneLoader] Cannot fit world - table not calibrated!");
            return;
        }

        // Get table bounds
        Bounds tableBounds = calibrationManager.GetTableBounds();
        Vector3 tableCenter = calibrationManager.GetTableCenter();

        Debug.Log($"[SceneLoader] Table bounds: Size={tableBounds.size}, Center={tableCenter}");

        // Calculate usable table area (with padding)
        float usableWidth = tableBounds.size.x - (tablePadding * 2);
        float usableDepth = tableBounds.size.z - (tablePadding * 2);

        Debug.Log($"[SceneLoader] Usable table area: Width={usableWidth:F3}m, Depth={usableDepth:F3}m");

        // Get the world's original bounds in local space
        Bounds worldBoundsLocal = loadedWorldReference.GetWorldBounds();
        Vector3 currentScale = loadedWorldReference.transform.localScale;

        Debug.Log($"[SceneLoader] World bounds (local): Size={worldBoundsLocal.size}, Center={worldBoundsLocal.center}");
        Debug.Log($"[SceneLoader] Current world scale: {currentScale}");

        // Calculate the actual world size accounting for current scale
        Vector3 actualWorldSize = Vector3.Scale(worldBoundsLocal.size, currentScale);

        Debug.Log($"[SceneLoader] Actual world size (scaled): {actualWorldSize}");

        // Calculate scale factor to fit world exactly on table
        float scaleX = usableWidth / actualWorldSize.x;
        float scaleZ = usableDepth / actualWorldSize.z;

        Vector3 newScale;
        if (preserveAspectRatio)
        {
            // Use the smaller scale to ensure it fits without overflowing
            float uniformScale = Mathf.Min(scaleX, scaleZ);
            newScale = currentScale * uniformScale;
            Debug.Log($"[SceneLoader] Uniform scale factor: {uniformScale:F3}");
        }
        else
        {
            // Scale X and Z independently to fill entire table
            newScale = new Vector3(
                currentScale.x * scaleX,
                currentScale.y, // Keep Y scale unchanged
                currentScale.z * scaleZ
            );
            Debug.Log($"[SceneLoader] Non-uniform scale - X: {scaleX:F3}, Z: {scaleZ:F3}");
        }

        // Apply rotation offset first (before scaling)
        if (worldRotationOffset != Vector3.zero)
        {
            loadedWorldReference.transform.rotation = Quaternion.Euler(worldRotationOffset);
            Debug.Log($"[SceneLoader] Applied rotation offset: {worldRotationOffset}");
        }

        // Apply scale to WorldReference
        loadedWorldReference.transform.localScale = newScale;
        Debug.Log($"[SceneLoader] New world scale: {newScale}");

        // Calculate the world bounds center in world space AFTER scaling
        Vector3 worldBoundsCenterLocal = worldBoundsLocal.center;
        Vector3 worldBoundsCenterWorld = loadedWorldReference.transform.TransformPoint(worldBoundsCenterLocal);

        // Calculate offset needed to center the world bounds on the table
        Vector3 currentWorldCenter = worldBoundsCenterWorld;
        Vector3 desiredWorldCenter = new Vector3(
            tableCenter.x,
            tableCenter.y + verticalOffset,
            tableCenter.z
        );

        // Apply offset to position world bounds center at table center
        Vector3 offset = desiredWorldCenter - currentWorldCenter;
        loadedWorldReference.transform.position += offset;

        Debug.Log($"[SceneLoader] World bounds center (before): {currentWorldCenter}");
        Debug.Log($"[SceneLoader] Desired center (table): {desiredWorldCenter}");
        Debug.Log($"[SceneLoader] Applied offset: {offset}");
        Debug.Log($"[SceneLoader] Final WorldReference position: {loadedWorldReference.transform.position}");

        // Final size verification
        Vector3 finalWorldSize = Vector3.Scale(worldBoundsLocal.size, newScale);
        Debug.Log($"[SceneLoader] ✓ World fitted to table!");
        Debug.Log($"[SceneLoader] Final world size: {finalWorldSize.x:F3}m x {finalWorldSize.z:F3}m");
        Debug.Log($"[SceneLoader] Table usable area: {usableWidth:F3}m x {usableDepth:F3}m");
        Debug.Log($"[SceneLoader] Fill ratio: {(finalWorldSize.x / usableWidth * 100):F1}% x {(finalWorldSize.z / usableDepth * 100):F1}%");

        // Rebuild NavMesh if needed
        if (rebuildNavMeshAfterScaling)
        {
            StartCoroutine(RebuildNavMeshDelayed());
        }
    }

    /// <summary>
    /// Rebuilds all NavMeshSurface components in the loaded world after a delay
    /// </summary>
    private IEnumerator RebuildNavMeshDelayed()
    {
        Debug.Log($"[SceneLoader] Waiting {navMeshRebuildDelay}s before rebuilding NavMesh...");
        yield return new WaitForSeconds(navMeshRebuildDelay);

        // Find all NavMeshSurface components in the loaded world
        NavMeshSurface[] navMeshSurfaces = loadedWorldReference.GetComponentsInChildren<NavMeshSurface>();

        if (navMeshSurfaces.Length == 0)
        {
            Debug.LogWarning("[SceneLoader] No NavMeshSurface components found in the loaded world.");
            yield break;
        }

        Debug.Log($"[SceneLoader] Found {navMeshSurfaces.Length} NavMeshSurface(s). Rebuilding...");

        int rebuiltCount = 0;
        foreach (NavMeshSurface surface in navMeshSurfaces)
        {
            if (surface == null) continue;

            try
            {
                // Clear old NavMesh data
                surface.RemoveData();

                // Build new NavMesh at the scaled size
                surface.BuildNavMesh();

                rebuiltCount++;
                Debug.Log($"[SceneLoader] ✓ Rebuilt NavMesh for: {surface.gameObject.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SceneLoader] Failed to rebuild NavMesh for {surface.gameObject.name}: {e.Message}");
            }
        }

        Debug.Log($"[SceneLoader] ✓ NavMesh rebuild complete! ({rebuiltCount}/{navMeshSurfaces.Length} surfaces rebuilt)");
    }

    /// <summary>
    /// Manually trigger NavMesh rebuild (useful for debugging)
    /// </summary>
    public void RebuildNavMesh()
    {
        if (loadedWorldReference == null)
        {
            Debug.LogWarning("[SceneLoader] Cannot rebuild NavMesh - no world loaded!");
            return;
        }

        StartCoroutine(RebuildNavMeshDelayed());
    }

    /// <summary>
    /// Manually trigger world fitting (useful for debugging or re-calibration)
    /// </summary>
    public void RefitWorldToTable()
    {
        if (loadedWorldReference != null)
        {
            FitWorldToTable();
            Debug.Log("[SceneLoader] World refitted to table!");
        }
        else
        {
            Debug.LogWarning("[SceneLoader] Cannot refit - no WorldReference loaded yet!");
        }
    }

    /// <summary>
    /// Unload the currently loaded scene
    /// </summary>
    public void UnloadScene()
    {
        if (!isSceneLoaded || string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneLoader] No scene to unload!");
            return;
        }

        SceneManager.UnloadSceneAsync(sceneToLoad);
        isSceneLoaded = false;
        loadedWorldReference = null;
        Debug.Log($"[SceneLoader] Scene unloaded: {sceneToLoad}");
    }
}
