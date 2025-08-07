using UnityEngine;

public class StabilizerGloves : MonoBehaviour, IPassiveItem, IDamageReaction
{
    private Character owner;
    private const int damageBonus = 5;

    public void ApplyEffect(Character character)
    {
        owner = character;
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        if (source != owner) return;
        Weapon weapon = owner.rightHandWeapon ?? owner.leftHandWeapon;
        if (weapon != null && weapon.weaponType == WeaponType.Ranged_Weapon)
        {
            damage += damageBonus;
        }
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage) { }
}
