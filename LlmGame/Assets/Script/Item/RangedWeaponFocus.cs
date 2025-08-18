using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RangedWeaponFocus : MonoBehaviour, IPassiveItem
{
    [SerializeField] private float critBonus = 0.15f; // +15% Critical Chance

    public void ApplyEffect(Character character)
    {
        // Ensure it's a Player character
        if (character is Player player)
        {
            Weapon weapon = player.equippedWeapon;

            if (weapon != null && weapon.weaponType == WeaponType.Ranged_Weapon)
            {
                player.possibilityPool.AddModifier(StatusChanceType.Critical, critBonus);
                Debug.Log($"[RangedWeaponFocus] Applied +{critBonus * 100}% Critical Chance to {player.characterName} (Ranged Weapon Equipped).");
            }
        }
    }

    public void DeApplyEffect(Character character)
    {
        if (character is Player player)
        {
            player.possibilityPool.AddModifier(StatusChanceType.Critical, -critBonus);
        }
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        throw new System.NotImplementedException();
    }
}
