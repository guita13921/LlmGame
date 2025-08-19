using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmorSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public BodyPartType slotType;
    public Image slotImage;

    private Player player;

    private void Awake()
    {
        if (slotImage == null)
            slotImage = GetComponent<Image>();
        player = FindObjectOfType<Player>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<InventoryItemDragHandler>() : null;
        if (drag == null) return;

        ArmorData newArmor = drag.armorData;
        if (newArmor == null) return;

        var part = player.bodyParts.FirstOrDefault(p => p.type == slotType);
        if (part == null) return;

        // 🔹 Get old equipped armor BEFORE replacing
        ArmorData oldArmor = part.equippedArmor;

        // 🔸 Try to equip new armor
        if (part.TryEquipArmor(newArmor))
        {
            part.EquipArmorTo(player, newArmor);

            // 🔹 Remove new armor from inventory
            player.inventoryArmors.Remove(newArmor);

            // 🔹 Return old armor to inventory, if different
            if (oldArmor != null && oldArmor != newArmor)
            {
                if (!player.inventoryArmors.Contains(oldArmor))
                    player.inventoryArmors.Add(oldArmor);
            }

            // 🔹 Update equipped mapping
            player.equippedArmorByPart[slotType] = newArmor;

            // 🔹 Update icon
            if (slotImage != null && newArmor.icon != null)
                slotImage.sprite = newArmor.icon;

            drag.MarkEquipped();

            // 🔹 Refresh UI
            FindObjectOfType<BattleInventoryUI>()?.RefreshUI();
            FindObjectOfType<MapInventoryUI>()?.RefreshUI();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        player = FindObjectOfType<Player>(); // Refresh reference in case it's lost
        if (player == null) return;

        // Get equipped armor on this slot
        if (!player.equippedArmorByPart.TryGetValue(slotType, out ArmorData equippedArmor) || equippedArmor == null)
            return;

        // Remove armor from slot
        var part = player.bodyParts.FirstOrDefault(p => p.type == slotType);
        if (part != null)
            part.ClearArmor(player); // ✅ Pass the Player/Character as required

        // Put armor back into inventory if not already
        if (!player.inventoryArmors.Contains(equippedArmor))
            player.inventoryArmors.Add(equippedArmor);

        // Remove from equipped dict
        player.equippedArmorByPart[slotType] = null;

        // Clear icon
        if (slotImage != null)
            slotImage.sprite = null;

        // Refresh UIs
        FindObjectOfType<BattleInventoryUI>()?.RefreshUI();
        FindObjectOfType<MapInventoryUI>()?.RefreshUI();
    }
}
