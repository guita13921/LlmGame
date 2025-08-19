using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;

public class EquipWeaponDropTarget : MonoBehaviour, IDropHandler
{
    [SerializeField] private Character character;
    [SerializeField] private Text equippedLabel;
    [SerializeField] private Image equippedIcon;
    [SerializeField] private Sprite defaultIcon;
    private MapInventoryUI mapInventoryUI;

    private void Awake()
    {
        mapInventoryUI = FindObjectOfType<MapInventoryUI>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<InventoryWeaponDragHandler>() : null;
        if (drag == null || drag.weaponData == null) return;

        if (character == null) character = FindObjectOfType<Character>();
        if (character == null) return;

        Weapon newWeapon = drag.weaponData;
        Weapon oldWeapon = character.equippedWeapon;

        // 🔄 Equip new weapon first
        character.equippedWeapon = newWeapon;

        // ✅ Remove new weapon from inventory if present
        character.inventoryItems.Remove(newWeapon);

        // ✅ Return old weapon to inventory (if it's not null and not the same as new)
        if (oldWeapon != null && oldWeapon != newWeapon)
        {
            if (!character.inventoryItems.Contains(oldWeapon))
                character.inventoryItems.Add(oldWeapon);
        }

        drag.MarkEquipped();

        // ✅ Update UI (already present in your version)
        if (equippedLabel != null)
        {
            string displayName = newWeapon.name;
            var nameField = newWeapon.GetType().GetField("weaponName", BindingFlags.Public | BindingFlags.Instance);
            if (nameField != null && nameField.FieldType == typeof(string))
            {
                var val = nameField.GetValue(newWeapon) as string;
                if (!string.IsNullOrEmpty(val)) displayName = val;
            }
            else
            {
                var itemNameField = newWeapon.GetType().GetField("itemName", BindingFlags.Public | BindingFlags.Instance);
                if (itemNameField != null && itemNameField.FieldType == typeof(string))
                {
                    var val = itemNameField.GetValue(newWeapon) as string;
                    if (!string.IsNullOrEmpty(val)) displayName = val;
                }
            }
            equippedLabel.text = displayName;
        }

        if (equippedIcon != null)
        {
            Sprite icon = null;
            var iconField = newWeapon.GetType().GetField("icon", BindingFlags.Public | BindingFlags.Instance);
            if (iconField != null && iconField.FieldType == typeof(Sprite))
                icon = iconField.GetValue(newWeapon) as Sprite;

            equippedIcon.sprite = icon != null ? icon : defaultIcon;
            equippedIcon.enabled = (equippedIcon.sprite != null);
        }

        // ✅ Refresh UI (so list updates too)
        FindObjectOfType<MapInventoryUI>()?.RefreshUI();
        FindObjectOfType<BattleInventoryUI>()?.RefreshUI();
    }


}
