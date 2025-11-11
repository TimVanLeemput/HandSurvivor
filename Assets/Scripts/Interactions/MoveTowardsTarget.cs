using UnityEngine;

public class MoveTowardsTarget : MonoBehaviour
{
    [Header("References")]
    public Transform source;
    public Transform target;

    [Header("Settings")]
    public float speed = 1f;

    [SerializeField] private bool isMoving = false;

    void Update()
    {
        if (!isMoving || source == null || target == null)
            return;

        source.position = Vector3.MoveTowards(
            source.position,
            target.position,
            speed * Time.deltaTime
        );
    }

    // Public controls
    public void StartMoving() => isMoving = true;
    public void StopMoving() => isMoving = false;
}