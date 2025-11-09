using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using MyBox;

public class DebugLogger : Singleton<DebugLogger>
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI debugAreaText;
    [SerializeField] private int maxLines = 15;
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private bool captureUnityLogs = true;
    [SerializeField] private bool showInfoLogs = true;
    [SerializeField] private bool showWarningLogs = true;
    [SerializeField] private bool showErrorLogs = true;
    [SerializeField] private bool alwaysShowErrors = true; // Errors shown regardless of enableDebug
    
    [Header("Performance")]
    [SerializeField] private bool useScrollBuffer = true;
    [SerializeField] private int maxBufferSize = 100;
    
    private Queue<string> logBuffer = new Queue<string>();
    private bool isInitialized = false;

    protected void Awake()
    {
        InitializeLogger();
    }

    private void InitializeLogger()
    {
        if (isInitialized) return;
        
        if (debugAreaText == null)
        {
            debugAreaText = GetComponent<TextMeshProUGUI>();
        }

        if (debugAreaText != null)
        {
            debugAreaText.text = string.Empty;
        }

        // Subscribe to Unity's log messages
        if (captureUnityLogs)
        {
            Application.logMessageReceived += HandleUnityLog;
        }

        isInitialized = true;
    }

    void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeLogger();
        }

        UpdateDebugAreaVisibility();

        if (enableDebug && debugAreaText != null)
        {
            LogToBuffer($"<color=\"white\">{DateTime.Now:HH:mm:ss.fff} DebugLogger enabled</color>");
            UpdateDisplay();
        }
    }

    void OnDisable()
    {
        if (captureUnityLogs)
        {
            Application.logMessageReceived -= HandleUnityLog;
        }
    }

    private void UpdateDebugAreaVisibility()
    {
        if (debugAreaText != null)
        {
            debugAreaText.enabled = enableDebug || alwaysShowErrors;
            enabled = enableDebug || alwaysShowErrors;
        }
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                if (alwaysShowErrors || (enableDebug && showErrorLogs))
                {
                    LogToBuffer($"<color=\"red\">{DateTime.Now:HH:mm:ss.fff} [Unity Error] {logString}</color>");
                    UpdateDisplay();
                    
                    // Force visibility for errors if alwaysShowErrors is true
                    if (alwaysShowErrors && debugAreaText != null)
                    {
                        debugAreaText.enabled = true;
                    }
                }
                break;
                
            case LogType.Warning:
                if (enableDebug && showWarningLogs)
                {
                    LogToBuffer($"<color=\"yellow\">{DateTime.Now:HH:mm:ss.fff} [Unity Warning] {logString}</color>");
                    UpdateDisplay();
                }
                break;
                
            case LogType.Log:
                if (enableDebug && showInfoLogs)
                {
                    LogToBuffer($"<color=\"green\">{DateTime.Now:HH:mm:ss.fff} [Unity] {logString}</color>");
                    UpdateDisplay();
                }
                break;
                
            case LogType.Assert:
                if (alwaysShowErrors || (enableDebug && showErrorLogs))
                {
                    LogToBuffer($"<color=\"red\">{DateTime.Now:HH:mm:ss.fff} [Unity Assert] {logString}</color>");
                    UpdateDisplay();
                }
                break;
        }
    }

    public void Clear()
    {
        logBuffer.Clear();
        if (debugAreaText != null)
        {
            debugAreaText.text = string.Empty;
        }
    }

    public void LogInfo(string message)
    {
        if (!enableDebug || !showInfoLogs) return;
        LogToBuffer($"<color=\"green\">{DateTime.Now:HH:mm:ss.fff} {message}</color>");
        UpdateDisplay();
    }

    public void LogError(string message)
    {
        if (!alwaysShowErrors && (!enableDebug || !showErrorLogs)) return;
        
        LogToBuffer($"<color=\"red\">{DateTime.Now:HH:mm:ss.fff} {message}</color>");
        UpdateDisplay();
        
        // Force visibility for errors if alwaysShowErrors is true
        if (alwaysShowErrors && debugAreaText != null)
        {
            debugAreaText.enabled = true;
        }
    }

    public void LogWarning(string message)
    {
        if (!enableDebug || !showWarningLogs) return;
        LogToBuffer($"<color=\"yellow\">{DateTime.Now:HH:mm:ss.fff} {message}</color>");
        UpdateDisplay();
    }

    private void LogToBuffer(string formattedMessage)
    {
        if (useScrollBuffer)
        {
            logBuffer.Enqueue(formattedMessage);
            
            // Maintain buffer size
            while (logBuffer.Count > maxBufferSize)
            {
                logBuffer.Dequeue();
            }
        }
        else
        {
            logBuffer.Enqueue(formattedMessage);
        }
    }

    private void UpdateDisplay()
    {
        if (debugAreaText == null) return;

        if (useScrollBuffer)
        {
            // Show only the last 'maxLines' entries
            var linesToShow = logBuffer.Skip(Math.Max(0, logBuffer.Count - maxLines));
            debugAreaText.text = string.Join("\n", linesToShow);
        }
        else
        {
            // Clear and show all lines when reaching maxLines
            if (logBuffer.Count >= maxLines)
            {
                logBuffer.Clear();
            }
            debugAreaText.text = string.Join("\n", logBuffer);
        }
    }

    // Optional: Manual refresh if needed
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }

    // Optional: Get log history
    public string[] GetLogHistory()
    {
        return logBuffer.ToArray();
    }
}
