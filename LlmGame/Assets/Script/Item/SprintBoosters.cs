using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SprintBoosters : MonoBehaviour, IPassiveItem
{
    private const int speedBonus = 2;

    public void ApplyEffect(Character character)
    {
        character.speed += speedBonus;
        Debug.Log($"🦿 Sprint Boosters equipped: +{speedBonus} Speed to {character.characterName}");
    }

    public void DeApplyEffect(Character character)
    {
        character.speed -= speedBonus;
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
