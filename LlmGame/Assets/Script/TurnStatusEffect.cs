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

    // Marks effects that should never expire or be removed
    public bool isPermanent = false;

    public TurnStatusEffect(StatusEffectType type, int duration, int magnitude, Character source = null, bool isPermanent = false)
    {
        this.effectType = type;
        this.remainingTurns = duration;
        this.magnitude = magnitude;
        this.source = source;
        this.isPermanent = isPermanent = false;
    }
}

