using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private GameObject fpsPanel;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool showOnStart = true;

    private int frameCount;
    private float deltaTime;
    private float timer;
    private int currentFPS;

    private void Start()
    {
        if (fpsPanel != null)
        {
            fpsPanel.SetActive(showOnStart);
        }
    }

    private void Update()
    {
        frameCount++;
        deltaTime += Time.unscaledDeltaTime;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            currentFPS = Mathf.RoundToInt(frameCount / deltaTime);

            if (fpsText != null)
            {
                fpsText.text = $"FPS: {currentFPS}";
            }

            frameCount = 0;
            deltaTime = 0f;
            timer = 0f;
        }
    }

    public void ToggleDisplay()
    {
        if (fpsPanel != null)
        {
            fpsPanel.SetActive(!fpsPanel.activeSelf);
        }
    }

    public void SetDisplayState(bool state)
    {
        if (fpsPanel != null)
        {
            fpsPanel.SetActive(state);
        }
    }
}
