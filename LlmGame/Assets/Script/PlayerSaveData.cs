using System;

[Serializable]
public class PlayerSaveData
{
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

    // TODO: If your Item/Weapon/Passive are ScriptableObjects, save their IDs/paths here.
    // public string leftWeaponId;
    // public string rightWeaponId;
    // public string[] inventoryIds;
    // public string[] passiveIds;
}
