using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TurnStatusEffect
{
    public StatusEffectType effectType;
    public int remainingTurns;
    public int magnitude; // Use this for stat debuffs
    public bool isApplied = false;

    public TurnStatusEffect(StatusEffectType type, int turns, int value = 0)
    {
        effectType = type;
        remainingTurns = turns;
        magnitude = value;
    }
}
