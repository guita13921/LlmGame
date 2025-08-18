using System.Collections.Generic;
using UnityEngine;
using Map;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    [SerializeField] public NodeType nextNodeType;
    public EnemyDifficulty nextEnemyDifficulty;

    // Basic stats
    public int attack;
    public int defense;
    public int focus;
    public int maxHP;
    public int maxMP;
    public int speed;
    public int maxShield;
    public int currentHP;
    public int currentMP;
    public int currentShield;
    public int money;

    // Inventory and equipment
    public List<Item> inventoryItems = new List<Item>();
    public List<ArmorData> inventoryArmors = new List<ArmorData>();
    public Weapon equippedWeapon;
    public List<PassiveItemData> equippedPassiveItems = new List<PassiveItemData>();

    // Tracks which passive item effects have already been applied to avoid stacking
    public HashSet<string> appliedPassiveItemTypes = new HashSet<string>();

    // Equipped armor per body part
    public List<EquippedArmorEntry> equippedArmors = new List<EquippedArmorEntry>();

    [SerializeField] private bool initialized = false;
    public bool Initialized => initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Initialize a brand-new game using a ScriptableObject configuration.
    /// Call this from your main menu when the player chooses "New Game".
    /// </summary>
    public void NewGame(DefaultPlayerConfig config)
    {
        if (config == null)
        {
            Debug.LogWarning("NewGame called with null DefaultPlayerConfig. Using zero/defaults.");
        }

        attack = config ? config.attack : 0;
        defense = config ? config.defense : 0;
        focus = config ? config.focus : 0;
        maxHP = config ? config.maxHP : 0;
        maxMP = config ? config.maxMP : 0;
        speed = config ? config.speed : 0;
        maxShield = config ? config.maxShield : 0;

        // Current values
        currentHP = config ? (config.startWithFullHP ? maxHP : Mathf.Min(currentHP, maxHP)) : 0;
        currentMP = config ? (config.startWithFullMP ? maxMP : Mathf.Min(currentMP, maxMP)) : 0;
        currentShield = config ? Mathf.Clamp(config.startShield, 0, maxShield) : 0;
        money = config ? config.startMoney : 0;

        // Inventory / equipment
        inventoryItems = config ? new List<Item>(config.startingInventory) : new List<Item>();
        inventoryArmors = config ? new List<ArmorData>(config.startingArmors) : new List<ArmorData>(); // ✅ Properly load starting armor
        equippedWeapon = config ? config.startingWeapon : null;
        equippedPassiveItems = config ? new List<PassiveItemData>(config.startingPassives) : new List<PassiveItemData>();

        equippedArmors = new List<EquippedArmorEntry>(); // remains empty unless equipped manually

        // Clear any previously applied passive effects for a fresh game
        appliedPassiveItemTypes = new HashSet<string>();

        initialized = true;
    }

    /// <summary>
    /// Copy runtime Player → PlayerData (in-memory save)
    /// </summary>
    public void SavePlayer(Player player)
    {
        //Debug.Log("SavePlayer");
        if (player == null) return;

        attack = player.attack - player.bonusAttack;
        defense = player.defense - player.bonusDefense;
        focus = player.focus - player.bonusFocus;
        maxHP = player.maxHP - player.bonusMaxHP;
        maxMP = player.maxMP - player.bonusMaxMP;
        speed = player.speed - player.bonusSpeed;
        maxShield = player.maxShield - player.bonusMaxShield;

        currentHP = Mathf.Clamp(player.currentHP, 0, maxHP);
        currentMP = Mathf.Clamp(player.currentMP, 0, maxMP);
        currentShield = Mathf.Clamp(player.currentshield, 0, maxShield);
        money = player.money;

        inventoryItems = new List<Item>(player.inventoryItems);
        inventoryArmors = new List<ArmorData>(player.inventoryArmors);
        equippedWeapon = player.equippedWeapon;

        // Save equipped armors from body parts
        equippedArmors = new List<EquippedArmorEntry>();
        foreach (var part in player.bodyParts)
        {
            if (part != null && part.equippedArmor != null)
            {
                equippedArmors.Add(new EquippedArmorEntry
                {
                    bodyPartType = part.type,
                    equippedArmor = part.equippedArmor
                });
            }
        }

        initialized = true;
    }

    /// <summary>
    /// Copy PlayerData → runtime Player
    /// </summary>
    public void LoadPlayer(Player player)
    {
        if (player == null) return;

        if (!initialized)
        {
            // First-time: use the Player prefab's current Inspector values as the baseline.
            SavePlayer(player);
            return;
        }

        //Debug.Log("LoadPlayer");

        player.attack = attack;
        player.defense = defense;
        player.focus = focus;
        player.maxHP = maxHP;
        player.maxMP = maxMP;
        player.speed = speed;
        player.maxShield = maxShield;

        player.bonusAttack = 0;
        player.bonusDefense = 0;
        player.bonusFocus = 0;
        player.bonusMaxHP = 0;
        player.bonusMaxMP = 0;
        player.bonusSpeed = 0;
        player.bonusMaxShield = 0;

        player.currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        player.currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        player.currentshield = Mathf.Clamp(currentShield, 0, maxShield);
        player.money = money;

        player.inventoryItems = new List<Item>(inventoryItems);
        player.inventoryArmors = new List<ArmorData>(inventoryArmors);
        player.equippedWeapon = equippedWeapon;

        // Load equipped armor into body parts and runtime map
        player.equippedArmorByPart = new Dictionary<BodyPartType, ArmorData>();
        foreach (var part in player.bodyParts)
        {
            if (part == null) continue;

            var match = equippedArmors.Find(e => e.bodyPartType == part.type);
            part.equippedArmor = match != null ? match.equippedArmor : null;

            if (part.equippedArmor != null)
            {
                player.equippedArmorByPart[part.type] = part.equippedArmor;
            }
        }

        // ✅ Re-apply passive and armor effects after loading
    }


    /// <summary>
    /// Convenience method if you want Player to just "do the right thing".
    /// If PlayerData isn't initialized yet, it saves Player's current Inspector values into memory.
    /// Otherwise, it overwrites Player with the saved values.
    /// </summary>
    public void LoadOrInitPlayer(Player player)
    {
        if (!initialized)
        {
            SavePlayer(player);
        }
        else
        {
            LoadPlayer(player);
        }
    }

    public void SetNextNode(NodeType type, EnemyDifficulty difficulty)
    {
        nextNodeType = type;
        nextEnemyDifficulty = difficulty;
    }

    // === Support Class for Equipped Armor Mapping ===
    [System.Serializable]
    public class EquippedArmorEntry
    {
        public BodyPartType bodyPartType;
        public ArmorData equippedArmor;
    }
}
