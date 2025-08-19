using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Slot Settings")]
    public Image slotImage;

    [Header("Tooltip")]
    public WeaponTooltip tooltip; // Assign this in the Inspector

    private Player player;
    private BattleManager battleManager;
    private MapInventoryUI inventoryUI;

    [Header("Empty Slot Icon")]
    public Sprite emptySlotIcon;

    private void Awake()
    {
        if (slotImage == null)
            slotImage = GetComponent<Image>();

        player = FindObjectOfType<Player>();
        battleManager = FindObjectOfType<BattleManager>();
        inventoryUI = FindObjectOfType<MapInventoryUI>();

        if (tooltip == null)
            tooltip = FindObjectOfType<WeaponTooltip>();
    }

    public void SetWeapon(Weapon weapon)
    {
        if (slotImage == null) return;

        if (weapon != null && weapon.icon != null)
        {
            slotImage.sprite = weapon.icon;
            slotImage.enabled = true;
        }
        else
        {
            slotImage.sprite = emptySlotIcon;
            slotImage.enabled = true;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<WeaponDragHandler>() : null;
        if (drag == null) return;

        Weapon weapon = drag.weapon;
        if (weapon == null || player == null) return;

        if (battleManager != null && (!battleManager.isActionPhase || battleManager.currentActingCharacter != player))
            return;

        Weapon oldWeapon = player.equippedWeapon;

        if (player.EquipWeapon(weapon))
        {
            player.inventoryItems.Remove(weapon);

            if (oldWeapon != null && oldWeapon != player.equippedWeapon)
            {
                if (!player.inventoryItems.Contains(oldWeapon))
                    player.inventoryItems.Add(oldWeapon);
            }

            drag.MarkEquipped();
            inventoryUI?.RefreshUI();
            battleManager?.EndPlayerTurn();
        }
    }

    // === Tooltip logic ===
    public void OnPointerEnter(PointerEventData eventData)
    {
        Weapon equippedWeapon = player?.equippedWeapon;

        if (equippedWeapon != null && tooltip != null)
        {
            tooltip.ShowTooltip(equippedWeapon, transform as RectTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right-click to unequip
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (player != null && player.equippedWeapon != null)
            {
                // Add equipped weapon back to inventory
                if (!player.inventoryItems.Contains(player.equippedWeapon))
                    player.inventoryItems.Add(player.equippedWeapon);

                // Unequip
                player.equippedWeapon = null;

                // Update UI
                SetWeapon(null);

                // Optional: Refresh inventory UI if needed
                inventoryUI?.RefreshUI();
            }
        }
        inventoryUI?.RefreshUI();
    }

}
