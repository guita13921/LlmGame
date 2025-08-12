using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image iconImage;
    public Image borderImage;
    public TMP_Text priceText;

    private ScriptableObject itemData;
    private ShopSceneController controller;

    public void Setup(ShopSceneController controller, ScriptableObject data)
    {
        this.controller = controller;
        this.itemData = data;

        if (iconImage != null)
            iconImage.sprite = controller.GetIcon(data);

        if (priceText != null)
            priceText.text = controller.GetItemValue(data).ToString();

        if (borderImage != null)
            borderImage.color = controller.GetRarityColor(controller.GetRarity(data));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        controller?.ShowDescription(itemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        controller?.ClearDescription();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount >= 2)
        {
            controller?.AttemptPurchase(itemData);
        }
    }
}
