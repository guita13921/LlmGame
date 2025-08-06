using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BleedingIncreasingCoating : MonoBehaviour, IPassiveItem, IPossibilityModifier
{
    public float bleedChanceBonus = 0.5f;

    public void ApplyEffect(Character character)
    {
        // Optional: for visual feedback or initial setup
    }

    public void ModifyChances(PossibilityPool pool)
    {
        pool.AddModifier(StatusChanceType.Bleed, bleedChanceBonus);
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


