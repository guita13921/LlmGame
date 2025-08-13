using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PrecisionArms : MonoBehaviour, IPassiveItem, IPossibilityModifier
{
    private const float critChanceBonus = 0.10f;

    public void ApplyEffect(Character character)
    {
        Debug.Log("🦾 Precision Arms equipped: +10% Critical Chance");
    }

    public void DeApplyEffect(Character character) { }

    public void ModifyChances(PossibilityPool pool)
    {
        pool.AddModifier(StatusChanceType.Critical, critChanceBonus);
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
