using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Map; // for NodeType / EnemyDifficulty (same as your other handlers)

public class RestSceneEventHandler : MonoBehaviour
{
    [Header("References")]
    public Player player;

    [Tooltip("Optional: used only if you want to source items from shared pools here.")]
    public List<PassiveItemData> passiveItems = new();
    public List<ArmorData> armorItems = new();
    public List<Weapon> weapons = new(); // If Weapon inherits Item, rarity is read via Item

    [Header("Progression")]
    public NodeType nextNodeAfterRest = NodeType.MinorEnemy;
    public EnemyDifficulty nextDifficultyAfterRest = EnemyDifficulty.Normal;

    [Header("Camp Config")]
    public int campSleepHealAmount = 40;  // HP restored by 'Sleep by the fire'
    public int campCookMealCost = 50;     // cost for 'Cook a meal'
    [Range(0f, 1f)] public float campCookMealMpGainPercent = 0.30f; // percent of Max MP to gain

    [Header("Bar Config")]
    public int barDrinkCost = 50;         // Buy a drink
    [Range(0f, 1f)] public float barDrinkMpGainPercent = 0.30f; // MP gained (percent of Max MP)
    public int barRoomCost = 200;         // Rent a room (full HP + full MP)
    public int barGambleStake = 100;      // Required to gamble; needs at least this amount
    [Range(0f, 1f)] public float barGambleWinRate = 0.45f; // 45%
    public int barGambleWinAmount = 200;  // On win, +200 credits
    public int barGambleLoseAmount = 100; // On loss, -100 credits
    public int barChatHealAmount = 20;    // Chat heals some HP

    [Header("Sofa Config")]
    public int sofaNapHealAmount = 40;    // Nap heals some HP
    public int sofaMinCreditsFound = 0;   // min credits from cushions
    public int sofaMaxCreditsFound = 75;  // max inclusive

    // UI listeners can show messages here
    public static System.Action<string> OnOutcome;

    /// <summary>
    /// UI reads this after an option is invoked to decide whether to advance scene.
    /// True = advance; False = stay and re-enable options.
    /// </summary>
    public static bool AllowAdvanceAfterLastAction { get; private set; } = true;

    // ===== Utility =====
    private void Log(string msg)
    {
        Debug.Log($"[Rest] {msg}", this);
        OnOutcome?.Invoke(msg);
    }

    private void BeginAction()
    {
        // Assume we should advance unless a check fails.
        AllowAdvanceAfterLastAction = true;
    }

    private void BlockAdvance(string reason)
    {
        AllowAdvanceAfterLastAction = false;
        if (!string.IsNullOrEmpty(reason))
            Log(reason);
    }

    private void ChangeMoney(int amount)
    {
        if (player == null) return;
        int before = player.money;
        player.money = Mathf.Max(0, player.money + amount);
        int after = player.money;
        string dir = amount >= 0 ? "Gained" : "Spent";
        Log($"{dir} {Mathf.Abs(amount)} credits (Money: {before} → {after})");
    }

    private void HealHP(int amount)
    {
        if (player == null) return;
        int before = player.currentHP;
        player.currentHP = Mathf.Clamp(player.currentHP + amount, 0, player.maxHP);
        Log($"Healed {player.currentHP - before} HP (HP: {before} → {player.currentHP})");
    }

    private void HealFullHP()
    {
        if (player == null) return;
        int before = player.currentHP;
        player.currentHP = player.maxHP;
        Log($"Fully healed (HP: {before} → {player.currentHP})");
    }

    private void GainMPPercent(float percent)
    {
        if (player == null) return;
        int gain = Mathf.RoundToInt(player.maxMP * Mathf.Clamp01(percent));
        int before = player.currentMP;
        player.currentMP = Mathf.Clamp(player.currentMP + gain, 0, player.maxMP);
        Log($"Gained {player.currentMP - before} MP (MP: {before} → {player.currentMP})");
    }

    private void RestoreFullMP()
    {
        if (player == null) return;
        int before = player.currentMP;
        player.currentMP = player.maxMP;
        Log($"Fully restored MP (MP: {before} → {player.currentMP})");
    }

    private bool EnsureCredits(int minAmount, string actionLabel)
    {
        if (player == null) { BlockAdvance($"{actionLabel}: No player found."); return false; }
        if (player.money < minAmount)
        {
            BlockAdvance($"{actionLabel}: Not enough credits. Need {minAmount}, have {player.money}.");
            return false;
        }
        return true;
    }

    private void QueueNext()
    {
        PlayerData.Instance?.SetNextNode(nextNodeAfterRest, nextDifficultyAfterRest);
        Log($"Next node queued: {nextNodeAfterRest} ({nextDifficultyAfterRest})");
    }

