using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSceneController : MonoBehaviour
{
    public enum ShopType { QuackDoctor, PassiveShop, ArmShop }
    private enum ItemCategory { Passive, Armor, Weapon, Consumable, Skill }

    [Header("UI")]
    public TMP_Text shopTitleText;
    public TMP_Text descriptionText;
    public Transform itemsParent;
    public ShopItemUI itemPrefab;

    [Header("Config")]
    public int itemsPerShop = 5;

    [Header("Item Pools")]
    public List<PassiveItemData> passiveItems = new List<PassiveItemData>();
    public List<ArmorData> armorItems = new List<ArmorData>();
    public List<Weapon> weapons = new List<Weapon>();
    public List<Item> consumables = new List<Item>();
    public List<CharacterActionData> skills = new List<CharacterActionData>();

    private Player player;
    private ShopType currentShopType;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        currentShopType = (ShopType)Random.Range(0, System.Enum.GetValues(typeof(ShopType)).Length);
        if (shopTitleText != null)
            shopTitleText.text = currentShopType.ToString();
        PopulateShop();
    }

    private void PopulateShop()
    {
        var stock = GenerateStock(itemsPerShop);
        foreach (var so in stock)
        {
            if (itemPrefab == null || itemsParent == null)
                break;
            var ui = Instantiate(itemPrefab, itemsParent);
            ui.Setup(this, so);
        }
    }

    private List<ScriptableObject> GenerateStock(int count)
    {
        var list = new List<ScriptableObject>();
        for (int i = 0; i < count; i++)
        {
            var category = GetRandomCategory();
            var item = GetRandomFromCategory(category);
            if (item != null)
                list.Add(item);
        }
        return list;
    }

    private ItemCategory GetRandomCategory()
    {
        var pool = new List<ItemCategory>();
        switch (currentShopType)
        {
            case ShopType.QuackDoctor:
                pool.Add(ItemCategory.Armor); pool.Add(ItemCategory.Armor); pool.Add(ItemCategory.Armor);
                pool.Add(ItemCategory.Passive);
                pool.Add(ItemCategory.Weapon);
                pool.Add(ItemCategory.Consumable);
                pool.Add(ItemCategory.Skill);
                break;
            case ShopType.PassiveShop:
                pool.Add(ItemCategory.Passive); pool.Add(ItemCategory.Passive); pool.Add(ItemCategory.Passive);
                pool.Add(ItemCategory.Weapon);
                pool.Add(ItemCategory.Armor);
                pool.Add(ItemCategory.Consumable);
                pool.Add(ItemCategory.Skill);
                break;
            case ShopType.ArmShop:
                pool.Add(ItemCategory.Weapon); pool.Add(ItemCategory.Weapon); pool.Add(ItemCategory.Weapon);
                pool.Add(ItemCategory.Passive);
                pool.Add(ItemCategory.Armor);
                pool.Add(ItemCategory.Consumable);
                pool.Add(ItemCategory.Skill);
                break;
        }
        return pool[Random.Range(0, pool.Count)];
    }

    private ScriptableObject GetRandomFromCategory(ItemCategory cat)
    {
        switch (cat)
        {
            case ItemCategory.Passive:
                return passiveItems.Count > 0 ? passiveItems[Random.Range(0, passiveItems.Count)] : null;
            case ItemCategory.Armor:
                return armorItems.Count > 0 ? armorItems[Random.Range(0, armorItems.Count)] : null;
            case ItemCategory.Weapon:
                return weapons.Count > 0 ? weapons[Random.Range(0, weapons.Count)] : null;
            case ItemCategory.Consumable:
                return consumables.Count > 0 ? consumables[Random.Range(0, consumables.Count)] : null;
            case ItemCategory.Skill:
                return skills.Count > 0 ? skills[Random.Range(0, skills.Count)] : null;
        }
        return null;
    }

    // === UI helpers ===
    public void ShowDescription(ScriptableObject obj)
    {
        if (descriptionText == null)
            return;
        descriptionText.text = GetItemName(obj) + "\n" + GetItemDescription(obj);
    }

    public void ClearDescription()
    {
        if (descriptionText != null)
            descriptionText.text = string.Empty;
    }

    public void AttemptPurchase(ScriptableObject obj)
    {
        if (player == null) return;
        int price = GetItemValue(obj);
        if (player.money < price)
        {
            Debug.Log("Not enough money to buy " + GetItemName(obj));
            return;
        }
        player.money -= price;

        if (obj is PassiveItemData passive)
        {
            if (!player.equippedPassiveItems.Contains(passive))
                player.equippedPassiveItems.Add(passive);
        }
        else if (obj is ArmorData armor)
        {
            foreach (var part in player.bodyParts)
            {
                if (armor.compatibleBodyParts.Contains(part.type))
                {
                    part.equippedArmor = armor;
                    break;
                }
            }
        }
        else if (obj is Weapon weapon)
        {
            player.inventoryItems.Add(weapon);
        }
        else if (obj is Item item)
        {
            player.inventoryItems.Add(item);
        }
        else if (obj is CharacterActionData skill)
        {
            player.availableActions.Add(skill);
        }
    }

    // === Data extraction helpers ===
    public Sprite GetIcon(ScriptableObject obj)
    {
        if (obj is PassiveItemData p)
            return p.icon;
        if (obj is ArmorData a)
            return a.icon;
        if (obj is Item i)
            return i.icon;
        return null;
    }

    public ItemRarity GetRarity(ScriptableObject obj)
    {
        if (obj is PassiveItemData p)
            return p.rarity;
        if (obj is ArmorData a)
            return a.rarity;
        if (obj is Item i)
            return i.rarity;
        return ItemRarity.Common;
    }

    public int GetItemValue(ScriptableObject obj)
    {
        if (obj is PassiveItemData p)
            return p.value;
        if (obj is ArmorData a)
            return a.value;
        if (obj is Item i)
            return i.value;
        return 0;
    }

    public string GetItemName(ScriptableObject obj)
    {
        if (obj is PassiveItemData p)
            return p.itemName;
        if (obj is ArmorData a)
            return a.armorName;
        if (obj is Item i)
            return i.itemName;
        if (obj is CharacterActionData act)
            return act.actionName;
        return obj.name;
    }

    public string GetItemDescription(ScriptableObject obj)
    {
        if (obj is PassiveItemData p)
            return p.description;
        if (obj is ArmorData a)
            return a.description;
        if (obj is Item i)
            return i.itemDescription;
        if (obj is CharacterActionData act)
            return "Skill";
        return string.Empty;
    }

    public Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Rare: return new Color(0.2f, 0.4f, 1f);
            case ItemRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);
        }
        return Color.white;
    }
}
