using UnityEngine;

public class RecoilStabilizerArmor : MonoBehaviour, IPassiveItem, IDamageReaction
{
    private Character owner;
    private const int defenseBonus = 10;
    private const int damageBonus = 10;
    private bool bonusApplied = false;

    public void ApplyEffect(Character character)
    {
        owner = character;
        Weapon weapon = GetWeapon();
        if (weapon != null && weapon.weaponType == WeaponType.Ranged_Weapon)
        {
            character.defense += defenseBonus;
            character.bonusDefense += defenseBonus;
            Debug.Log($"🦴 Recoil Stabilizer equipped: +{defenseBonus} Defense when using ranged weapons.");
            bonusApplied = true;
        }
    }

    private Weapon GetWeapon()
    {
        if (owner is Player p)
            return p.equippedWeapon;
        return null;
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        if (source != owner) return;
        Weapon weapon = GetWeapon();
        if (weapon != null && weapon.weaponType == WeaponType.Ranged_Weapon)
        {
            damage += damageBonus;
        }
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage) { }

    public void DeApplyEffect(Character character)
    {
        if (character == owner && bonusApplied)
        {
            character.defense -= defenseBonus;
            character.bonusDefense -= defenseBonus;
            bonusApplied = false;
        }
        if (character == owner)
            owner = null;
    }
}
