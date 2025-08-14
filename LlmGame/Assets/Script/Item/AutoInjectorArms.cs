using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoInjectorArms : MonoBehaviour, IPassiveItem, ITurnListener
{
    private Character owner;
    private int turnCounter = 1;

    public int turnsPerInjection = 2;       // every 2 turns
    public int buffAmount = 3;              // +3 stat
    public int buffDuration = 3;            // 3 turns

    public void ApplyEffect(Character character)
    {
        owner = character;
        Debug.Log("💉 Auto-Injector Arms equipped: will inject buffs every 2 turns.");
    }

    public void DeApplyEffect(Character character)
    {
        if (owner == character)
        {
            owner = null;
            turnCounter = 0;
        }
    }

    public void OnTurnStart(Character character)
    {
        if (character != owner) return;

        turnCounter++;

        if (turnCounter >= turnsPerInjection)
        {
            turnCounter = 0;

            // Choose random buff type
            var buffTypes = new List<StatusEffectType>
            {
                StatusEffectType.AttackUp,
                StatusEffectType.DefenseUp,
                StatusEffectType.SpeedUp
            };

            var chosenBuff = buffTypes[Random.Range(0, buffTypes.Count)];

            var effect = new TurnStatusEffect(chosenBuff, buffDuration, buffAmount)
            {
                source = owner
            };

            owner.ApplyStatusEffect(effect);

            Debug.Log($"💉 Auto-Injector Arms applied random buff: {chosenBuff} (+{buffAmount}) for {buffDuration} turns.");
        }
    }

    public void OnTurnEnd(Character character)
    {
        // Optional: logic if you want to track at end of turn
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
