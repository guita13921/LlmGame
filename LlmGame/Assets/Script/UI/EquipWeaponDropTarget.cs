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

    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<InventoryWeaponDragHandler>() : null;
        if (drag == null || drag.weaponData == null) return;

        if (character == null) character = FindObjectOfType<Character>();
        if (character == null) return;

        character.equippedWeapon = drag.weaponData;   // equip
        drag.MarkEquipped();                          // remove the dragged UI entry

        // Update simple UI (name + icon)
        if (equippedLabel != null)
        {
            string displayName = drag.weaponData.name;
            var nameField = drag.weaponData.GetType().GetField("weaponName", BindingFlags.Public | BindingFlags.Instance);
            if (nameField != null && nameField.FieldType == typeof(string))
            {
                var val = nameField.GetValue(drag.weaponData) as string;
                if (!string.IsNullOrEmpty(val)) displayName = val;
            }
            else
            {
                var itemNameField = drag.weaponData.GetType().GetField("itemName", BindingFlags.Public | BindingFlags.Instance);
                if (itemNameField != null && itemNameField.FieldType == typeof(string))
                {
                    var val = itemNameField.GetValue(drag.weaponData) as string;
                    if (!string.IsNullOrEmpty(val)) displayName = val;
                }
            }
            equippedLabel.text = displayName;
        }

        if (equippedIcon != null)
        {
            Sprite icon = null;
            var iconField = drag.weaponData.GetType().GetField("icon", BindingFlags.Public | BindingFlags.Instance);
            if (iconField != null && iconField.FieldType == typeof(Sprite))
                icon = iconField.GetValue(drag.weaponData) as Sprite;

            equippedIcon.sprite = icon != null ? icon : defaultIcon;
            equippedIcon.enabled = (equippedIcon.sprite != null);
        }
    }
}
