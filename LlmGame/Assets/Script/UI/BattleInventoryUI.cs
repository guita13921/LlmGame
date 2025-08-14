using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleInventoryUI : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public BattleManager battleManager;

    [Header("Inventory Containers")]
    public Transform consumableContainer;
    public Transform weaponContainer;

    [Header("Prefabs")]
    public GameObject inventoryButtonPrefab;

    [Header("Equipment Slots")]
    public WeaponSlotUI leftHandSlot;
    public WeaponSlotUI rightHandSlot;

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();
    }

    private void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// Rebuilds the inventory list and updates equipped weapon icons.
    /// </summary>
    public void RefreshUI()
    {
        if (player == null) return;

        // Clear existing children
        if (consumableContainer != null)
        {
            foreach (Transform child in consumableContainer)
                Destroy(child.gameObject);
        }
        if (weaponContainer != null)
        {
            foreach (Transform child in weaponContainer)
                Destroy(child.gameObject);
        }

        // Populate inventory
        foreach (var item in new List<Item>(player.inventoryItems))
        {
            if (item is ConsumeTurnItem)
            {
                CreateConsumableEntry(item as ConsumeTurnItem);
            }
            else if (item is Weapon weapon)
            {
                CreateWeaponEntry(weapon);
            }
        }

        // Update equipped weapon slots
        leftHandSlot?.SetWeapon(player.leftHandWeapon);
        rightHandSlot?.SetWeapon(player.rightHandWeapon);
    }

    private void CreateConsumableEntry(ConsumeTurnItem item)
    {
        if (consumableContainer == null || inventoryButtonPrefab == null || battleManager == null) return;

        GameObject obj = Instantiate(inventoryButtonPrefab, consumableContainer);
        var image = obj.GetComponent<Image>();
        if (image != null && item.icon != null)
            image.sprite = item.icon;

        var button = obj.GetComponent<ItemButtonUI>();
        if (button == null)
            button = obj.AddComponent<ItemButtonUI>();
        button.item = item;
        button.battleManager = battleManager;
    }

    private void CreateWeaponEntry(Weapon weapon)
    {
        if (weaponContainer == null || inventoryButtonPrefab == null) return;

        GameObject obj = Instantiate(inventoryButtonPrefab, weaponContainer);
        var image = obj.GetComponent<Image>();
        if (image != null && weapon.icon != null)
            image.sprite = weapon.icon;

        // Ensure canvas group for dragging
        if (obj.GetComponent<CanvasGroup>() == null)
            obj.AddComponent<CanvasGroup>();

        var drag = obj.GetComponent<WeaponDragHandler>();
        if (drag == null)
            drag = obj.AddComponent<WeaponDragHandler>();
        drag.weapon = weapon;
    }
}

