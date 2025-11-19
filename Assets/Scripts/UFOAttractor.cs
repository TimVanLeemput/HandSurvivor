using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UFOAttractor : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private float radiusScaleMultiplier = 1f;
    [SerializeField] private GameObject attractorBeamVisual;
    [SerializeField] [Range(0f, 10f)] private float collectDuration = 2f;
    [SerializeField] [Range(0f, 10f)] private float ufoDuration = 10f;
    [SerializeField] [Range(0f, 10f)] private float escapeDestroyDelay = 2f;
    [SerializeField] private Animator animator;

    [Header("Events")]
    public UnityEvent OnAttractionStarted;
    public UnityEvent OnAttractionStopped;
    public UnityEvent OnEnemyCollected;
    public UnityEvent OnUFOEscape;
    public UnityEvent OnUFODestroyed;

    private float ufoTimer = 0f;
    private bool isAttracting = false;
    private bool hasEscaped = false;
    private bool timerStarted = false;
    private SelfDestruct selfDestruct;
    private System.Collections.Generic.List<Coroutine> activeAttractions = new System.Collections.Generic.List<Coroutine>();

    private void Awake()
    {
        selfDestruct = GetComponent<SelfDestruct>();
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false;
        }
        if (attractorBeamVisual != null)
        {
            attractorBeamVisual.SetActive(false);
        }
    }

    private void Update()
    {
        if (!timerStarted || hasEscaped) return;

        ufoTimer += Time.deltaTime;
        if (ufoTimer >= ufoDuration)
        {
            DestroyUFO();
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
        if (attractorBeamVisual != null)
        {
            attractorBeamVisual.SetActive(false);
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

    private void DestroyUFO()
    {
        TriggerEscape();
    }

    private void TriggerEscape()
    {
        if (hasEscaped) return;

        hasEscaped = true;
        isAttracting = false;

        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false;
        }

        StopAllAttractions();
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
        if (!isAttracting || hasEscaped) return;
        Coroutine coroutine = StartCoroutine(AttractCoroutine(other));
        activeAttractions.Add(coroutine);
    }
    
    private IEnumerator AttractCoroutine(Collider other)
    {
        Enemy enemy = FindEnemyInParents(other.transform);
        if (enemy == null) yield break;
        Destroy(enemy.GetComponent<InvisibleEnemyRef>().Ref.gameObject);
        Transform enemyTransform = enemy.transform;
        enemy.GetComponentInChildren<RagdollController>()?.SetRagdoll(true);
        enemy.GetComponentInChildren<RagdollController_Medium>()?.SetRagdoll(true);
        
        enemy.GetComponent<Animator>().SetTrigger("Floating");

        DisableAllChildColliders(enemy.gameObject);
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
    
    private void DisableAllChildColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        Rigidbody[] rbs = root.GetComponentsInChildren<Rigidbody>(includeInactive: true);
        foreach (var rb in rbs)
        {
            rb.useGravity = false;
        }
    }
}
