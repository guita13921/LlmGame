using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalVisor : MonoBehaviour, IPassiveItem
{
    private const int bonusRangedDamage = 10;
    private bool applied = false;

    public void ApplyEffect(Character character)
    {
        // Only apply if using a ranged weapon
        if (character.rightHandWeapon != null && character.rightHandWeapon.weaponType == WeaponType.Ranged_Weapon)
        {
            character.attack += bonusRangedDamage;
            character.bonusAttack += bonusRangedDamage;
            Debug.Log($"🔭 Tactical Visor equipped: +{bonusRangedDamage} Ranged Weapon Damage applied to {character.characterName}");
            applied = true;
        }
    }

    public void DeApplyEffect(Character character)
    {
        if (applied)
        {
            character.attack -= bonusRangedDamage;
            character.bonusAttack -= bonusRangedDamage;
            applied = false;
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
