using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Map;

/// <summary>
/// Contains concrete handlers for all MysteryScene events such as the
/// noodle cart, spooky alleyway, strangers, etc. Methods here can be
/// wired to <see cref="MysteryEvent.Option.onSelected"/> via the inspector.
/// </summary>
public class MysterySceneEventHandler : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public TreasureSystem treasureSystem;
    public StoreSystem storeSystem;

    [Header("Config")]
    // Updated per design: 50 coin standard small credit cost
    public int smallCreditCost = 50;
    public int droneMpCost = 20;
    public int hackMpCost = 20; // design says reduce 20 MP for hack
    public PassiveItemData msgNoodleItem;
    public List<Weapon> weaponPool = new();

    // Track karma from the homeless camp event.
    public bool goodKarma = false;

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

    // New helper: pick random from provided rarities
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
    #endregion

    #region Noodle Cart
    // Order a bowl (50 coin) → get passive item : MSG noodle.
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
    }

    // Order a bowl and Offer to help clean up (No cost) → Heal 25hp
    public void OrderAndClean()
    {
        ChangeHP(25);
        Log("Helped clean up the stall.");
    }

    // Refuse and walk away → Nothing happens.
    public void Noodle_WalkAway() { Log("Noodle Cart: walked away (no effect)."); }
    #endregion

    #region Spooky Alleyway
    // Investigate → 50% loot (Common or Rare); 50%: surprise enemy next encounter (MinorEnemy Normal)
    public void InvestigateAlley()
    {
        if (Random.value < 0.5f)
        {
            Log("Alley: found loot.");
            GrantRandomItemFromRarities(ItemRarity.Common, ItemRarity.Rare);
        }
        else
        {
            Log("Alley: ambush flagged for next encounter.");
            QueueNextEncounter(NodeType.MinorEnemy, EnemyDifficulty.Normal);
        }
    }

    // Throw a light drone (reduce 20MP) → Reveal hidden stash (random credits or item). Always yields something.
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
    }

    // Ignore and pass by → Nothing happens.
    public void Alley_Ignore() { Log("Alley: ignored (no effect)."); }
    #endregion

    #region Cloaked Stranger (ชายที่ใส่ผ้าคลุม)
    // Hear him out → 50% random reward (item/credits) or 50% random curse (lose HP).
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
    }

    // Offer credits for information (50 coin) → Reveal one common item.
    public void PayCloakedMan()
    {
        ChangeMoney(-smallCreditCost);
        Log("Paid cloaked man for information.");
        GrantRandomItem(ItemRarity.Common);
    }

    // Intimidate him → 50% success: gain stolen loot; 50% fail: lose HP.
    public void IntimidateCloakedMan()
    {
        if (Random.value < 0.5f)
        {
            int credits = Random.Range(10, 21);
            ChangeMoney(credits);
            GrantRandomItem(ItemRarity.Rare);
            Log($"Intimidation succeeded: stole +{credits} credits and rare loot.");
        }
        else
        {
            int dmg = Random.Range(10, 21);
            ChangeHP(-dmg);
            Log($"Intimidation failed: took -{dmg} HP.");
        }
    }

    // Walk away → Nothing
    public void Cloaked_WalkAway() { Log("Cloaked stranger: walked away (no effect)."); }
    #endregion

    #region Woman Stranger (ผญ)
    // Flirt → encounter elite enemy
    public void FlirtWithWoman()
    {
        QueueNextEncounter(NodeType.EliteEnemy, EnemyDifficulty.Normal);
        Log("Flirted with stranger: elite encounter queued.");
    }

    // Ignore and pass by → Nothing happens.
    public void Woman_Ignore() { Log("Woman: ignored (no effect)."); }
    #endregion

    #region Man in Suit (ชายใส่สูท)
    // Offer to work for him → Random job reward (credits, item, or map reveal) and encounter a minor enemy.
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
            // TODO: map reveal feature
            Log("Work for suit: (placeholder) map reveal would occur here.");
            // e.g., MapSystem.Instance?.RevealNearby();
        }

        QueueNextEncounter(NodeType.MinorEnemy, EnemyDifficulty.Normal);
    }

    // Pickpocket him (random) → 40% Gain better credits/passive item; 60% trigger elite enemy.
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
        }
        else
        {
            QueueNextEncounter(NodeType.EliteEnemy, EnemyDifficulty.Hard);
            Log("Pickpocket failed: elite enemy triggered.");
        }
    }

    // Walk away → Nothing
    public void Suit_WalkAway() { Log("Suit: walked away (no effect)."); }
    #endregion

    #region Vending Machine (ตู้กด)
    // Buy an item (50 coin) → Gain a random consumable or nothing.
    // (We don't have consumables here; using random item or nothing.)
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
    }

    // Hack it (reduce 20 MP) → Free item.
    public void HackVending()
    {
        ChangeMP(-hackMpCost);
        GrantRandomItem();
        Log("Vending hacked: free item obtained.");
    }

    // Kick it (Random outcome) → 25% Free item, 75% nothing.
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
    }

    // Walk away → Nothing
    public void Vending_WalkAway() { Log("Vending: walked away (no effect)."); }
    #endregion

    #region Dumpster (กองขยะ)
    // Dig through → 50% rare scrap item; 50% catch infection (-HP).
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
    }

    // Send drone (reduce 20 MP) → Safely retrieve loot.
    public void DroneDumpster()
    {
        ChangeMP(-droneMpCost);
        GrantRandomItem(ItemRarity.Rare);
        Log("Dumpster: drone retrieved rare loot safely.");
    }

    // Walk away → Nothing
    public void Dumpster_WalkAway() { Log("Dumpster: walked away (no effect)."); }
    #endregion

    #region Homeless Camp (แคมป์คนจรจัด)
    // Share food/credits (50 Pay) → Gain “Good Karma”
    public void ShareWithHomeless()
    {
        ChangeMoney(-smallCreditCost);
        goodKarma = true;
        Log("Shared with homeless: Good Karma gained.");
    }

    // Trade items → Exchange a random item for a different random item (same rarity)
    public void TradeWithHomeless()
    {
        if (player == null || player.equippedPassiveItems.Count == 0)
        {
            Log("Trade failed: no items equipped to trade.");
            return;
        }

        int idx = Random.Range(0, player.equippedPassiveItems.Count);
        var removedItem = player.equippedPassiveItems[idx];
        ItemRarity rarity = removedItem.rarity;
        player.equippedPassiveItems.RemoveAt(idx);
        Log($"Traded away: {removedItem.name} (Rarity: {rarity})");
        GrantRandomItem(rarity);
    }

    // Steal from them → 50% Gain loot , 50% encounter enemies.
    public void StealFromHomeless()
    {
        if (Random.value < 0.5f)
        {
            GrantRandomItem();
            Log("Stole from homeless: gained loot.");
        }
        else
        {
            QueueNextEncounter(NodeType.MinorEnemy, EnemyDifficulty.Normal);
            Log("Steal attempt failed: enemies alerted.");
        }
    }

    // Walk away → Nothing
    public void Homeless_WalkAway() { Log("Homeless camp: walked away (no effect)."); }
    #endregion

    #region Police Car Wreck (ซากรถตำรวจ)
    // Loot the trunk → Gain weapon, but 50% trigger security alarm → encounter enemy
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
        }
    }

    // Hack onboard computer (reduce 20 MP) → Gain weapon or passive item.
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
    }

    // Walk away → Nothing
    public void PoliceCar_WalkAway() { Log("Police car: walked away (no effect)."); }
    #endregion
}
