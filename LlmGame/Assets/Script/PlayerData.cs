using System.Collections.Generic;
using UnityEngine;
using Map;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public NodeType nextNodeType;
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
    public Weapon leftHandWeapon;
    public Weapon rightHandWeapon;
    public List<PassiveItemData> equippedPassiveItems = new List<PassiveItemData>();

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
        leftHandWeapon = config ? config.leftHandWeapon : null;
        rightHandWeapon = config ? config.rightHandWeapon : null;
        equippedPassiveItems = config ? new List<PassiveItemData>(config.startingPassives) : new List<PassiveItemData>();

        initialized = true;
    }

    /// <summary>
    /// Copy runtime Player → PlayerData (in-memory save)
    /// </summary>
    public void SavePlayer(Player player)
    {
        if (player == null) return;
        foreach (var behavior in player.runtimePassiveBehaviors)
        {
            if (behavior is IPassiveItem item)
                item.DeApplyEffect(player);
        }

        attack = player.attack;
        defense = player.defense;
        focus = player.focus;
        maxHP = player.maxHP;
        maxMP = player.maxMP;
        speed = player.speed;
        maxShield = player.maxShield;

        currentHP = player.currentHP;
        currentMP = player.currentMP;
        currentShield = player.currentshield; // note: Player field name uses 'currentshield' per your example
        money = player.money;

        inventoryItems = new List<Item>(player.inventoryItems);
        leftHandWeapon = player.leftHandWeapon;
        rightHandWeapon = player.rightHandWeapon;
        equippedPassiveItems = new List<PassiveItemData>(player.equippedPassiveItems);

        initialized = true;

        foreach (var behavior in player.runtimePassiveBehaviors)
        {
            if (behavior is IPassiveItem item)
                item.ApplyEffect(player);
        }
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

        player.attack = attack;
        player.defense = defense;
        player.focus = focus;
        player.maxHP = maxHP;
        player.maxMP = maxMP;
        player.speed = speed;
        player.maxShield = maxShield;

        player.currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        player.currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        player.currentshield = Mathf.Clamp(currentShield, 0, maxShield);
        player.money = money;

        player.inventoryItems = new List<Item>(inventoryItems);
        player.leftHandWeapon = leftHandWeapon;
        player.rightHandWeapon = rightHandWeapon;
        player.equippedPassiveItems = new List<PassiveItemData>(equippedPassiveItems);
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
}
