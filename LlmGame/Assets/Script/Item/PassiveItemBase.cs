using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveItemBase : MonoBehaviour, IPassiveItem
{
    public abstract void ApplyEffect(Character character);

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        throw new System.NotImplementedException();
    }

    public void OnPlayerDamaged(Character character, int rawDamage, int finalDamage)
    {
        throw new System.NotImplementedException();
    }
}
