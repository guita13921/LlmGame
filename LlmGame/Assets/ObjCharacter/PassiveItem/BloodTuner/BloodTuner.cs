using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodTuner : MonoBehaviour, IPassiveItem, IStatusEffectListener
{
    public void ApplyEffect(Character character) { }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        throw new System.NotImplementedException();
    }

    public bool ShouldSpreadBleed(Character character)
    {
        return true; // Always true when this is equipped
    }
}
