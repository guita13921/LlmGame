using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PainConverter : MonoBehaviour, IPassiveItem, IDamageReaction, ITurnListener
{
    public bool healedThisTurn = false;

    void Start()
    {
        healedThisTurn = false;
    }

    public void ApplyEffect(Character character)
    {
        // No immediate effect
    }

    public void DeApplyEffect(Character character)
    {
        healedThisTurn = false;
    }

    public void OnTurnStart(Character character)
    {
        healedThisTurn = false; // Reset for new turn
    }

    public void OnTurnEnd(Character character) { }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        // Do nothing before
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        Debug.Log("PainConverter");
        Debug.Log(healedThisTurn);
        Debug.Log(finalDamage);
        if (!healedThisTurn && finalDamage > 0)
        {
            target.currentHP = Mathf.Min(target.currentHP + 5, target.maxHP);
            healedThisTurn = true;

            Debug.Log($"{target.characterName} healed 5 HP from Pain Converter.");
        }
    }
}
