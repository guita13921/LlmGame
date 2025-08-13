// (Header remains unchanged)
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Map;

public class MysterySceneEventHandler : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public TreasureSystem treasureSystem;
    public StoreSystem storeSystem;

    [Header("Config")]
    public int smallCreditCost = 50;
    public int droneMpCost = 20;
    public int hackMpCost = 20;
    public PassiveItemData msgNoodleItem;
    public List<Weapon> weaponPool = new();

    public bool goodKarma = false;

    void Awake() { }

    #region Utility + Debug
    private void Log(string msg)
    {
        Debug.Log($"[MysteryEvent] {msg}", this);
        Outcome($"{msg}");
    }

    public static System.Action<string> OnOutcome;

    private void Outcome(string msg)
    {
        OnOutcome?.Invoke(msg);
    }

    private void ChangeMoney(int amount)
    {
        if (player == null) return;
        int before = player.money;
        player.money = Mathf.Max(0, player.money + amount);
        int after = player.money;
        string dir = amount >= 0 ? "Gained" : "Spent";
        Log($"{dir} {Mathf.Abs(amount)} credits (Money: {before} -> {after})");
    }

    private void ChangeHP(int amount)
    {
        if (player == null) return;
        int before = player.currentHP;
        player.currentHP = Mathf.Clamp(player.currentHP + amount, 0, player.maxHP);
        int after = player.currentHP;
        string dir = amount >= 0 ? "Healed" : "Took";
        Log($"{dir} {Mathf.Abs(amount)} HP (HP: {before} -> {after})");
    }

    private void ChangeMP(int amount)
    {
        if (player == null) return;
        int before = player.currentMP;
        player.currentMP = Mathf.Clamp(player.currentMP + amount, 0, player.maxMP);
        int after = player.currentMP;
        string dir = amount >= 0 ? "Restored" : "Spent";
        Log($"{dir} {Mathf.Abs(amount)} MP (MP: {before} -> {after})");
    }

    private void GrantRandomItem(ItemRarity rarity = ItemRarity.Common)
    {
        if (player == null || storeSystem == null) return;
        var pool = new List<ScriptableObject>();
        pool.AddRange(storeSystem.passiveItems.Where(p => p.rarity == rarity));
        pool.AddRange(storeSystem.armors.Where(a => a.rarity == rarity));
        if (pool.Count == 0)
        {
            Log($"No items available in storeSystem for rarity {rarity}.");
            return;
        }
        ScriptableObject item = pool[Random.Range(0, pool.Count)];
        storeSystem.BuyItem(player, item);
        Log($"Granted item: {item.name} (Rarity: {rarity})");
    }

    private void GrantRandomItemFromRarities(params ItemRarity[] rarities)
    {
        if (rarities == null || rarities.Length == 0) return;
        var rarity = rarities[Random.Range(0, rarities.Length)];
        GrantRandomItem(rarity);
    }

    private void GrantWeapon()
    {
        if (player == null || weaponPool.Count == 0)
        {
            Log("No weapon granted: player null or weaponPool empty.");
            return;
        }
        Weapon weapon = weaponPool[Random.Range(0, weaponPool.Count)];
        player.inventoryItems.Add(weapon);
        Log($"Granted weapon: {weapon.name}");
    }

    private void QueueNextEncounter(NodeType type, EnemyDifficulty difficulty)
    {
        PlayerData.Instance?.SetNextNode(type, difficulty);
        Log($"Next encounter queued: {type} ({difficulty})");
    }

    private void QueueNextMystery()
    {
        PlayerData.Instance?.SetNextNode(NodeType.Mystery, EnemyDifficulty.None);
        //Log($"Next node queued: {NodeType.Mystery}");
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
            Log($"Received passive item: {msgNoodleItem.name}");
        }
        else
        {
            Log("MSG noodle item not granted (missing item/player or already owned).");
        }
        QueueNextMystery();
    }

    public void OrderAndClean()
    {
        ChangeHP(25);
        Log("Helped clean up the stall.");
        QueueNextMystery();
    }

    public void Noodle_WalkAway()
    {
        Log("Noodle Cart: walked away (no effect).");
        QueueNextMystery();
    }
    #endregion

    #region Spooky Alleyway
    public void InvestigateAlley()
    {
        if (Random.value < 0.5f)
        {
            Log("Alley: found loot.");
            GrantRandomItemFromRarities(ItemRarity.Common, ItemRarity.Rare);
            QueueNextMystery();
        }
        else
        {
            Log("Alley: ambush flagged for next encounter.");
            QueueNextEncounter(NodeType.MinorEnemy, EnemyDifficulty.Normal);
        }
    }

    public void ThrowLightDrone()
    {
        ChangeMP(-droneMpCost);
        if (Random.value < 0.5f)
        {
            int credits = Random.Range(5, 16);
            ChangeMoney(credits);
            Log($"Drone revealed hidden credits: +{credits}");
        }
        else
        {
            GrantRandomItem();
            Log("Drone revealed a hidden item.");
        }
        QueueNextMystery();
    }

    public void Alley_Ignore()
    {
        Log("Alley: ignored (no effect).");
        QueueNextMystery();
    }
    #endregion

    #region Cloaked Stranger
    public void HearCloakedMan()
    {
        if (Random.value < 0.5f)
        {
            if (Random.value < 0.5f)
            {
                Log("Cloaked man: offered an item.");
                GrantRandomItem();
            }
            else
            {
                int credits = Random.Range(5, 16);
                ChangeMoney(credits);
                Log($"Cloaked man: slipped you credits (+{credits}).");
            }
        }
        else
        {
            int dmg = Random.Range(8, 16);
            ChangeHP(-dmg);
            Log($"Cloaked man: cursed (-{dmg} HP).");
        }
        QueueNextMystery();
    }

    public void PayCloakedMan()
    {
        ChangeMoney(-smallCreditCost);
        Log("Paid cloaked man for information.");
        GrantRandomItem(ItemRarity.Common);
        QueueNextMystery();
    }

    public void IntimidateCloakedMan()
    {
        if (Random.value < 0.5f)
        {
            int credits = Random.Range(10, 21);
            ChangeMoney(credits);
            GrantRandomItem(ItemRarity.Rare);
            Log($"Intimidation succeeded: stole +{credits} credits and rare loot.");
            QueueNextMystery();
        }
        else
        {
            int dmg = Random.Range(10, 21);
            ChangeHP(-dmg);
            Log($"Intimidation failed: took -{dmg} HP.");
            QueueNextEncounter(NodeType.MinorEnemy, EnemyDifficulty.Normal);
        }
    }

    public void Cloaked_WalkAway()
    {
        Log("Cloaked stranger: walked away (no effect).");
        QueueNextMystery();
    }
    #endregion

    #region Woman Stranger
    public void FlirtWithWoman()
    {
        QueueNextEncounter(NodeType.EliteEnemy, EnemyDifficulty.Normal);
        Log("Flirted with stranger: elite encounter queued.");
    }

    public void Woman_Ignore()
    {
        Log("Woman: ignored (no effect).");
        QueueNextMystery();
    }
    #endregion

    #region Man in Suit
    public void WorkForSuit()
    {
        int roll = Random.Range(0, 3);
        if (roll == 0)
        {
            int credits = Random.Range(10, 21);
            ChangeMoney(credits);
            Log($"Work for suit: paid +{credits} credits.");
        }
        else if (roll == 1)
        {
            GrantRandomItem();
            Log("Work for suit: received an item.");
        }
        else
        {
            Log("Work for suit: (placeholder) map reveal would occur here.");
        }

        QueueNextEncounter(NodeType.MinorEnemy, EnemyDifficulty.Normal);
    }

    public void PickpocketSuit()
    {
        if (Random.value < 0.4f)
        {
            if (Random.value < 0.5f)
            {
                int credits = Random.Range(20, 41);
                ChangeMoney(credits);
                Log($"Pickpocket succeeded: +{credits} credits.");
            }
            else
            {
                GrantRandomItem(ItemRarity.Rare);
                Log("Pickpocket succeeded: stole a rare item.");
            }
            QueueNextMystery();
        }
        else
        {
            QueueNextEncounter(NodeType.EliteEnemy, EnemyDifficulty.Hard);
            Log("Pickpocket failed: elite enemy triggered.");
        }
    }

    public void Suit_WalkAway()
    {
        Log("Suit: walked away (no effect).");
        QueueNextMystery();
    }
    #endregion

    #region Vending Machine
    public void BuyFromVending()
    {
        ChangeMoney(-smallCreditCost);
        if (Random.value < 0.5f)
        {
            GrantRandomItem();
            Log("Vending: item dispensed.");
        }
        else
        {
            Log("Vending: nothing dispensed.");
        }
        QueueNextMystery();
    }

    public void HackVending()
    {
        ChangeMP(-hackMpCost);
        GrantRandomItem();
        Log("Vending hacked: free item obtained.");
        QueueNextMystery();
    }

    public void KickVending()
    {
        if (Random.value < 0.25f)
        {
            GrantRandomItem();
            Log("Vending kicked: jackpot! free item.");
        }
        else
        {
            Log("Vending kicked: nothing happened.");
        }
        QueueNextMystery();
    }

    public void Vending_WalkAway()
    {
        Log("Vending: walked away (no effect).");
        QueueNextMystery();
    }
    #endregion

    #region Dumpster
    public void DigDumpster()
    {
        if (Random.value < 0.5f)
        {
            GrantRandomItem(ItemRarity.Rare);
            Log("Dumpster: found rare scrap item.");
        }
        else
        {
            int dmg = Random.Range(8, 16);
            ChangeHP(-dmg);
            Log($"Dumpster: caught infection (-{dmg} HP).");
        }
        QueueNextMystery();
    }

    public void DroneDumpster()
    {
        ChangeMP(-droneMpCost);
        GrantRandomItem(ItemRarity.Rare);
        Log("Dumpster: drone retrieved rare loot safely.");
        QueueNextMystery();
    }

    public void Dumpster_WalkAway()
    {
        Log("Dumpster: walked away (no effect).");
        QueueNextMystery();
    }
    #endregion

    #region Homeless Camp
    public void ShareWithHomeless()
    {
        ChangeMoney(-smallCreditCost);
        goodKarma = true;
        Log("Shared with homeless: Good Karma gained.");
        QueueNextMystery();
    }

    public void TradeWithHomeless()
    {
        if (player == null || player.equippedPassiveItems.Count == 0)
        {
            Log("Trade failed: no items equipped to trade.");
            QueueNextMystery();
            return;
        }

        int idx = Random.Range(0, player.equippedPassiveItems.Count);
        var removedItem = player.equippedPassiveItems[idx];
        ItemRarity rarity = removedItem.rarity;
        player.equippedPassiveItems.RemoveAt(idx);
        Log($"Traded away: {removedItem.name} (Rarity: {rarity})");
        GrantRandomItem(rarity);
        QueueNextMystery();
    }

    public void StealFromHomeless()
    {
        if (Random.value < 0.5f)
        {
            GrantRandomItem();
            Log("Stole from homeless: gained loot.");
            QueueNextMystery();
        }
        else
        {
            QueueNextEncounter(NodeType.MinorEnemy, EnemyDifficulty.Normal);
            Log("Steal attempt failed: enemies alerted.");
        }
    }

    public void Homeless_WalkAway()
    {
        Log("Homeless camp: walked away (no effect).");
        QueueNextMystery();
    }
    #endregion

    #region Police Car Wreck
    public void LootPoliceCar()
    {
        GrantWeapon();
        if (Random.value < 0.5f)
        {
            QueueNextEncounter(NodeType.MinorEnemy, EnemyDifficulty.Hard);
            Log("Police car: alarm triggered, hard enemy queued.");
        }
        else
        {
            Log("Police car: no alarm triggered.");
            QueueNextMystery();
        }
    }

    public void HackPoliceCar()
    {
        ChangeMP(-hackMpCost);
        if (Random.value < 0.5f)
        {
            GrantWeapon();
            Log("Police car hacked: obtained weapon.");
        }
        else
        {
            GrantRandomItem();
            Log("Police car hacked: obtained item.");
        }
        QueueNextMystery();
    }

    public void PoliceCar_WalkAway()
    {
        Log("Police car: walked away (no effect).");
        QueueNextMystery();
    }
    #endregion
}
