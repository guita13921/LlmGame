using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmorSlotUI : MonoBehaviour, IDropHandler
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

        ArmorData armor = drag.armorData;
        if (armor == null) return;

        var part = player.bodyParts.FirstOrDefault(p => p.type == slotType);
        if (part != null && part.TryEquipArmor(armor))
        {
            part.EquipArmorTo(player, armor);

            if (slotImage != null && armor.icon != null)
                slotImage.sprite = armor.icon;

            player.inventoryArmors.Remove(armor);
            player.equippedArmorByPart[slotType] = armor;

            drag.MarkEquipped();

            // Refresh any inventory UIs so visuals stay in sync
            var battleInv = FindObjectOfType<BattleInventoryUI>();
            battleInv?.RefreshUI();
            var mapInv = FindObjectOfType<MapInventoryUI>();
            //mapInv?.RefreshUI();
        }
    }
}
