using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroStabilizedFeet : MonoBehaviour, IPassiveItem, IStatusEffectListener
{
    [Tooltip("Status effects this armor blocks completely.")]
    public List<StatusEffectType> blockedEffects = new List<StatusEffectType> { StatusEffectType.Stun };

    public void ApplyEffect(Character character)
    {
        Debug.Log("🌀 Gyro-Stabilized Feet equipped: Blocking " + string.Join(", ", blockedEffects));
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        throw new System.NotImplementedException();
    }

    public void OnBleedDamageDealt(Character target, int damage, Character source)
    {
        throw new System.NotImplementedException();
    }

    public bool ShouldBlockStatus(Character character, TurnStatusEffect effect)
    {
        return blockedEffects.Contains(effect.effectType);
    }

    public bool ShouldSpreadBleed(Character character)
    {
        throw new System.NotImplementedException();
    }
}

