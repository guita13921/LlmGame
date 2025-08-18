using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultPlayerConfig", menuName = "Configs/Default Player Config")]
public class DefaultPlayerConfig : ScriptableObject
{
    [Header("Stats")]
    public int attack = 10;
    public int defense = 5;
    public int focus = 3;
    public int maxHP = 100;
    public int maxMP = 50;
    public int speed = 5;
    public int maxShield = 20;

    [Header("Start State")]
    public int startMoney = 0;
    public int startShield = 0; // usually 0
    public bool startWithFullHP = true;
    public bool startWithFullMP = true;

    [Header("Starting Inventory & Equipment")]
    public List<Item> startingInventory = new List<Item>();

    // Single starting weapon
    public Weapon startingWeapon;

    public List<PassiveItemData> startingPassives = new List<PassiveItemData>();

    [Tooltip("Armor that the player will start with in their backpack (not equipped automatically).")]
    public List<ArmorData> startingArmors = new List<ArmorData>(); // ✅ NEW FIELD
}
