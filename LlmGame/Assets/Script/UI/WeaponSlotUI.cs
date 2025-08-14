using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour, IDropHandler
{
    [Header("Slot Settings")]
    public bool isRightHand;
    public Image slotImage;

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

        // Only allow equipping during the player's action phase
        if (battleManager != null && (!battleManager.isActionPhase || battleManager.currentActingCharacter != player))
            return;

        // Store previous weapons to return to inventory
        Weapon oldLeft = player.leftHandWeapon;
        Weapon oldRight = player.rightHandWeapon;

        if (player.EquipWeapon(weapon, isRightHand))
        {
            // Remove equipped weapon from inventory
            player.inventoryItems.Remove(weapon);

            // Return previous weapons to inventory if they are no longer equipped
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
}