    // ===== Item Grant (Sofa Search) =====
    private void GrantRandomRareItem()
    {
        var rareCandidates = new List<ScriptableObject>();

        if (passiveItems != null && passiveItems.Count > 0)
            rareCandidates.AddRange(passiveItems.Where(p => p != null && p.rarity == ItemRarity.Rare));

        if (armorItems != null && armorItems.Count > 0)
            rareCandidates.AddRange(armorItems.Where(a => a != null && a.rarity == ItemRarity.Rare));

        if (weapons != null && weapons.Count > 0)
        {
            foreach (var w in weapons)
            {
                if (w == null) continue;
                if (w is Item wi && wi.rarity == ItemRarity.Rare)
                    rareCandidates.Add(w);
                // else: add custom rarity check if Weapon has its own rarity field
            }
        }

        if (rareCandidates.Count == 0)
        {
            Log("Sofa: no rare items available in pools.");
            return;
        }

        var pick = rareCandidates[Random.Range(0, rareCandidates.Count)];
        var pdata = PlayerData.Instance;

        if (pdata == null || player == null)
        {
            Log("Sofa: cannot grant item (missing PlayerData or Player).");
            return;
        }

        if (pick is PassiveItemData p)
        {
            if (pdata.equippedPassiveItems == null)
                pdata.equippedPassiveItems = new List<PassiveItemData>();
            if (!pdata.equippedPassiveItems.Contains(p))
                pdata.equippedPassiveItems.Add(p);
            Log($"Sofa: found rare passive item — {p.itemName}");
        }
        else if (pick is ArmorData a)
        {
            if (pdata.inventoryArmors == null)
                pdata.inventoryArmors = new List<ArmorData>();
            if (!pdata.inventoryArmors.Contains(a))
                pdata.inventoryArmors.Add(a);
            Log($"Sofa: found rare armor — {a.armorName}");
        }
        else if (pick is Weapon w)
        {
            if (pdata.inventoryItems == null)
                pdata.inventoryItems = new List<Item>();
            if (!pdata.inventoryItems.Contains(w))
                pdata.inventoryItems.Add(w);
            Log($"Sofa: found rare weapon — {w.name}");
        }
        else if (pick is Item i)
        {
            if (pdata.inventoryItems == null)
                pdata.inventoryItems = new List<Item>();
            pdata.inventoryItems.Add(i);
            Log($"Sofa: found rare item — {i.itemName}");
        }

        pdata.LoadPlayer(player);
    }

    // ======================
    // CAMP
    // ======================

    public void Camp_SleepByFire()
    {
        BeginAction();
        HealHP(campSleepHealAmount);
        QueueNext();
    }

    public void Camp_CookMeal()
    {
        BeginAction();
        if (!EnsureCredits(campCookMealCost, "Cook a meal")) return; // blocks advance
        ChangeMoney(-campCookMealCost);
        HealFullHP();
        GainMPPercent(campCookMealMpGainPercent);
        QueueNext();
    }

    // ======================
    // BAR
    // ======================

    public void Bar_BuyDrink()
    {
        BeginAction();
        if (!EnsureCredits(barDrinkCost, "Buy a drink")) return; // blocks advance
        ChangeMoney(-barDrinkCost);
        HealFullHP();
        GainMPPercent(barDrinkMpGainPercent);
        QueueNext();
    }

    public void Bar_RentRoom()
    {
        BeginAction();
        if (!EnsureCredits(barRoomCost, "Rent a room")) return; // blocks advance
        ChangeMoney(-barRoomCost);
        HealFullHP();
        RestoreFullMP();
        QueueNext();
    }

    public void Bar_Gamble()
    {
        BeginAction();
        if (!EnsureCredits(barGambleStake, "Gamble")) return; // blocks advance

        bool win = Random.value < barGambleWinRate;
        if (win)
        {
            ChangeMoney(+barGambleWinAmount);
            Log("Gamble: You won!");
        }
        else
        {
            ChangeMoney(-barGambleLoseAmount);
            Log("Gamble: You lost...");
        }
        QueueNext();
    }

    public void Bar_ChatWithLocals()
    {
        BeginAction();
        HealHP(barChatHealAmount);
        Log("You picked up some stories and feel a bit better.");
        QueueNext();
    }

    // ======================
    // SOFA
    // ======================

    public void Sofa_TakeNap()
    {
        BeginAction();
        HealHP(sofaNapHealAmount);
        QueueNext();
    }

    public void Sofa_SearchCushions()
    {
        BeginAction();
        GrantRandomRareItem();

        int credits = Random.Range(sofaMinCreditsFound, sofaMaxCreditsFound + 1);
        if (credits != 0) ChangeMoney(credits);

        QueueNext();
    }
}
