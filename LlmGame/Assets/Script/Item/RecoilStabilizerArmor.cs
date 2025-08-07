using UnityEngine;

public class RecoilStabilizerArmor : MonoBehaviour, IPassiveItem, IDamageReaction
{
    private Character owner;
    private const int defenseBonus = 10;
    private const int damageBonus = 10;

    public void ApplyEffect(Character character)
    {
        owner = character;
        Weapon weapon = GetWeapon();
        if (weapon != null && weapon.weaponType == WeaponType.Ranged_Weapon)
        {
            character.defense += defenseBonus;
            Debug.Log($"🦴 Recoil Stabilizer equipped: +{defenseBonus} Defense when using ranged weapons.");
        }
    }

    private Weapon GetWeapon()
    {
        if (owner is Player p)
            return p.rightHandWeapon ?? p.leftHandWeapon;
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
}
