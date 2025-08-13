using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CombatGrips : MonoBehaviour, IPassiveItem
{
    private const float meleeDamageMultiplier = 0.10f;
    private int bonusAttack = 0;

    public void ApplyEffect(Character character)
    {
        Weapon weapon = character.rightHandWeapon;

        if (weapon != null && weapon.weaponType == WeaponType.Melee_Weapon)
        {
            bonusAttack = Mathf.RoundToInt(character.attack * meleeDamageMultiplier);
            character.attack += bonusAttack;

            Debug.Log($"🥊 Combat Grips equipped: +{bonusAttack} Attack (+10%) for melee weapon.");
        }
        else
        {
            bonusAttack = 0;
            Debug.Log("🥊 Combat Grips equipped, but no melee weapon detected.");
        }
    }

    public void DeApplyEffect(Character character)
    {
        if (bonusAttack != 0)
        {
            character.attack -= bonusAttack;
            bonusAttack = 0;
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
