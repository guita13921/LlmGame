using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a single status effect icon in the UI. Handles animation and tooltip display.
/// </summary>
public class StatusEffectIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public Animator animator;
    private TurnStatusEffect effect;
    private StatusEffectTooltip tooltip;

    /// <summary>Initialize this icon with a status effect and tooltip reference.</summary>
    public void Initialize(TurnStatusEffect status, StatusEffectTooltip tooltipUI)
    {
        effect = status;
        tooltip = tooltipUI;
        UpdateVisual();
        animator?.SetTrigger("Activate");
    }

    /// <summary>Update the underlying effect (e.g., remaining turns).</summary>
    public void UpdateData(TurnStatusEffect status)
    {
        effect = status;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (iconImage != null)
            iconImage.sprite = StatusEffectInfo.GetIcon(effect.effectType);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip?.Show(effect.effectType, StatusEffectInfo.GetDescription(effect.effectType), iconImage?.sprite, transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.HideTooltip();
    }
}

