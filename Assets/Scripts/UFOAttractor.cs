using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using HandSurvivor.Stats;

public class UFOAttractor : MonoBehaviour
{
    private class AttractedEnemyData
    {
        public Enemy enemy;
        public Collider[] colliders;
        public Rigidbody[] rigidbodies;
    }
    [SerializeField] private Transform target;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private float radiusScaleMultiplier = 1f;
    [SerializeField] private GameObject attractorBeamVisual;
    [SerializeField] private GrabbableController grabbableController;
    [SerializeField] [Range(0f, 10f)] private float collectDuration = 2f;
    [SerializeField] [Range(0f, 10f)] private float ufoDuration = 10f;
    [SerializeField] [Range(0f, 10f)] private float escapeDestroyDelay = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private float flingForce = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] beamSounds;
    [SerializeField] private AudioClip[] escapeSounds;
    [SerializeField] private AudioClip[] escapeSpecificAmountEnemiesEjectedSounds;
    [SerializeField] private int escapeSpecialSoundThreshold = 10;
    [SerializeField] private float audioFadeDuration = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float maxVolume = 0.8f;
    [SerializeField] [Range(0f, 1f)] private float escapeSoundVolume = 1f;

    [Header("Visual Animation")]
    [SerializeField] private float beamScaleFadeDuration = 0.25f;
    [SerializeField] private GameObject escapeVFX;

    [Header("Events")]
    public UnityEvent OnAttractionStarted;
    public UnityEvent OnAttractionStopped;
    public UnityEvent OnEnemyCollected;
    public UnityEvent OnUFOEscape;
    public UnityEvent OnUFODestroyed;

    private string skillId = ""; // Set at runtime for stats tracking
    private float ufoTimer = 0f;
    private bool isAttracting = false;
    private bool hasEscaped = false;
    private bool timerStarted = false;
    private SelfDestruct selfDestruct;
    private System.Collections.Generic.List<Coroutine> activeAttractions = new System.Collections.Generic.List<Coroutine>();
    private System.Collections.Generic.List<AttractedEnemyData> attractedEnemies = new System.Collections.Generic.List<AttractedEnemyData>();
    private AudioSource audioSource;
    private Coroutine audioFadeCoroutine;
    private Coroutine beamScaleCoroutine;
    private Vector3 originalBeamScale;

    private void Awake()
    {
        selfDestruct = GetComponent<SelfDestruct>();
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false;
        }
        if (attractorBeamVisual != null)
        {
            originalBeamScale = attractorBeamVisual.transform.localScale;
            attractorBeamVisual.transform.localScale = Vector3.zero;
            attractorBeamVisual.SetActive(false);
        }

        if (beamSounds != null && beamSounds.Length > 0)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.spatialBlend = 1;
            audioSource.volume = 0f;
            audioSource.playOnAwake = false;
        }

        if (escapeVFX != null)
        {
            escapeVFX.SetActive(false);
        }
    }

    private void Update()
    {
        if (!timerStarted || hasEscaped) return;

        ufoTimer += Time.deltaTime;

        if (ufoTimer >= ufoDuration)
        {
            TriggerEscape();
        }
    }

    public void StartAttraction()
    {
        if (hasEscaped) return;

        bool firstGrab = !timerStarted;
        timerStarted = true;
        isAttracting = true;

        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = true;
        }
        if (animator != null)
        {
            animator.enabled = false;
        }
        if (attractorBeamVisual != null)
        {
            attractorBeamVisual.SetActive(true);
        }

        if (audioFadeCoroutine != null)
            StopCoroutine(audioFadeCoroutine);
        if (beamScaleCoroutine != null)
            StopCoroutine(beamScaleCoroutine);

        if (audioSource != null && beamSounds != null && beamSounds.Length > 0)
        {
            if (!audioSource.isPlaying)
                audioSource.PlayRandomClipWithPitch(beamSounds);
            audioFadeCoroutine = StartCoroutine(FadeAudio(audioSource, maxVolume, audioFadeDuration));
        }

        if (attractorBeamVisual != null)
        {
            beamScaleCoroutine = StartCoroutine(ScaleBeam(originalBeamScale, beamScaleFadeDuration));
        }

        if (firstGrab)
        {
            OnAttractionStarted?.Invoke();
        }
    }

    public void StopAttraction()
    {
        isAttracting = false;
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false;
        }

        OnAttractionStopped?.Invoke();
    }

    public void ReleaseEarly()
    {
        StopAttraction();
    }

    public void SetUFODuration(float duration)
    {
        ufoDuration = duration;
    }

    public void SetSkillId(string id)
    {
        skillId = id;
    }

    public float GetUFODuration()
    {
        return ufoDuration;
    }

    public float GetCollectDuration()
    {
        return collectDuration;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, ufoDuration - ufoTimer);
    }

    private void TriggerEscape()
    {
        if (hasEscaped) return;

        hasEscaped = true;

        if (attractedEnemies.Count > 0)
        {
            // Play special sound if threshold reached, otherwise play regular escape sound
            if (attractedEnemies.Count >= escapeSpecialSoundThreshold && escapeSpecificAmountEnemiesEjectedSounds != null && escapeSpecificAmountEnemiesEjectedSounds.Length > 0)
            {
                AudioSourceExtensions.PlayRandomClipAtPointWithPitch(escapeSpecificAmountEnemiesEjectedSounds, transform.position, escapeSoundVolume);
            }
            else if (escapeSounds != null && escapeSounds.Length > 0)
            {
                AudioSourceExtensions.PlayRandomClipAtPointWithPitch(escapeSounds, transform.position, escapeSoundVolume);
            }
        }

        if (escapeVFX != null)
        {
            escapeVFX.SetActive(true);
        }

        if (grabbableController != null)
        {
            grabbableController.DisableGrabbing();
        }

        if (audioFadeCoroutine != null)
            StopCoroutine(audioFadeCoroutine);
        if (beamScaleCoroutine != null)
            StopCoroutine(beamScaleCoroutine);

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
        }

        if (attractorBeamVisual != null)
        {
            beamScaleCoroutine = StartCoroutine(ScaleBeamAndDeactivate(beamScaleFadeDuration));
        }

        isAttracting = false;

        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false;
        }

        StopAllAttractions();
        FlingAttractedEnemies();
        OnUFOEscape?.Invoke();

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(escapeDestroyDelay);
        OnUFODestroyed?.Invoke();
        Destroy(gameObject);
    }

    private void StopAllAttractions()
    {
        foreach (Coroutine coroutine in activeAttractions)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeAttractions.Clear();
    }

    public int GetActiveAttractionCount()
    {
        return activeAttractions.Count;
    }

    public void SetRadiusScale(float scale)
    {
        if (capsuleCollider != null)
        {
            capsuleCollider.radius = 0.5f * scale / radiusScaleMultiplier;
        }
    }

    public void SetCollectDuration(float duration)
    {
        collectDuration = duration;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAttracting || hasEscaped || other == null) return;
        Coroutine coroutine = StartCoroutine(AttractCoroutine(other));
        activeAttractions.Add(coroutine);
    }
    
    private IEnumerator AttractCoroutine(Collider other)
    {
        Enemy enemy = FindEnemyInParents(other.transform);
        if (enemy == null) yield break;

        AttractedEnemyData enemyData = new AttractedEnemyData
        {
            enemy = enemy,
            colliders = enemy.GetComponentsInChildren<Collider>(includeInactive: true),
            rigidbodies = enemy.GetComponentsInChildren<Rigidbody>(includeInactive: true)
        };
        attractedEnemies.Add(enemyData);

        Destroy(enemy.GetComponent<InvisibleEnemyRef>().Ref.gameObject);
        Transform enemyTransform = enemy.transform;
        enemy.GetComponentInChildren<RagdollController>()?.SetRagdoll(true);
        enemy.GetComponentInChildren<RagdollController_Medium>()?.SetRagdoll(true);

        enemy.GetComponent<Animator>().SetTrigger("Floating");

        DisableAllChildColliders(enemyData);
        float elapsed = 0f;
        Vector3 initialScale = enemyTransform.localScale;

        while (elapsed < collectDuration && enemyTransform != null)
        {
            elapsed += Time.deltaTime;

            float remaining = Mathf.Max(collectDuration - elapsed, 0.0001f);

            // On recalcule la direction vers le UFOAttractor à chaque frame
            Vector3 toTarget = target.position - enemyTransform.position;

            // Vitesse nécessaire pour atteindre la cible dans le temps restant
            Vector3 velocity = toTarget / remaining;

            enemyTransform.position += velocity * Time.deltaTime;
            
            float progress = Mathf.Clamp01(elapsed / collectDuration); // 0 → 1

            if (progress >= 0.7f) // Derniers 10%
            {
                // progress = 0.9 → 1.0  devient t = 0 → 1
                float t = Mathf.InverseLerp(0.7f, 1f, progress);
                float scaleFactor = Mathf.Lerp(1f, 0f, t); // 1 → 0
                enemyTransform.localScale = initialScale * scaleFactor;
            }

            yield return null;
        }

        if (enemyTransform != null)
            enemyTransform.position = target.position;

        yield return null;

        // Track damage for stats (enemy's remaining HP = damage dealt)
        if (PlayerStatsManager.Instance != null && !string.IsNullOrEmpty(skillId) && enemy != null)
            PlayerStatsManager.Instance.RecordDamage(skillId, enemy.HP, enemy.name);

        attractedEnemies.RemoveAll(data => data.enemy == enemy);
        if (enemyTransform != null && enemyTransform.gameObject != null)
            Destroy(enemyTransform.gameObject);
        OnEnemyCollected?.Invoke();
    }
    
    private Enemy FindEnemyInParents(Transform start)
    {
        Transform current = start;

        while (current != null)
        {
            Enemy enemy = current.GetComponent<Enemy>();
            if (enemy != null)
                return enemy;

            current = current.parent;
        }

        return null;
    }
    
    private void FlingAttractedEnemies()
    {
        foreach (AttractedEnemyData enemyData in attractedEnemies)
        {
            if (enemyData.enemy == null) continue;

            foreach (Collider col in enemyData.colliders)
            {
                if (col != null)
                    col.enabled = true;
            }

            foreach (Rigidbody rb in enemyData.rigidbodies)
            {
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    Vector3 directionAway = (rb.position - transform.position).normalized;
                    rb.AddForce(directionAway * flingForce, ForceMode.Impulse);
                }
            }

            // Track damage for stats (enemy's remaining HP = damage dealt)
            if (PlayerStatsManager.Instance != null && !string.IsNullOrEmpty(skillId))
                PlayerStatsManager.Instance.RecordDamage(skillId, enemyData.enemy.HP, enemyData.enemy.name);

            enemyData.enemy.Die();
        }

        attractedEnemies.Clear();
    }

    private void DisableAllChildColliders(AttractedEnemyData enemyData)
    {
        foreach (Collider col in enemyData.colliders)
        {
            if (col != null)
                col.enabled = false;
        }
        foreach (Rigidbody rb in enemyData.rigidbodies)
        {
            if (rb != null)
                rb.useGravity = false;
        }
    }

    private IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator FadeAudioAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }

    private IEnumerator ScaleBeam(Vector3 targetScale, float duration)
    {
        Vector3 startScale = attractorBeamVisual.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            attractorBeamVisual.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        attractorBeamVisual.transform.localScale = targetScale;
    }

    private IEnumerator ScaleBeamAndDeactivate(float duration)
    {
        Vector3 startScale = attractorBeamVisual.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            attractorBeamVisual.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        attractorBeamVisual.transform.localScale = Vector3.zero;
        attractorBeamVisual.SetActive(false);
    }
}
