using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NexusLifeDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _fillImage;
    [SerializeField] private TextMeshProUGUI _percentageText;

    [Header("Visual Settings")]
    [SerializeField] private Color _healthyColor = Color.green;
    [SerializeField] private Color _criticalColor = Color.red;
    [SerializeField] private float _criticalThreshold = 0.25f;

    public void UpdateDisplay(float currentHealth, float maxHealth)
    {
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        if (_fillImage != null)
            _fillImage.fillAmount = healthPercent;

        if (_percentageText != null)
        {
            _percentageText.text = $"{Mathf.FloorToInt(healthPercent * 100)}%";
            _percentageText.color = healthPercent <= _criticalThreshold ? _criticalColor : _healthyColor;
        }
    }
}
