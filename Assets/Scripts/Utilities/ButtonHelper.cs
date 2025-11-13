using MyBox;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonHelper : MonoBehaviour
{
    [HideInInspector] public Button button;

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
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