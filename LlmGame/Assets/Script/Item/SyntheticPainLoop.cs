using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SyntheticPainLoop : MonoBehaviour, IPassiveItem, IDamageReaction
{
    [SerializeField] private int attackBonus = 2;
    [SerializeField] private int maxStacks = 3;

    private int currentStacks = 0;
    private Character owner;

    public void ApplyEffect(Character character)
    {
        owner = character;
    }

    public void DeApplyEffect(Character character)
    {
        if (owner == character)
        {
            character.attack -= currentStacks * attackBonus;
            currentStacks = 0;
            owner = null;
        }
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        // Not needed for this passive
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        Debug.Log(source);
        Debug.Log(target);

        if (target == null || source == null || source != owner) return;

        if (!target.HasStatusEffect(StatusEffectType.Bleed)) return;

        // Make sure this passive is only applied for the attacker (source)
        if (currentStacks >= maxStacks)
            return;

        owner.attack += attackBonus;
        currentStacks++;

        Debug.Log($"🧬 Synthetic Pain Loop: {owner.characterName} gains +{attackBonus} Attack (Total: +{currentStacks * attackBonus}) for damaging a bleeding enemy.");
    }
}
