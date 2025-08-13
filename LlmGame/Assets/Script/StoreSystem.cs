using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates a list of items for the store. Ensures no duplicates with player's inventory.
/// Supports passive items and armor. Selection weighted by item rarity.
/// </summary>
public class StoreSystem : MonoBehaviour
{
    [Header("Item Pools")]
    public List<PassiveItemData> passiveItems = new();
    public List<ArmorData> armors = new();

    [Header("Rarity Weights")]
    [Range(0, 100)] public int commonWeight = 60;
    [Range(0, 100)] public int rareWeight = 30;
    [Range(0, 100)] public int epicWeight = 10;

    /// <summary>
    /// Create a store stock list with items the player doesn't already own.
    /// </summary>
    public List<ScriptableObject> GenerateStock(Player player, int count)
    {
        var pool = BuildAvailablePool(player);
        var stock = new List<ScriptableObject>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            var item = GetRandomItem(pool);
            stock.Add(item);
            pool.Remove(item);
        }
        return stock;
    }

    /// <summary>
    /// Attempt to purchase an item and add it to the player.
    /// </summary>
    public void BuyItem(Player player, ScriptableObject item)
    {
        if (item is PassiveItemData passive)
        {
            player.EquipPassiveItem(passive);
        }
        else if (item is ArmorData armor)
        {
            if (!player.inventoryArmors.Contains(armor))
                player.inventoryArmors.Add(armor);
        }
    }


    private List<ScriptableObject> BuildAvailablePool(Player player)
    {
        var pool = new List<ScriptableObject>();

        var ownedPassives = new HashSet<PassiveItemData>(player.equippedPassiveItems);

        var ownedArmors = new HashSet<ArmorData>(
            player.bodyParts.Where(p => p.equippedArmor != null).Select(p => p.equippedArmor)
            .Concat(player.inventoryArmors)
        );

        pool.AddRange(passiveItems.Where(p => !ownedPassives.Contains(p)));
        pool.AddRange(armors.Where(a => !ownedArmors.Contains(a)));

        return pool;
    }


    private ScriptableObject GetRandomItem(List<ScriptableObject> pool)
    {
        int total = pool.Sum(i => GetWeight(GetRarity(i)));
        int roll = Random.Range(0, total);
        foreach (var item in pool)
        {
            int w = GetWeight(GetRarity(item));
            if (roll < w)
                return item;
            roll -= w;
        }
        return pool[0];
    }

    private ItemRarity GetRarity(ScriptableObject obj)
    {
        return obj switch
        {
            PassiveItemData p => p.rarity,
            ArmorData a => a.rarity,
            _ => ItemRarity.Common
        };
    }

    private int GetWeight(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => commonWeight,
            ItemRarity.Rare => rareWeight,
            ItemRarity.Epic => epicWeight,
            _ => commonWeight
        };
    }
}
