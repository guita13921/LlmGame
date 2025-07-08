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
}


