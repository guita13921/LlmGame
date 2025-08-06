using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoilStabilizers : MonoBehaviour, IPassiveItem, IPossibilityModifier
{
    private const float rangedDamageMultiplier = 0.15f;
    private const float criticalChanceBonus = 0.10f;

    public void ApplyEffect(Character character)
    {
        Weapon weapon = character.rightHandWeapon;

        if (weapon != null && weapon.weaponType == WeaponType.Ranged_Weapon)
        {
            // Apply damage bonus by increasing attack stat
            int bonusAttack = Mathf.RoundToInt(character.attack * rangedDamageMultiplier);
            character.attack += bonusAttack;

            Debug.Log($"🦿 Recoil Stabilizers equipped: +{bonusAttack} Attack (+15%) for ranged weapon.");
        }
        else
        {
            Debug.Log("🦿 Recoil Stabilizers equipped, but no ranged weapon detected.");
        }
    }

    public void ModifyChances(PossibilityPool pool)
    {
        pool.AddModifier(StatusChanceType.Critical, criticalChanceBonus);
        Debug.Log("🦿 Recoil Stabilizers: +10% Critical Chance added.");
    }

    public void ModifyCritical(Character character, PossibilityPool pool)
    {
        throw new System.NotImplementedException();
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
