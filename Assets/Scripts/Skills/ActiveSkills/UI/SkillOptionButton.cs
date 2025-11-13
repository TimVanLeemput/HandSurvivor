using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillOptionButton : MonoBehaviour
{
    [SerializeField] private Image _skillImage = null;
    public TextMeshProUGUI TitleText = null;
    public TextMeshProUGUI DescriptionText = null;
    public Button Button = null;

    public Sprite SkillImage
    {
        get => _skillImage != null ? _skillImage.sprite : null;
        set
        {
            if (_skillImage != null)
                _skillImage.sprite = value;
        }
    }
}
