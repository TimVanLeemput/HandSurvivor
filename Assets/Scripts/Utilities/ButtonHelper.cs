using MyBox;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonHelper : MonoBehaviour
{
    [HideInInspector] public Button button;
    private BoxCollider boxCollider;

    private void Reset()
    {
        button = GetComponent<Button>();
        SetupCollider();
    }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        
        SetupCollider();
    }

    private void SetupCollider()
    {
        boxCollider = GetComponent<BoxCollider>();
        
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            
            // Auto-size the collider based on RectTransform if available
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                boxCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 10f);
            }
            else
            {
                boxCollider.size = new Vector3(100f, 50f, 10f);
            }
            
            Debug.Log("[ButtonHelper] BoxCollider added automatically");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is on the "Hand" layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Hand"))
        {
            SelectButton();
        }
    }

    [ButtonMethod]
    public void SelectButton()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.Invoke();
            Debug.Log("[ButtonHelper] Button pressed!");
        }
        else
        {
            Debug.LogWarning("[ButtonHelper] No Button found!");
        }
    }
}