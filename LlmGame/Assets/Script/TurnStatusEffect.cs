using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TurnStatusEffect
{
    public StatusEffectType effectType;
    public int remainingTurns;
    public int magnitude;

    public Character source; // ✅ New field

    public bool isApplied = false;

    public TurnStatusEffect(StatusEffectType type, int duration, int magnitude, Character source = null)
    {
        this.effectType = type;
        this.remainingTurns = duration;
        this.magnitude = magnitude;
        this.source = source;
    }
}

