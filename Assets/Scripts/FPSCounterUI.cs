using UnityEngine;
using TMPro;

/// <summary>
/// FPS counter UI component
/// Displays in world-space, billboards toward VR camera, always visible
/// </summary>
public class FPSCounterUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private float followDistanceAhead = 2f;
    [SerializeField] private float followHeightOffset = 0.5f;

    private Transform vrCamera;
    private int frameCount;
    private float deltaTime;
    private float timer;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        Camera vrCameraComponent = FindVRCamera.GetVRCamera();
        if (vrCameraComponent != null)
        {
            vrCamera = vrCameraComponent.transform;
        }
    }

    private void Update()
    {
        // Try to find VR camera if not found yet
        if (vrCamera == null)
        {
            Camera vrCameraComponent = FindVRCamera.GetVRCamera();
            if (vrCameraComponent != null)
            {
                vrCamera = vrCameraComponent.transform;
            }
        }

        // Follow VR camera
        if (vrCamera != null)
        {
            Vector3 targetPosition = vrCamera.position +
                                    vrCamera.forward * followDistanceAhead +
                                    Vector3.up * followHeightOffset;

            transform.position = targetPosition;
            transform.LookAt(vrCamera.position);
            transform.Rotate(0, 180, 0);
        }

        // Calculate FPS
        frameCount++;
        deltaTime += Time.unscaledDeltaTime;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            int currentFPS = Mathf.RoundToInt(frameCount / deltaTime);

            if (fpsText != null)
            {
                fpsText.text = $"FPS: {currentFPS}";
            }

            frameCount = 0;
            deltaTime = 0f;
            timer = 0f;
        }
    }

    public void Show()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
