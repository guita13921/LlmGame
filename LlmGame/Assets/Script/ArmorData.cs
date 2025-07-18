using UnityEngine;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Character/Armor")]
public class ArmorData : ScriptableObject
{
    public string armorName;
    public int defense; // general defense
    public float reducePotentialDamage; // e.g., 0.2f = 20%
    public float reduceFeasibility; // e.g., 0.1f = 10%
    public int armorHealth;
    public string description;
}
