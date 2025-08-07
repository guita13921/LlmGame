using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalVisor : MonoBehaviour, IPassiveItem
{
    private const int bonusRangedDamage = 10;

    public void ApplyEffect(Character character)
    {
        // Only apply if using a ranged weapon
        if (character.rightHandWeapon != null && character.rightHandWeapon.weaponType == WeaponType.Ranged_Weapon)
        {
            character.attack += bonusRangedDamage;
            Debug.Log($"🔭 Tactical Visor equipped: +{bonusRangedDamage} Ranged Weapon Damage applied to {character.characterName}");
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
