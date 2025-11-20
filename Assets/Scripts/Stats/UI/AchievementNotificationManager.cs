using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace HandSurvivor.Stats.UI
{
    /// <summary>
    /// Manages achievement notification spawning, stacking, and pooling
    /// Subscribes to AchievementManager unlock events and displays world-space notifications
    /// </summary>
    public class AchievementNotificationManager : MonoBehaviour
    {
        public static AchievementNotificationManager Instance;

        [Header("Prefab")]
        [SerializeField] private AchievementNotificationUI notificationPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistanceAhead = 2f;
        [SerializeField] private float spawnHeightOffset = 0.3f; // Slightly above eye level
        [SerializeField] private float stackSpacing = 0.25f; // Vertical spacing between stacked notifications

        [Header("Pool Settings")]
        [SerializeField] private int defaultCapacity = 3;
        [SerializeField] private int maxPoolSize = 10;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private ObjectPool<AchievementNotificationUI> notificationPool;
        private List<AchievementNotificationUI> activeNotifications = new List<AchievementNotificationUI>();
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

            InitializePool();
            FindVRCamera();
        }

        private void Start()
        {
            StartCoroutine(InitializeWithRetry());
        }

        private IEnumerator InitializeWithRetry()
        {
            float timeout = 5f;
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                if (AchievementManager.Instance != null)
                {
                    break;
                }
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (AchievementManager.Instance != null)
            {
                SubscribeToEvents();
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.Log("[AchievementNotificationManager] Initialized and subscribed to achievement unlock events");
            }
            else
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                    Debug.LogWarning("[AchievementNotificationManager] AchievementManager not available after timeout");
            }
        }

        private void SubscribeToEvents()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked.AddListener(OnAchievementUnlocked);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnsubscribeFromEvents();
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked.RemoveListener(OnAchievementUnlocked);
            }
        }

        private void InitializePool()
        {
            if (notificationPrefab == null)
            {
                Debug.LogError("[AchievementNotificationManager] Notification prefab is not assigned!");
                return;
            }

            notificationPool = new ObjectPool<AchievementNotificationUI>(
                createFunc: CreateNotification,
                actionOnGet: OnGetNotification,
                actionOnRelease: OnReleaseNotification,
                actionOnDestroy: OnDestroyNotification,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxPoolSize
            );

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[AchievementNotificationManager] Pool initialized with capacity {defaultCapacity}");
        }

        private void FindVRCamera()
        {
            Camera vrCameraComponent = global::FindVRCamera.GetVRCamera();

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
                Debug.LogWarning("[AchievementNotificationManager] VR camera not found!");
            }
        }

        // ===== POOL CALLBACKS =====

        private AchievementNotificationUI CreateNotification()
        {
            AchievementNotificationUI notification = Instantiate(notificationPrefab, transform);
            notification.gameObject.SetActive(false);
            return notification;
        }

        private void OnGetNotification(AchievementNotificationUI notification)
        {
            notification.gameObject.SetActive(true);
        }

        private void OnReleaseNotification(AchievementNotificationUI notification)
        {
            notification.gameObject.SetActive(false);
        }

        private void OnDestroyNotification(AchievementNotificationUI notification)
        {
            if (notification != null)
                Destroy(notification.gameObject);
        }

        // ===== EVENT HANDLERS =====

        private void OnAchievementUnlocked(Achievement achievement)
        {
            if (achievement == null || notificationPrefab == null)
                return;

            // Check if we should show notification for this achievement
            // (Future: some achievements might be silent)
            ShowNotification(achievement);
        }

        // ===== PUBLIC API =====

        /// <summary>
        /// Show an achievement notification
        /// </summary>
        public void ShowNotification(Achievement achievement)
        {
            if (achievement == null || notificationPool == null)
                return;

            if (vrCamera == null)
            {
                FindVRCamera();
                if (vrCamera == null)
                {
                    Debug.LogWarning("[AchievementNotificationManager] Cannot show notification - VR camera not found");
                    return;
                }
            }

            // Get notification from pool
            AchievementNotificationUI notification = notificationPool.Get();

            // Calculate spawn position
            Vector3 spawnPosition = CalculateSpawnPosition();

            // Position the notification
            notification.transform.position = spawnPosition;
            notification.transform.rotation = Quaternion.identity;

            // Add to active list
            activeNotifications.Add(notification);

            // Update stack positions
            UpdateStackPositions();

            // Show notification with completion callback
            notification.Show(achievement, () => OnNotificationComplete(notification));

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log($"[AchievementNotificationManager] Showing notification: {achievement.displayName}");
        }

        private Vector3 CalculateSpawnPosition()
        {
            // Base position: ahead of player at eye level
            Vector3 basePosition = vrCamera.position +
                                   vrCamera.forward * spawnDistanceAhead +
                                   Vector3.up * spawnHeightOffset;

            // Stack offset: move up based on number of active notifications
            float stackOffset = activeNotifications.Count * stackSpacing;
            basePosition += Vector3.up * stackOffset;

            return basePosition;
        }

        private void UpdateStackPositions()
        {
            // Update all active notification positions to maintain stack
            for (int i = 0; i < activeNotifications.Count; i++)
            {
                if (activeNotifications[i] != null)
                {
                    Vector3 targetPosition = vrCamera.position +
                                           vrCamera.forward * spawnDistanceAhead +
                                           Vector3.up * (spawnHeightOffset + i * stackSpacing);

                    // Smoothly move to new position (or snap for simplicity)
                    activeNotifications[i].transform.position = targetPosition;
                }
            }
        }

        private void OnNotificationComplete(AchievementNotificationUI notification)
        {
            // Remove from active list
            activeNotifications.Remove(notification);

            // Return to pool
            notificationPool.Release(notification);

            // Update remaining stack positions
            UpdateStackPositions();
        }

        /// <summary>
        /// Clear all active notifications
        /// </summary>
        public void ClearAllNotifications()
        {
            foreach (AchievementNotificationUI notification in activeNotifications.ToArray())
            {
                notification.Hide();
                notificationPool.Release(notification);
            }

            activeNotifications.Clear();

            if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)
                Debug.Log("[AchievementNotificationManager] Cleared all notifications");
        }

        /// <summary>
        /// Get count of currently visible notifications
        /// </summary>
        public int GetActiveNotificationCount()
        {
            return activeNotifications.Count;
        }
    }
}
