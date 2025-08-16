using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PassiveItemUIEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI nameText;

    private PassiveItemData itemData;
    private PassiveItemTooltip tooltip;

    public void Initialize(PassiveItemData data, PassiveItemTooltip tooltipUI)
    {
        itemData = data;
        tooltip = tooltipUI;

        if (nameText != null)
            nameText.text = data.itemName;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null && itemData != null)
        {
            tooltip.ShowTooltip(itemData.itemName, itemData.description, itemData.rarity.ToString());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }
    }
}
