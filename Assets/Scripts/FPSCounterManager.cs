using UnityEngine;

/// <summary>
/// Manages FPS counter spawning and visibility
/// Spawns world-space FPS counter ahead of VR camera
/// </summary>
public class FPSCounterManager : MonoBehaviour
{
    public static FPSCounterManager Instance;

    [Header("Prefab")]
    [SerializeField] private FPSCounterUI fpsCounterPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistanceAhead = 2f;
    [SerializeField] private float spawnHeightOffset = 0.5f;
    [SerializeField] private bool showOnStart = true;

    private FPSCounterUI activeFPSCounter;
    private Transform vrCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CallFindVRCamera();
    }

    private void Start()
    {
        if (showOnStart)
        {
            StartCoroutine(SpawnWithDelay());
        }
    }

    private System.Collections.IEnumerator SpawnWithDelay()
    {
        // Wait a frame for VR camera to initialize
        yield return new WaitForSeconds(0.5f);
        SpawnFPSCounter();
    }

    private void CallFindVRCamera()
    {
        Camera vrCameraComponent = FindVRCamera.GetVRCamera();

        if (vrCameraComponent != null)
        {
            vrCamera = vrCameraComponent.transform;
        }
        else
        {
            vrCamera = Camera.main?.transform;
        }

        if (vrCamera == null)
        {
            Debug.LogWarning("[FPSCounterManager] VR camera not found!");
        }
    }

    private void SpawnFPSCounter()
    {
        if (fpsCounterPrefab == null)
        {
            Debug.LogError("[FPSCounterManager] FPS counter prefab is not assigned!");
            return;
        }

        if (vrCamera == null)
        {
            CallFindVRCamera();
            if (vrCamera == null)
            {
                Debug.LogWarning("[FPSCounterManager] Cannot spawn FPS counter - VR camera not found");
                return;
            }
        }

        if (activeFPSCounter != null)
        {
            activeFPSCounter.Show();
            return;
        }

        // Instantiate FPS counter
        activeFPSCounter = Instantiate(fpsCounterPrefab, transform);

        // Position ahead of player
        Vector3 spawnPosition = vrCamera.position +
                               vrCamera.forward * spawnDistanceAhead +
                               Vector3.up * spawnHeightOffset;

        activeFPSCounter.transform.position = spawnPosition;
        activeFPSCounter.transform.rotation = Quaternion.identity;

        activeFPSCounter.Show();
    }

    public void ToggleDisplay()
    {
        if (activeFPSCounter == null)
        {
            SpawnFPSCounter();
        }
        else
        {
            if (activeFPSCounter.gameObject.activeSelf)
            {
                activeFPSCounter.Hide();
            }
            else
            {
                activeFPSCounter.Show();
            }
        }
    }

    public void SetDisplayState(bool state)
    {
        if (state)
        {
            if (activeFPSCounter == null)
            {
                SpawnFPSCounter();
            }
            else
            {
                activeFPSCounter.Show();
            }
        }
        else
        {
            if (activeFPSCounter != null)
            {
                activeFPSCounter.Hide();
            }
        }
    }
}
