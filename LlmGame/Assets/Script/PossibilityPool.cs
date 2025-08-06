using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PossibilityPool
{
    private Dictionary<StatusChanceType, float> baseChances = new();
    private Dictionary<StatusChanceType, float> modifiers = new();
    private float critMultiplierBase = 1.5f;
    private float critMultiplierModifier = 0f;


    public PossibilityPool()
    {
        // Set default base chances
        baseChances[StatusChanceType.Bleed] = 0.0f;
        baseChances[StatusChanceType.Poison] = 0.0f;
        baseChances[StatusChanceType.Critical] = 0.0f;
    }

    public void SetBaseChance(StatusChanceType type, float chance)
    {
        baseChances[type] = Mathf.Clamp01(chance);
    }

    public void AddModifier(StatusChanceType type, float amount)
    {
        if (!modifiers.ContainsKey(type))
            modifiers[type] = 0f;

        modifiers[type] += amount;
    }

    public float GetFinalChance(StatusChanceType type)
    {
        float baseChance = baseChances.TryGetValue(type, out var baseVal) ? baseVal : 0f;
        float modifier = modifiers.TryGetValue(type, out var modVal) ? modVal : 0f;
        return Mathf.Clamp01(baseChance + modifier);
    }

    public bool Roll(StatusChanceType type)
    {
        float chance = GetFinalChance(type);
        return Random.value <= chance;
    }

    public Dictionary<StatusChanceType, float> GetAllChances()
    {
        Dictionary<StatusChanceType, float> result = new();
        foreach (var kvp in baseChances)
        {
            result[kvp.Key] = GetFinalChance(kvp.Key);
        }
        return result;
    }

    public void SetBaseCriticalMultiplier(float value)
    {
        critMultiplierBase = Mathf.Max(1f, value);
    }

    public void AddCriticalMultiplierBonus(float value)
    {
        critMultiplierModifier += value;
    }

    public float GetFinalCriticalMultiplier()
    {
        float final = critMultiplierBase + critMultiplierModifier;
        Debug.Log($"💥 GetFinalCriticalMultiplier: {critMultiplierBase} + {critMultiplierModifier} = {final}");
        return final;
    }



}
