using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    public bool isRightHand;
    public Image slotImage;

    [Header("Tooltip")]
    public WeaponTooltip tooltip; // Assign this in the Inspector

    private Player player;
    private BattleManager battleManager;
    private BattleInventoryUI inventoryUI;

    [Header("Empty Slot Icons")]
    public Sprite emptyLeftHandIcon;
    public Sprite emptyRightHandIcon;

    private void Awake()
    {
        if (slotImage == null)
            slotImage = GetComponent<Image>();

        player = FindObjectOfType<Player>();
        battleManager = FindObjectOfType<BattleManager>();
        inventoryUI = FindObjectOfType<BattleInventoryUI>();

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
            slotImage.sprite = isRightHand ? emptyRightHandIcon : emptyLeftHandIcon;
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

        Weapon oldLeft = player.leftHandWeapon;
        Weapon oldRight = player.rightHandWeapon;

        if (player.EquipWeapon(weapon, isRightHand))
        {
            player.inventoryItems.Remove(weapon);

            if (oldLeft != null && oldLeft != player.leftHandWeapon && oldLeft != player.rightHandWeapon)
            {
                if (!player.inventoryItems.Contains(oldLeft))
                    player.inventoryItems.Add(oldLeft);
            }

            if (oldRight != null && oldRight != player.leftHandWeapon && oldRight != player.rightHandWeapon && oldRight != oldLeft)
            {
                if (!player.inventoryItems.Contains(oldRight))
                    player.inventoryItems.Add(oldRight);
            }

            drag.MarkEquipped();
            inventoryUI?.RefreshUI();
            battleManager?.EndPlayerTurn();
        }
    }

    // === Tooltip logic ===
    public void OnPointerEnter(PointerEventData eventData)
    {
        Weapon equippedWeapon = isRightHand ? player?.rightHandWeapon : player?.leftHandWeapon;

        if (equippedWeapon != null && tooltip != null)
        {
            tooltip.ShowTooltip(equippedWeapon, transform as RectTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.HideTooltip();
    }
}
