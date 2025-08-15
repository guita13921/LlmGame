using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;        // Item icon
    public Image borderImage;      // Rarity border
    public TMP_Text priceText;     // Price label

    private ScriptableObject itemData;
    private ShopSceneController controller;

    /// <summary>
    /// Initializes the UI with item data and visuals.
    /// </summary>
    public void Setup(ShopSceneController controller, ScriptableObject data)
    {
        this.controller = controller;
        this.itemData = data;

        // Set icon image
        if (iconImage != null)
        {
            iconImage.sprite = controller.GetIcon(data);

            // Ensure there's a Button on the icon to allow single-click purchase
            Button iconButton = iconImage.GetComponent<Button>();
            if (iconButton != null)
            {
                iconButton.onClick.RemoveAllListeners();
                iconButton.onClick.AddListener(OnImageClick);
            }
        }

        // Set price and color
        if (priceText != null)
        {
            int price = controller.GetItemValue(data);
            priceText.text = price.ToString();

            // Change price text color based on affordability
            priceText.color = controller.PlayerHasEnoughMoney(price) ? Color.white : Color.red;
        }

        // Set border color based on rarity
        if (borderImage != null)
        {
            var rarity = controller.GetRarity(data);
            borderImage.color = controller.GetRarityColor(rarity);
        }
    }

    /// <summary>
    /// Hover to show item description.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        controller?.ShowDescription(itemData);
    }

    /// <summary>
    /// Stop showing description when not hovered.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        controller?.ClearDescription();
    }

    /// <summary>
    /// Double-click on the item to purchase.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount >= 2)
        {
            AttemptPurchase();
        }
    }

    /// <summary>
    /// Single-click on the icon to purchase.
    /// </summary>
    private void OnImageClick()
    {
        AttemptPurchase();
    }

    /// <summary>
    /// Attempts to buy the item via controller.
    /// </summary>
    private void AttemptPurchase()
    {
        controller?.AttemptPurchase(itemData);
    }

    /// <summary>
    /// Optional: Call this method if you want to refresh color dynamically when player's money changes.
    /// </summary>
    public void RefreshPriceColor()
    {
        if (controller != null && priceText != null)
        {
            int price = controller.GetItemValue(itemData);
            priceText.color = controller.PlayerHasEnoughMoney(price) ? Color.white : Color.red;
        }
    }
}
