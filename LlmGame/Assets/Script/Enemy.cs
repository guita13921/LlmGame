using System.Collections;
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
    public EnemyArchetype Archetype;

    public Enemy(string name, string description, int attack, int defense, int focus, int maxHP, int maxMP, int speed, EnemyArchetype archetype)
        : base(name, description, attack, defense, focus, maxHP, maxMP, speed)
    {
        Archetype = archetype;
    }
}
