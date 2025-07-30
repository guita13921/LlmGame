using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AutoRepairGel : MonoBehaviour, IPassiveItem, ITurnListener
{
    private int turnCounter = 0;

    public void ApplyEffect(Character character)
    {
        // No immediate effect
    }

    public void OnTurnStart(Character character)
    {
        // Not needed
    }

    public void OnTurnEnd(Character character)
    {
        turnCounter++;

        if (turnCounter >= 1)
        {
            int healAmount = Mathf.RoundToInt(character.maxHP * 0.05f);
            character.currentHP = Mathf.Min(character.currentHP + healAmount, character.maxHP);

            Debug.Log($"{character.characterName} healed {healAmount} HP from Auto-Repair Gel.");

            turnCounter = 0; // Reset cycle
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
