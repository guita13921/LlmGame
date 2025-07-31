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
        TurnStatusEffect bleed = new TurnStatusEffect(
            StatusEffectType.Bleed,
            duration: this.characterType == CharacterType.Human ? 2 : 1,
            magnitude: 1,
            source: battleManager.player
        );

        this.ApplyStatusEffect(bleed);
    }
}


