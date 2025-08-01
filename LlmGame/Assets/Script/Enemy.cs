using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyArchetype
{
    Attacker,
    Defender,
    Boss
}

public class Enemy : Character
{
    [Header("Enemy Info")]
    public EnemyArchetype archetype;

    [Header("Action")]
    public List<string> actions;

    void Start()
    {
        TurnStatusEffect posion = new TurnStatusEffect(
            StatusEffectType.Poison,
            duration: this.characterType == CharacterType.Human ? 2 : 1,
            magnitude: 1,
            source: battleManager.player
        );

        TurnStatusEffect Radiation = new TurnStatusEffect(
            StatusEffectType.Radiation,
            duration: this.characterType == CharacterType.Human ? 2 : 1,
            magnitude: 1,
            source: battleManager.player
        );

        //this.ApplyStatusEffect(posion);
        //this.ApplyStatusEffect(Radiation);
    }

    private Character GetLowestHPTargetInTeam(Enemy user)
    {
        List<Character> allies = GetAlliesOf(user);

        Character lowest = null;
        int lowestHP = int.MaxValue;

        foreach (var ally in allies)
        {
            if (ally.IsAlive() && ally.currentHP < ally.maxHP && ally.currentHP < lowestHP)
            {
                lowestHP = ally.currentHP;
                lowest = ally;
            }
        }

        return lowest;
    }

    // Replace with your actual method to get allies of a character
    private List<Character> GetAlliesOf(Character character)
    {
        return battleManager.allCharacters.FindAll(c =>
            c.characterType == character.characterType &&
            c.IsAlive());
    }

}


