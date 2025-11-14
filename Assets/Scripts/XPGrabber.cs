using System.Collections;
using UnityEngine;

public class XPGrabber : MonoBehaviour
{
    [SerializeField] private float collectDuration = 0.4f;
    
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(CollectCoroutine(other));
    }
    
    private IEnumerator CollectCoroutine(Collider other)
    {
        Transform dropletTransform = other.transform;
        XPDroplet droplet = other.GetComponent<XPDroplet>();
        
        other.GetComponent<Collider>().enabled = false;
        float elapsed = 0f;

        while (elapsed < collectDuration && dropletTransform != null)
        {
            elapsed += Time.deltaTime;

            float remaining = Mathf.Max(collectDuration - elapsed, 0.0001f);

            // On recalcule la direction vers le XPGrabber à chaque frame
            Vector3 toTarget = transform.position - dropletTransform.position;

            // Vitesse nécessaire pour atteindre la cible dans le temps restant
            Vector3 velocity = toTarget / remaining;

            dropletTransform.position += velocity * Time.deltaTime;

            yield return null;
        }

        // On s’assure que le droplet finit exactement sur le XPGrabber
        if (dropletTransform != null)
            dropletTransform.position = transform.position;

        // Callback de collecte + destruction
        droplet.OnDropLetCollected();
        yield return null;
        Destroy(droplet.gameObject);
    }
}
