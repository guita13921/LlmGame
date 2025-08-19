using UnityEngine;
using UnityEngine.UI;   // for Button, Image, Text
using System.Linq;      // optional
using System.Reflection;

public class MapInventoryUI : MonoBehaviour
{
    [Header("Target Character (source of inventory)")]
    [Tooltip("Character whose inventory/equipment will be shown. If not assigned, the script will try to find one at runtime.")]
    [SerializeField] private Character character;

    [Header("Show these when opening")]
    [SerializeField] private GameObject bodyPartSlotRoot;
    [SerializeField] private GameObject armorItemsRoot;
    [SerializeField] private GameObject weaponItemsRoot;
    [SerializeField] private GameObject passiveItemButton;
    [SerializeField] private GameObject statTextRoot;

    [Header("Equipped Weapon UI (equipmentSlotsRoot)")]
    [Tooltip("Panel for showing the currently equipped weapon (was equipmentSlotsRoot).")]
    [SerializeField] private GameObject equipmentSlotsRoot; // now used as equipped-weapon panel
    [Tooltip("Optional label to display equipped weapon name.")]
    [SerializeField] private Text equippedWeaponLabel;
    [Tooltip("Optional icon image to display equipped weapon icon.")]
    [SerializeField] private Image equippedWeaponIcon;
    [Tooltip("Fallback icon if weapon has no icon field.")]
    [SerializeField] private Sprite defaultWeaponIcon;

    [Header("Armor List UI")]
    [Tooltip("Prefab that represents one armor entry. Must have InventoryItemDragHandler on it.")]
    [SerializeField] private GameObject armorItemPrefab;
    [Tooltip("Optional container (e.g., ScrollView Content). If null, will use armorItemsRoot.transform.")]
    [SerializeField] private Transform armorListContainer;

    [Header("Weapon List UI")]
    [Tooltip("Prefab that represents one weapon entry.")]
    [SerializeField] private GameObject weaponItemPrefab;
    [Tooltip("Optional container (e.g., ScrollView Content). If null, will use weaponItemsRoot.transform.")]
    [SerializeField] private Transform weaponListContainer;

    [Header("UI Buttons")]
    [SerializeField] private Button closeButton;   // assign in Inspector

    [Header("Startup")]
    [SerializeField] private bool startHidden = true;

    private void Awake()
    {
        if (startHidden)
            HideInventoryUI();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
            closeButton.gameObject.SetActive(!startHidden);
        }

        if (armorListContainer == null && armorItemsRoot != null)
            armorListContainer = armorItemsRoot.transform;

        if (weaponListContainer == null && weaponItemsRoot != null)
            weaponListContainer = weaponItemsRoot.transform;

