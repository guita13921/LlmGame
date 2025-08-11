using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Map;

/// <summary>
/// Contains concrete handlers for all MysteryScene events such as the
/// noodle cart, spooky alleyway, strangers, etc.  Methods here can be
/// wired to <see cref="MysteryEvent.Option.onSelected"/> via the
/// inspector.
/// </summary>
public class MysterySceneEventHandler : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public TreasureSystem treasureSystem;
    public StoreSystem storeSystem;

    [Header("Config")]
    public int smallCreditCost = 10;
    public int droneMpCost = 20;
    public int hackMpCost = 10;
    public PassiveItemData msgNoodleItem;
    public List<Weapon> weaponPool = new();

    // Track karma from the homeless camp event.
    public bool goodKarma = false;

    #region Utility
    private void ChangeMoney(int amount)
    {
        if (player == null) return;
        player.money = Mathf.Max(0, player.money + amount);
    }

    private void ChangeHP(int amount)
    {
        if (player == null) return;
        player.currentHP = Mathf.Clamp(player.currentHP + amount, 0, player.maxHP);
    }

    private void ChangeMP(int amount)
    {
        if (player == null) return;
        player.currentMP = Mathf.Clamp(player.currentMP + amount, 0, player.maxMP);
    }

    private void GrantRandomItem(ItemRarity rarity = ItemRarity.Common)
    {
        if (player == null || storeSystem == null) return;
        var pool = new List<ScriptableObject>();
        pool.AddRange(storeSystem.passiveItems.Where(p => p.rarity == rarity));
        pool.AddRange(storeSystem.armors.Where(a => a.rarity == rarity));
        if (pool.Count == 0) return;
        ScriptableObject item = pool[Random.Range(0, pool.Count)];
        storeSystem.BuyItem(player, item);
    }

    private void GrantWeapon()
    {
        if (player == null || weaponPool.Count == 0) return;
        Weapon weapon = weaponPool[Random.Range(0, weaponPool.Count)];
        player.leftHandWeapon = weapon; // simple assignment for now
    }
    #endregion

    #region Noodle Cart
    public void OrderNoodles()
    {
        ChangeMoney(-smallCreditCost);
        if (msgNoodleItem != null && player != null && !player.equippedPassiveItems.Contains(msgNoodleItem))
        {
            player.equippedPassiveItems.Add(msgNoodleItem);
            msgNoodleItem.EquipTo(player);
        }
    }

    public void OrderAndClean()
    {
        ChangeHP(25);
    }
    #endregion

    #region Spooky Alleyway
    public void InvestigateAlley()
    {
        if (Random.value < 0.5f)
        {
            GrantRandomItem();
        }
        else
        {
            PlayerData.Instance?.SetNextNode(NodeType.MinorEnemy, EnemyDifficulty.Normal);
        }
    }

    public void ThrowLightDrone()
    {
        ChangeMP(-droneMpCost);
        if (Random.value < 0.5f)
            ChangeMoney(Random.Range(5, 16));
        else
            GrantRandomItem();
    }
    #endregion

    #region Cloaked Stranger
    public void HearCloakedMan()
    {
        if (Random.value < 0.5f)
        {
            if (Random.value < 0.5f)
                GrantRandomItem();
            else
                ChangeMoney(Random.Range(5, 16));
        }
        else
        {
            ChangeHP(-10);
        }
    }

    public void PayCloakedMan()
    {
        ChangeMoney(-smallCreditCost);
        GrantRandomItem(ItemRarity.Common);
    }

    public int intimidateAttackThreshold = 15;
    public void IntimidateCloakedMan()
    {
        if (player != null && player.attack >= intimidateAttackThreshold)
        {
            ChangeMoney(Random.Range(10, 21));
            GrantRandomItem(ItemRarity.Rare);
        }
        else
        {
            ChangeHP(-15);
        }
    }
    #endregion

    #region Woman Stranger
    public void FlirtWithWoman()
    {
        PlayerData.Instance?.SetNextNode(NodeType.EliteEnemy, EnemyDifficulty.Normal);
    }
    #endregion

    #region Man in Suit
    public void WorkForSuit()
    {
        int roll = Random.Range(0, 3);
        if (roll == 0) ChangeMoney(Random.Range(10, 21));
        else if (roll == 1) GrantRandomItem();
        // roll == 2 -> map reveal, not implemented
        PlayerData.Instance?.SetNextNode(NodeType.MinorEnemy, EnemyDifficulty.Normal);
    }

    public void PickpocketSuit()
    {
        if (Random.value < 0.5f)
        {
            ChangeMoney(Random.Range(15, 31));
            GrantRandomItem(ItemRarity.Rare);
        }
        else
        {
            PlayerData.Instance?.SetNextNode(NodeType.EliteEnemy, EnemyDifficulty.Hard);
        }
    }
    #endregion

    #region Vending Machine
    public void BuyFromVending()
    {
        ChangeMoney(-smallCreditCost);
        if (Random.value < 0.5f)
            GrantRandomItem();
    }

    public void HackVending()
    {
        ChangeMP(-hackMpCost);
        GrantRandomItem();
    }

    public void KickVending()
    {
        if (Random.value < 0.5f)
            GrantRandomItem();
    }
    #endregion

    #region Dumpster
    public void DigDumpster()
    {
        if (Random.value < 0.5f)
            GrantRandomItem(ItemRarity.Rare);
        else
            ChangeHP(-10);
    }

    public void DroneDumpster()
    {
        ChangeMP(-droneMpCost);
        GrantRandomItem(ItemRarity.Rare);
    }
    #endregion

    #region Homeless Camp
    public void ShareWithHomeless()
    {
        ChangeMoney(-smallCreditCost);
        goodKarma = true;
    }

    public void TradeWithHomeless()
    {
        if (player == null || player.equippedPassiveItems.Count == 0) return;
        int idx = Random.Range(0, player.equippedPassiveItems.Count);
        ItemRarity rarity = player.equippedPassiveItems[idx].rarity;
        player.equippedPassiveItems.RemoveAt(idx);
        GrantRandomItem(rarity);
    }

    public void StealFromHomeless()
    {
        GrantRandomItem();
        PlayerData.Instance?.SetNextNode(NodeType.MinorEnemy, EnemyDifficulty.Normal);
    }
    #endregion

    #region Police Car Wreck
    public void LootPoliceCar()
    {
        GrantWeapon();
        if (Random.value < 0.5f)
            PlayerData.Instance?.SetNextNode(NodeType.MinorEnemy, EnemyDifficulty.Hard);
    }

    public void HackPoliceCar()
    {
        ChangeMP(-hackMpCost);
        if (Random.value < 0.5f)
            GrantWeapon();
        else
            GrantRandomItem();
    }
    #endregion
}

