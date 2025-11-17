using UnityEngine;
using UnityEngine.Pool;

namespace HandSurvivor.ActiveSkills
{
    /// <summary>
    /// Manages object pooling for damage numbers
    /// Singleton pattern for easy access from damage dealing code
    /// </summary>
    public class DamageNumberManager : MonoBehaviour
    {
       [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         public static DamageNumberManager Instance { get; private set; }

        [Header("Pool Settings")] [SerializeField]
        private GameObject damageNumberPrefab;

        [SerializeField] private int defaultPoolSize = 20;
        [SerializeField] private int maxPoolSize = 50;

        [Header("Animation Settings")]
        [SerializeField] private float lifetime = 1.5f;
        [SerializeField] private float riseSpeed = 1.5f;
        [SerializeField] private float damageNumberYPositionOffset = 0.1f;

        [Header("Spacing & Pattern")]
        [Tooltip("Random offset applied to spawn position (0 = no randomness)")]
        [SerializeField] private float randomSpreadRadius = 0.2f;
        [Tooltip("Stagger spawn vertically when multiple numbers appear at once")]
        [SerializeField] private bool staggerVertically = true;
        [SerializeField] private float verticalStaggerAmount = 0.1f;
        

        [Header("Visual Settings")]
        [SerializeField] private float fontSize = 72f;
        [SerializeField] private Color textColor = new Color(1f, 0.3f, 0.2f, 1f);
        [SerializeField] private float worldScale = 0.01f;

        private ObjectPool<DamageNumber> damageNumberPool;
        private int spawnCounter = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePool()
        {
            damageNumberPool = new ObjectPool<DamageNumber>(
                createFunc: CreateDamageNumber,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: defaultPoolSize,
                maxSize: maxPoolSize
            );

            // Pre-warm pool
            DamageNumber[] prewarmed = new DamageNumber[defaultPoolSize];
            for (int i = 0; i < defaultPoolSize; i++)
            {
                prewarmed[i] = damageNumberPool.Get();
            }

            for (int i = 0; i < defaultPoolSize; i++)
            {
                damageNumberPool.Release(prewarmed[i]);
            }
        }

        private DamageNumber CreateDamageNumber()
        {
            GameObject damageNumberObj = Instantiate(damageNumberPrefab, transform);
            damageNumberObj.SetActive(false);

            DamageNumber damageNumber = damageNumberObj.GetComponent<DamageNumber>();
            if (damageNumber == null)
            {
                damageNumber = damageNumberObj.AddComponent<DamageNumber>();
            }

            return damageNumber;
        }

        private void OnGetFromPool(DamageNumber damageNumber)
        {
            damageNumber.gameObject.SetActive(true);
        }

        private void OnReleaseToPool(DamageNumber damageNumber)
        {
            damageNumber.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(DamageNumber damageNumber)
        {
            Destroy(damageNumber.gameObject);
        }

        /// <summary>
        /// Spawn a damage number at the specified world position
        /// </summary>
        public void SpawnDamageNumber(int damage, Vector3 position)
        {
            if (damageNumberPool == null)
            {
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.LogWarning("[DamageNumberManager] Pool not initialized!");
                return;
            }

            // Apply base Y offset
            Vector3 finalPosition = new Vector3(position.x, position.y + damageNumberYPositionOffset, position.z);

            // Apply random spread if enabled
            if (randomSpreadRadius > 0f)
            {
                Vector2 randomCircle = Random.insideUnitCircle * randomSpreadRadius;
                finalPosition.x += randomCircle.x;
                finalPosition.z += randomCircle.y;
            }

            // Apply vertical stagger if enabled
            if (staggerVertically)
            {
                finalPosition.y += (spawnCounter % 3) * verticalStaggerAmount;
                spawnCounter++;
            }

            DamageNumber damageNumber = damageNumberPool.Get();
            damageNumber.Initialize(damage, finalPosition, lifetime, riseSpeed, fontSize, textColor, worldScale);
        }

        /// <summary>
        /// Return a damage number to the pool (called by DamageNumber component)
        /// </summary>
        public void ReturnToPool(DamageNumber damageNumber)
        {
            if (damageNumberPool != null)
            {
                damageNumberPool.Release(damageNumber);
            }
        }

        private void OnDestroy()
        {
            damageNumberPool?.Clear();
        }
    }
}