        if (character == null)
            character = FindObjectOfType<Character>();
    }

    private void Update()
    {
        // Lazy lookup Character until found (avoid doing this forever once we have it)
        if (character == null)
            character = FindObjectOfType<Character>();
    }

    public void Open()
    {
        // Show UI groups
        SetActiveSafe(bodyPartSlotRoot, true);
        SetActiveSafe(armorItemsRoot, true);
        SetActiveSafe(weaponItemsRoot, true);
        SetActiveSafe(passiveItemButton, true);
        SetActiveSafe(statTextRoot, true);

        // Show equipped weapon panel (equipmentSlotsRoot) and populate it
        SetActiveSafe(equipmentSlotsRoot, true);
        PopulateEquippedWeaponUI();

        // Populate lists from Character
        PopulateArmorList();
        PopulateWeaponList();

        // Show close button
        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }

    public void Close()
    {
        HideInventoryUI();

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        bool isOpen =
            (bodyPartSlotRoot != null && bodyPartSlotRoot.activeSelf) ||
            (equipmentSlotsRoot != null && equipmentSlotsRoot.activeSelf) ||
            (armorItemsRoot != null && armorItemsRoot.activeSelf) ||
            (weaponItemsRoot != null && weaponItemsRoot.activeSelf);

        if (isOpen) Close(); else Open();
    }

    private void HideInventoryUI()
    {
        SetActiveSafe(bodyPartSlotRoot, false);
        SetActiveSafe(armorItemsRoot, false);
        SetActiveSafe(weaponItemsRoot, false);
        SetActiveSafe(passiveItemButton, false);
        SetActiveSafe(statTextRoot, false);
        SetActiveSafe(equipmentSlotsRoot, false);
    }

    private static void SetActiveSafe(GameObject go, bool value)
    {
        if (go != null && go.activeSelf != value)
            go.SetActive(value);
    }

    // === Armor List (sourced from Character) ===
    private void PopulateArmorList()
    {
        if (armorItemsRoot == null)
        {
            Debug.LogWarning("MapInventoryUI: armorItemsRoot is not assigned.");
            return;
        }
        if (armorItemPrefab == null)
        {
            Debug.LogWarning("MapInventoryUI: armorItemPrefab is not assigned. Cannot populate armor list.");
            return;
        }
        if (armorListContainer == null)
            armorListContainer = armorItemsRoot.transform;

        // Clear old entries
        for (int i = armorListContainer.childCount - 1; i >= 0; i--)
            Destroy(armorListContainer.GetChild(i).gameObject);

        if (character == null)
        {
            Debug.LogWarning("MapInventoryUI: Character reference not found. Assign a Character to show armor.");
            return;
        }

        var list = character.inventoryArmors;
        if (list == null || list.Count == 0)
            return;

        foreach (var armor in list)
        {
            if (armor == null) continue;

            var entry = Instantiate(armorItemPrefab, armorListContainer);

            // Attach armor data to draggable item (if your prefab uses it)
            var drag = entry.GetComponent<InventoryItemDragHandler>();
            if (drag != null) drag.armorData = armor;

            // Label
            var label = entry.GetComponentInChildren<Text>();
            if (label != null)
                label.text = string.IsNullOrEmpty(armor.armorName) ? armor.name : armor.armorName;

            // Icon
            var img = entry.GetComponentInChildren<Image>();
            if (img != null) img.sprite = armor.icon;
        }
    }

    // === Weapon List (sourced from Character.inventoryItems; only weapons) ===
    private void PopulateWeaponList()
    {
        if (weaponItemsRoot == null)
        {
            Debug.LogWarning("MapInventoryUI: weaponItemsRoot is not assigned.");
            return;
        }
        if (weaponItemPrefab == null)
        {
            Debug.LogWarning("MapInventoryUI: weaponItemPrefab is not assigned. Cannot populate weapon list.");
            return;
        }
        if (weaponListContainer == null)
            weaponListContainer = weaponItemsRoot.transform;

        // Clear old entries
        for (int i = weaponListContainer.childCount - 1; i >= 0; i--)
            Destroy(weaponListContainer.GetChild(i).gameObject);

        if (character == null)
        {
            Debug.LogWarning("MapInventoryUI: Character reference not found. Assign a Character to show weapons.");
            return;
        }

        var allItems = character.inventoryItems;
        if (allItems == null || allItems.Count == 0)
            return;

        foreach (var item in allItems)
        {
            if (item == null) continue;

            // Only show Weapon items
            var weapon = item as Weapon;
            if (weapon == null) continue;

            var entry = Instantiate(weaponItemPrefab, weaponListContainer);

            // 🔹 Assign weaponData to InventoryWeaponDragHandler
            var dragHandler = entry.GetComponent<InventoryWeaponDragHandler>();
            if (dragHandler != null)
                dragHandler.weaponData = weapon;

            // Set label (try common fields: weaponName, itemName; fallback to Unity name)
            var label = entry.GetComponentInChildren<Text>();
            if (label != null)
            {
                string displayName = weapon.name;
                var wnField = weapon.GetType().GetField("weaponName", BindingFlags.Public | BindingFlags.Instance);
                if (wnField != null && wnField.FieldType == typeof(string))
                {
                    var val = wnField.GetValue(weapon) as string;
                    if (!string.IsNullOrEmpty(val)) displayName = val;
                }
                else
                {
                    var inField = weapon.GetType().GetField("itemName", BindingFlags.Public | BindingFlags.Instance);
                    if (inField != null && inField.FieldType == typeof(string))
                    {
                        var val = inField.GetValue(weapon) as string;
                        if (!string.IsNullOrEmpty(val)) displayName = val;
                    }
                }
                label.text = displayName;
            }

            // Set icon (try common field: icon)
            var img = entry.GetComponentInChildren<Image>();
            if (img != null)
            {
                Sprite iconToUse = null;
                var iconField = weapon.GetType().GetField("icon", BindingFlags.Public | BindingFlags.Instance);
                if (iconField != null && iconField.FieldType == typeof(Sprite))
                    iconToUse = iconField.GetValue(weapon) as Sprite;

                img.sprite = iconToUse != null ? iconToUse : defaultWeaponIcon;
                img.enabled = (img.sprite != null);
            }
        }

    }

    // === Equipped Weapon Panel ===
    private void PopulateEquippedWeaponUI()
    {
        if (equipmentSlotsRoot == null) return;

        var weaponObj = character != null ? character.equippedWeapon as Object : null;

        // Label
        if (equippedWeaponLabel != null)
        {
            if (weaponObj != null)
            {
                string displayName = weaponObj.name;
                var nameField = weaponObj.GetType().GetField("weaponName", BindingFlags.Public | BindingFlags.Instance);
                if (nameField != null && nameField.FieldType == typeof(string))
                {
                    var v = nameField.GetValue(weaponObj) as string;
                    if (!string.IsNullOrEmpty(v)) displayName = v;
                }
                else
                {
                    var inField = weaponObj.GetType().GetField("itemName", BindingFlags.Public | BindingFlags.Instance);
                    if (inField != null && inField.FieldType == typeof(string))
                    {
                        var v = inField.GetValue(weaponObj) as string;
                        if (!string.IsNullOrEmpty(v)) displayName = v;
                    }
                }
                equippedWeaponLabel.text = displayName;
            }
            else
            {
                equippedWeaponLabel.text = "No Weapon";
            }
        }

        // Icon
        if (equippedWeaponIcon != null)
        {
            Sprite iconToUse = null;
            if (weaponObj != null)
            {
                var iconField = weaponObj.GetType().GetField("icon", BindingFlags.Public | BindingFlags.Instance);
                if (iconField != null && iconField.FieldType == typeof(Sprite))
                    iconToUse = iconField.GetValue(weaponObj) as Sprite;
            }
            equippedWeaponIcon.sprite = iconToUse != null ? iconToUse : defaultWeaponIcon;
            equippedWeaponIcon.enabled = (equippedWeaponIcon.sprite != null);
        }
    }

    public void RefreshUI()
    {
        PopulateEquippedWeaponUI();
        PopulateWeaponList();
        // Optionally:
        PopulateArmorList(); // if your armor can change during battle
    }
}
