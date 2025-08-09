using System.Collections.Generic;
using UnityEngine;
using Map;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public NodeType nextNodeType;

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

    private bool initialized = false;

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

    public void SavePlayer(Player player)
    {
        attack = player.attack;
        defense = player.defense;
        focus = player.focus;
        maxHP = player.maxHP;
        maxMP = player.maxMP;
        speed = player.speed;
        maxShield = player.maxShield;
        currentHP = player.currentHP;
        currentMP = player.currentMP;
        currentShield = player.currentshield;
        money = player.money;

        inventoryItems = new List<Item>(player.inventoryItems);
        leftHandWeapon = player.leftHandWeapon;
        rightHandWeapon = player.rightHandWeapon;
        equippedPassiveItems = new List<PassiveItemData>(player.equippedPassiveItems);

        initialized = true;
    }

    public void LoadPlayer(Player player)
    {
        if (!initialized)
        {
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
        player.currentHP = currentHP;
        player.currentMP = currentMP;
        player.currentshield = currentShield;
        player.money = money;

        player.inventoryItems = new List<Item>(inventoryItems);
        player.leftHandWeapon = leftHandWeapon;
        player.rightHandWeapon = rightHandWeapon;
        player.equippedPassiveItems = new List<PassiveItemData>(equippedPassiveItems);
    }

    public void SetNextNode(NodeType type)
    {
        nextNodeType = type;
    }
}
