using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple tooltip UI used to display details about a status effect.
/// </summary>
public class StatusEffectTooltip : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Image iconImage;

    private void Awake()
    {
        HideTooltip();
    }

    /// <summary>Show the tooltip near the given target.</summary>
    public void Show(StatusEffectType type, string description, Sprite icon, RectTransform target)
    {
        if (nameText != null)
            nameText.text = type.ToString();

        if (descriptionText != null)
            descriptionText.text = description;

        if (iconImage != null)
            iconImage.sprite = icon;

        gameObject.SetActive(true);
        PositionTooltip(target);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    private void PositionTooltip(RectTransform target)
    {
        if (target == null) return;
        RectTransform rect = transform as RectTransform;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);
        rect.position = screenPoint;
    }
}

